using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Veritheia.Data.Services;
using Veritheia.Data.Entities;

namespace Veritheia.ApiService.Services;

/// <summary>
/// Service for importing documents into user corpus from various sources
/// </summary>
public class CorpusImportService
{
    private readonly DocumentMetadataService _metadataService;
    private readonly CsvParserService _csvParser;
    private readonly ILogger<CorpusImportService> _logger;

    public CorpusImportService(
        DocumentMetadataService metadataService,
        CsvParserService csvParser,
        ILogger<CorpusImportService> logger)
    {
        _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
        _csvParser = csvParser ?? throw new ArgumentNullException(nameof(csvParser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Import CSV file containing academic papers into user corpus
    /// </summary>
    public async Task<CorpusImportResult> ImportCsvAsync(
        Stream csvStream,
        Guid userId,
        string fileName)
    {
        _logger.LogInformation("Starting CSV import for user {UserId} from file {FileName}", 
            userId, fileName);
            
        // Parse CSV using existing parser
        var articles = _csvParser.ParseCsv(csvStream);

        if (!articles.Any())
        {
            throw new InvalidOperationException("No articles found in CSV file");
        }

        _logger.LogInformation("Parsed {Count} articles from CSV", articles.Count);

        // Convert to metadata DTOs
        var metadataList = articles.Select(article => new DocumentMetadataDto
        {
            Title = article.Title,
            Authors = ParseAuthors(article.Authors),
            Abstract = article.Abstract,
            Keywords = ParseKeywords(article.Keywords),
            Year = article.Year,
            Venue = article.Venue,
            DOI = article.DOI,
            Link = article.Link,
            PublicationDate = article.Year.HasValue 
                ? new DateTime(article.Year.Value, 1, 1) 
                : null
        }).ToList();

        // Batch create metadata in corpus
        var createdMetadata = await _metadataService.BatchCreateMetadataAsync(userId, metadataList);

        _logger.LogInformation("Successfully imported {Count} articles into corpus for user {UserId}", 
            createdMetadata.Count, userId);

        return new CorpusImportResult
        {
            Success = true,
            TotalArticles = articles.Count,
            ImportedArticles = createdMetadata.Count,
            ImportedDocumentIds = createdMetadata.Select(m => m.DocumentId).ToList(),
            Summary = GenerateImportSummary(createdMetadata)
        };
    }

    private string[] ParseAuthors(string authorsString)
    {
        if (string.IsNullOrWhiteSpace(authorsString))
            return new[] { "Unknown" };

        // Handle various author formats: semicolon, comma, "and"
        var separators = new[] { ";", ", and ", " and ", "," };
        var authors = authorsString.Split(separators, StringSplitOptions.RemoveEmptyEntries)
            .Select(a => a.Trim())
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .ToArray();

        return authors.Length > 0 ? authors : new[] { authorsString.Trim() };
    }

    private string[] ParseKeywords(string keywordsString)
    {
        if (string.IsNullOrWhiteSpace(keywordsString))
            return Array.Empty<string>();

        // Split by semicolon or comma
        return keywordsString.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(k => k.Trim())
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .ToArray();
    }

    private CorpusImportSummary GenerateImportSummary(List<DocumentMetadata> metadata)
    {
        var yearGroups = metadata
            .Where(m => m.ExtendedMetadata?.ContainsKey("year") == true)
            .GroupBy(m => m.ExtendedMetadata["year"])
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var venueGroups = metadata
            .Where(m => !string.IsNullOrWhiteSpace(m.Publisher))
            .GroupBy(m => m.Publisher)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .ToDictionary(g => g.Key!, g => g.Count());

        return new CorpusImportSummary
        {
            TotalDocuments = metadata.Count,
            DocumentsByYear = yearGroups,
            TopVenues = venueGroups,
            DocumentsWithAbstract = metadata.Count(m => !string.IsNullOrWhiteSpace(m.Abstract)),
            DocumentsWithKeywords = metadata.Count(m => m.Keywords?.Length > 0)
        };
    }
}

/// <summary>
/// Result of corpus import operation
/// </summary>
public class CorpusImportResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int TotalArticles { get; set; }
    public int ImportedArticles { get; set; }
    public List<Guid> ImportedDocumentIds { get; set; } = new();
    public CorpusImportSummary? Summary { get; set; }
}

/// <summary>
/// Summary statistics of imported corpus
/// </summary>
public class CorpusImportSummary
{
    public int TotalDocuments { get; set; }
    public Dictionary<string, int> DocumentsByYear { get; set; } = new();
    public Dictionary<string, int> TopVenues { get; set; } = new();
    public int DocumentsWithAbstract { get; set; }
    public int DocumentsWithKeywords { get; set; }
}