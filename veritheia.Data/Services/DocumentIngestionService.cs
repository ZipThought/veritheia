using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Veritheia.Core.Interfaces;
using Veritheia.Data;
using Veritheia.Data.Entities;

namespace Veritheia.Data.Services;

/// <summary>
/// Platform Service: Document Ingestion Pipeline
/// MVP 2.2.1: Document Ingestion
/// </summary>
public class DocumentIngestionService
{
    private readonly VeritheiaDbContext _db;
    private readonly DocumentService _documentService;
    private readonly TextExtractionService _textExtraction;
    private readonly EmbeddingService _embedding;
    private readonly ILogger<DocumentIngestionService> _logger;
    
    public DocumentIngestionService(
        VeritheiaDbContext dbContext,
        DocumentService documentService,
        TextExtractionService textExtraction,
        EmbeddingService embedding,
        ILogger<DocumentIngestionService> logger)
    {
        _db = dbContext;
        _documentService = documentService;
        _textExtraction = textExtraction;
        _embedding = embedding;
        _logger = logger;
    }
    
    /// <summary>
    /// Ingest a document into a journey's projection space
    /// </summary>
    public async Task<IngestionResult> IngestDocumentAsync(
        Stream content,
        string fileName,
        string mimeType,
        Guid userId,
        Guid journeyId,
        IngestionOptions? options = null)
    {
        options ??= new IngestionOptions();
        
        _logger.LogInformation("Starting ingestion of {FileName} for journey {JourneyId}", 
            fileName, journeyId);
        
        // Upload document
        var document = await _documentService.UploadDocumentAsync(
            content, fileName, mimeType, userId, options.ScopeId);
        
        // Extract text
        content.Position = 0;
        var extractedText = await _textExtraction.ExtractTextAsync(content, mimeType);
        
        if (string.IsNullOrEmpty(extractedText))
        {
            _logger.LogWarning("No text extracted from {FileName}", fileName);
            return new IngestionResult 
            { 
                DocumentId = document.Id, 
                Success = false, 
                Error = "No text content extracted" 
            };
        }
        
        // Extract metadata
        var metadata = ExtractMetadata(extractedText, fileName);
        
        // Store metadata
        var documentMetadata = new DocumentMetadata
        {
            Id = Guid.CreateVersion7(),
            DocumentId = document.Id,
            Title = metadata.Title,
            Authors = new List<string> { metadata.Author },
            PublicationDate = metadata.PublicationDate,
            ExtendedMetadata = new Dictionary<string, object> 
            { 
                ["abstract"] = metadata.Abstract,
                ["keywords"] = metadata.Keywords
            },
            CreatedAt = DateTime.UtcNow
        };
        
        _db.DocumentMetadata.Add(documentMetadata);
        
        // Chunk document for journey
        var chunks = ChunkDocument(extractedText, options);
        
        // Create journey segments
        var segments = new List<JourneyDocumentSegment>();
        for (int i = 0; i < chunks.Count; i++)
        {
            var segment = new JourneyDocumentSegment
            {
                Id = Guid.CreateVersion7(),
                JourneyId = journeyId,
                DocumentId = document.Id,
                SegmentContent = chunks[i].Content,
                SegmentType = chunks[i].Type,
                SegmentPurpose = options.SegmentPurpose ?? "General",
                SequenceIndex = i,
                CreatedAt = DateTime.UtcNow
            };
            
            segments.Add(segment);
            _db.JourneyDocumentSegments.Add(segment);
        }
        
        await _db.SaveChangesAsync();
        
        // Generate embeddings (async, don't block)
        _ = Task.Run(async () => await GenerateEmbeddingsAsync(segments));
        
        _logger.LogInformation("Ingested {FileName} with {SegmentCount} segments", 
            fileName, segments.Count);
        
        return new IngestionResult
        {
            DocumentId = document.Id,
            Success = true,
            SegmentCount = segments.Count,
            Metadata = metadata
        };
    }
    
    private MetadataInfo ExtractMetadata(string text, string fileName)
    {
        // Simple metadata extraction (would be enhanced with AI)
        var lines = text.Split('\n').Take(50).ToArray();
        
        return new MetadataInfo
        {
            Title = ExtractTitle(lines, fileName),
            Author = ExtractAuthor(lines),
            Abstract = ExtractAbstract(text),
            Keywords = ExtractKeywords(text)
        };
    }
    
    private string ExtractTitle(string[] lines, string fallback)
    {
        // Look for title patterns
        var titleLine = lines.FirstOrDefault(l => 
            l.Length > 10 && l.Length < 200 && 
            !l.StartsWith("by", StringComparison.OrdinalIgnoreCase));
        
        return titleLine ?? Path.GetFileNameWithoutExtension(fallback);
    }
    
    private string ExtractAuthor(string[] lines)
    {
        // Look for author patterns
        var authorLine = lines.FirstOrDefault(l => 
            l.StartsWith("by", StringComparison.OrdinalIgnoreCase) ||
            l.Contains("Author:", StringComparison.OrdinalIgnoreCase));
        
        return authorLine?.Replace("by", "", StringComparison.OrdinalIgnoreCase).Trim() ?? "Unknown";
    }
    
    private string ExtractAbstract(string text)
    {
        // Look for abstract section
        var abstractIndex = text.IndexOf("abstract", StringComparison.OrdinalIgnoreCase);
        if (abstractIndex > 0)
        {
            var abstractText = text.Substring(abstractIndex, Math.Min(1000, text.Length - abstractIndex));
            return abstractText.Split('\n').Skip(1).FirstOrDefault()?.Trim() ?? "";
        }
        
        // Fallback to first paragraph
        return text.Split("\n\n").FirstOrDefault()?.Trim() ?? "";
    }
    
    private string[] ExtractKeywords(string text)
    {
        // Simple keyword extraction (would be enhanced with AI)
        var commonWords = new HashSet<string> { "the", "and", "or", "but", "in", "on", "at", "to", "for" };
        
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 4 && !commonWords.Contains(w.ToLower()))
            .GroupBy(w => w.ToLower())
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => g.Key)
            .ToArray();
        
        return words;
    }
    
    private List<DocumentChunk> ChunkDocument(string text, IngestionOptions options)
    {
        var chunks = new List<DocumentChunk>();
        
        // Simple paragraph-based chunking
        var paragraphs = text.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var para in paragraphs)
        {
            if (para.Length < options.MinChunkSize)
                continue;
            
            if (para.Length > options.MaxChunkSize)
            {
                // Split large paragraphs
                var sentences = para.Split(". ");
                var currentChunk = new StringBuilder();
                
                foreach (var sentence in sentences)
                {
                    if (currentChunk.Length + sentence.Length > options.MaxChunkSize)
                    {
                        chunks.Add(new DocumentChunk
                        {
                            Content = currentChunk.ToString(),
                            Type = "paragraph"
                        });
                        currentChunk.Clear();
                    }
                    currentChunk.Append(sentence).Append(". ");
                }
                
                if (currentChunk.Length > 0)
                {
                    chunks.Add(new DocumentChunk
                    {
                        Content = currentChunk.ToString(),
                        Type = "paragraph"
                    });
                }
            }
            else
            {
                chunks.Add(new DocumentChunk
                {
                    Content = para,
                    Type = "paragraph"
                });
            }
        }
        
        return chunks;
    }
    
    private async Task GenerateEmbeddingsAsync(List<JourneyDocumentSegment> segments)
    {
        foreach (var segment in segments)
        {
            try
            {
                await _embedding.GenerateEmbeddingForSegmentAsync(segment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate embedding for segment {SegmentId}", segment.Id);
            }
        }
    }
}

public class IngestionOptions
{
    public Guid? ScopeId { get; set; }
    public string? SegmentPurpose { get; set; }
    public int MinChunkSize { get; set; } = 100;
    public int MaxChunkSize { get; set; } = 1000;
}

public class IngestionResult
{
    public Guid DocumentId { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int SegmentCount { get; set; }
    public MetadataInfo? Metadata { get; set; }
}

public class MetadataInfo
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime? PublicationDate { get; set; }
    public string Abstract { get; set; } = string.Empty;
    public string[] Keywords { get; set; } = Array.Empty<string>();
}

public class DocumentChunk
{
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}