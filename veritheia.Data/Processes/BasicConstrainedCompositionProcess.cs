using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pgvector;
using Veritheia.Core.Interfaces;
using Veritheia.Core.ValueObjects;
using Veritheia.Data.Entities;

namespace Veritheia.Data.Processes;

/// <summary>
/// Basic Constrained Composition Process
/// Generates documents using LLM with constraints
/// </summary>
public class BasicConstrainedCompositionProcess : IAnalyticalProcess
{
    private readonly ILogger<BasicConstrainedCompositionProcess> _logger;
    private readonly IServiceProvider _serviceProvider;
    
    public BasicConstrainedCompositionProcess(
        ILogger<BasicConstrainedCompositionProcess> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    
    public string ProcessId => "basic-constrained-composition";
    public string Name => "Basic Constrained Composition";
    public string Description => "Generate documents with specified constraints";
    public string Category => "Writing";
    
    public InputDefinition GetInputDefinition()
    {
        return new InputDefinition()
            .AddTextInput("document_type", "Type of document to generate", true)
            .AddTextArea("constraints", "Composition constraints (JSON)", true)
            .AddTextArea("outline", "Document outline", true);
    }
    
    public bool ValidateInputs(ProcessContext context)
    {
        if (!context.Inputs.ContainsKey("document_type"))
        {
            _logger.LogError("Missing required input: document_type");
            return false;
        }
        
        if (!context.Inputs.ContainsKey("constraints"))
        {
            _logger.LogError("Missing required input: constraints");
            return false;
        }
        
        if (!context.Inputs.ContainsKey("outline"))
        {
            _logger.LogError("Missing required input: outline");
            return false;
        }
        
        return true;
    }
    
    public async Task<AnalyticalProcessResult> ExecuteAsync(
        ProcessContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing basic constrained composition for journey {JourneyId}", 
            context.JourneyId);
        
        try
        {
            // Parse inputs
            var documentType = context.Inputs["document_type"].ToString();
            var constraintsText = context.Inputs["constraints"].ToString();
            var outlineText = context.Inputs["outline"].ToString();
            
            // Get services
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<VeritheiaDbContext>();
            var cognitiveAdapter = scope.ServiceProvider.GetRequiredService<ICognitiveAdapter>();
            
            // Create composition prompt
            var prompt = $@"Generate a {documentType} document with the following outline:

{outlineText}

Apply these constraints:
{constraintsText}

Generate the complete document in markdown format.";
            
            // Generate with LLM
            var generatedText = await cognitiveAdapter.GenerateTextAsync(
                prompt,
                "You are a professional writer. Follow the outline and constraints precisely.");
            
            // Get journey and user
            var journey = await dbContext.Journeys
                .Include(j => j.User)
                .FirstOrDefaultAsync(j => j.Id == context.JourneyId, cancellationToken);
            
            if (journey == null)
            {
                throw new InvalidOperationException($"Journey {context.JourneyId} not found");
            }
            
            // Save as new document
            var newDocument = new Document
            {
                Id = Guid.CreateVersion7(),
                UserId = journey.UserId,
                FileName = $"generated_{documentType?.Replace(" ", "_").ToLower() ?? "document"}_{DateTime.UtcNow:yyyyMMddHHmmss}.md",
                MimeType = "text/markdown",
                FilePath = $"generated/{journey.UserId}/{Guid.CreateVersion7()}.md",
                FileSize = Encoding.UTF8.GetByteCount(generatedText),
                UploadedAt = DateTime.UtcNow
            };
            
            dbContext.Documents.Add(newDocument);
            
            // Create journey document segment
            var segment = new JourneyDocumentSegment
            {
                Id = Guid.CreateVersion7(),
                JourneyId = context.JourneyId,
                DocumentId = newDocument.Id,
                SegmentContent = generatedText,
                SegmentType = "full",
                SequenceIndex = 0,
                CreatedByRule = "constrained-composition",
                CreatedForQuestion = "Generated content"
            };
            
            dbContext.JourneyDocumentSegments.Add(segment);
            await dbContext.SaveChangesAsync(cancellationToken);
            
            var wordCount = generatedText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            
            var results = new Dictionary<string, object>
            {
                ["status"] = "completed",
                ["document_id"] = newDocument.Id,
                ["document_type"] = documentType,
                ["word_count"] = wordCount,
                ["generated_text"] = generatedText
            };
            
            return new AnalyticalProcessResult
            {
                Success = true,
                Data = results,
                ErrorMessage = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Constrained composition failed");
            return new AnalyticalProcessResult
            {
                Success = false,
                Data = new Dictionary<string, object>(),
                ErrorMessage = ex.Message
            };
        }
    }
    
    public Dictionary<string, object> GetCapabilities()
    {
        return new Dictionary<string, object>
        {
            ["supports_batch"] = false,
            ["supports_streaming"] = true,
            ["requires_llm"] = true
        };
    }
}