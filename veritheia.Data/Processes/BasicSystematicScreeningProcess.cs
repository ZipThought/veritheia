using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Veritheia.Core.Interfaces;
using Veritheia.Core.ValueObjects;
using Veritheia.Data.Entities;

namespace Veritheia.Data.Processes;

/// <summary>
/// Basic Systematic Screening Process
/// Screens documents against research questions
/// </summary>
public class BasicSystematicScreeningProcess : IAnalyticalProcess
{
    private readonly ILogger<BasicSystematicScreeningProcess> _logger;
    private readonly IServiceProvider _serviceProvider;
    
    public BasicSystematicScreeningProcess(
        ILogger<BasicSystematicScreeningProcess> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    
    public string ProcessId => "basic-systematic-screening";
    public string Name => "Basic Systematic Screening";
    public string Description => "Screen documents against research questions";
    public string Category => "Research";
    
    public InputDefinition GetInputDefinition()
    {
        return new InputDefinition()
            .AddTextArea("research_questions", "List of research questions (one per line)", true)
            .AddTextArea("screening_criteria", "Screening criteria in JSON format", true)
            .AddDocumentSelector("documents", "Documents to screen", false);
    }
    
    public bool ValidateInputs(ProcessContext context)
    {
        if (!context.Inputs.ContainsKey("research_questions"))
        {
            _logger.LogError("Missing required input: research_questions");
            return false;
        }
        
        if (!context.Inputs.ContainsKey("screening_criteria"))
        {
            _logger.LogError("Missing required input: screening_criteria");
            return false;
        }
        
        return true;
    }
    
    public async Task<AnalyticalProcessResult> ExecuteAsync(
        ProcessContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing basic systematic screening for journey {JourneyId}", 
            context.JourneyId);
        
        try
        {
            // Parse research questions
            var questionsText = context.Inputs["research_questions"].ToString();
            var researchQuestions = questionsText?.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(q => q.Trim())
                .Where(q => !string.IsNullOrWhiteSpace(q))
                .ToList() ?? new List<string>();
            
            // Get services
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<VeritheiaDbContext>();
            var cognitiveAdapter = scope.ServiceProvider.GetRequiredService<ICognitiveAdapter>();
            
            // Get journey segments with documents
            var segments = await dbContext.JourneyDocumentSegments
                .Include(jds => jds.Document)
                .Where(jds => jds.JourneyId == context.JourneyId)
                .ToListAsync(cancellationToken);
            
            // Get unique documents
            var documents = segments
                .Select(jds => jds.Document)
                .DistinctBy(d => d.Id)
                .ToList();
            
            var screeningResults = new List<Dictionary<string, object>>();
            
            foreach (var document in documents)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                
                // Get document segments for content preview
                var docSegments = segments
                    .Where(jds => jds.DocumentId == document.Id)
                    .Take(3)
                    .Select(jds => jds.SegmentContent)
                    .ToList();
                
                var contentPreview = docSegments.Any() 
                    ? string.Join("\n", docSegments) 
                    : "No content available";
                
                // Create simple screening prompt
                var prompt = $@"Screen this document against these research questions:
{string.Join("\n", researchQuestions.Select((q, i) => $"{i + 1}. {q}"))}

Document: {document.FileName}
Content preview: {contentPreview.Substring(0, Math.Min(500, contentPreview.Length))}

Answer with INCLUDE or EXCLUDE and a brief reason.";
                
                // Use LLM to evaluate
                var llmResponse = await cognitiveAdapter.GenerateTextAsync(prompt);
                
                var decision = new Dictionary<string, object>
                {
                    ["document_id"] = document.Id,
                    ["document_name"] = document.FileName,
                    ["decision"] = llmResponse.Contains("INCLUDE", StringComparison.OrdinalIgnoreCase) ? "INCLUDE" : "EXCLUDE",
                    ["reasoning"] = llmResponse
                };
                
                screeningResults.Add(decision);
            }
            
            var results = new Dictionary<string, object>
            {
                ["status"] = "completed",
                ["screened_count"] = screeningResults.Count,
                ["screening_decisions"] = screeningResults,
                ["research_questions"] = researchQuestions
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
            _logger.LogError(ex, "Systematic screening failed");
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
            ["supports_batch"] = true,
            ["supports_streaming"] = false,
            ["requires_llm"] = true
        };
    }
}