using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;

namespace Veritheia.Data.Services;

/// <summary>
/// Service for parsing CSV files with bibliographic data
/// Supports both Scopus and IEEE Xplore formats
/// </summary>
public class CsvParserService
{
    private readonly ILogger<CsvParserService> _logger;

    public CsvParserService(ILogger<CsvParserService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Parse CSV content into article records
    /// </summary>
    public List<ArticleRecord> ParseCsv(Stream csvStream)
    {
        var articles = new List<ArticleRecord>();

        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null, // Ignore missing fields
            HeaderValidated = null,   // Don't validate headers
            TrimOptions = TrimOptions.Trim
        });

        // Read header to detect format
        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord?.ToList() ?? new List<string>();
        
        var format = DetectFormat(headers);
        _logger.LogInformation("Detected CSV format: {Format}", format);

        // Read records
        while (csv.Read())
        {
            try
            {
                var article = format switch
                {
                    CsvFormat.Scopus => ParseScopusRecord(csv),
                    CsvFormat.IeeeXplore => ParseIeeeRecord(csv),
                    _ => ParseGenericRecord(csv, headers)
                };

                if (article != null)
                {
                    articles.Add(article);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse CSV row {Row}", csv.Context.Parser.Row);
            }
        }

        _logger.LogInformation("Parsed {Count} articles from CSV", articles.Count);
        return articles;
    }

    private CsvFormat DetectFormat(List<string> headers)
    {
        // Check for Scopus-specific columns
        if (headers.Contains("Authors") && headers.Contains("Source title") && headers.Contains("EID"))
        {
            return CsvFormat.Scopus;
        }

        // Check for IEEE Xplore-specific columns
        if (headers.Contains("Document Title") && headers.Contains("Publication Title") && headers.Contains("Document Identifier"))
        {
            return CsvFormat.IeeeXplore;
        }

        return CsvFormat.Generic;
    }

    private ArticleRecord? ParseScopusRecord(CsvReader csv)
    {
        var title = csv.GetField("Title");
        var abstractText = csv.GetField("Abstract");

        // Skip if no title or abstract
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(abstractText))
            return null;

        return new ArticleRecord
        {
            Title = title,
            Abstract = abstractText,
            Authors = csv.GetField("Authors") ?? string.Empty,
            Year = ParseYear(csv.GetField("Year")),
            Venue = csv.GetField("Source title") ?? string.Empty,
            DOI = csv.GetField("DOI") ?? string.Empty,
            Link = csv.GetField("Link") ?? string.Empty,
            Keywords = CombineKeywords(
                csv.GetField("Author Keywords"),
                csv.GetField("Index Keywords")
            )
        };
    }

    private ArticleRecord? ParseIeeeRecord(CsvReader csv)
    {
        var title = csv.GetField("Document Title");
        var abstractText = csv.GetField("Abstract");

        // Skip if no title or abstract
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(abstractText))
            return null;

        return new ArticleRecord
        {
            Title = title,
            Abstract = abstractText,
            Authors = csv.GetField("Authors") ?? string.Empty,
            Year = ParseYear(csv.GetField("Publication Year")),
            Venue = csv.GetField("Publication Title") ?? string.Empty,
            DOI = csv.GetField("DOI") ?? string.Empty,
            Link = csv.GetField("PDF Link") ?? string.Empty,
            Keywords = CombineKeywords(
                csv.GetField("Author Keywords"),
                csv.GetField("IEEE Terms")
            )
        };
    }

    private ArticleRecord? ParseGenericRecord(CsvReader csv, List<string> headers)
    {
        // Try to map common column variations
        var titleField = FindField(headers, "title", "document title", "paper title");
        var abstractField = FindField(headers, "abstract", "summary");
        var authorsField = FindField(headers, "authors", "author", "author names");
        var yearField = FindField(headers, "year", "publication year", "date");
        var venueField = FindField(headers, "venue", "source", "journal", "publication title", "source title");
        var doiField = FindField(headers, "doi");
        var linkField = FindField(headers, "link", "url", "pdf link");

        var title = titleField != null ? csv.GetField(titleField) : null;
        var abstractText = abstractField != null ? csv.GetField(abstractField) : null;

        // Skip if no title or abstract
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(abstractText))
            return null;

        return new ArticleRecord
        {
            Title = title,
            Abstract = abstractText,
            Authors = authorsField != null ? csv.GetField(authorsField) ?? string.Empty : string.Empty,
            Year = yearField != null ? ParseYear(csv.GetField(yearField)) : null,
            Venue = venueField != null ? csv.GetField(venueField) ?? string.Empty : string.Empty,
            DOI = doiField != null ? csv.GetField(doiField) ?? string.Empty : string.Empty,
            Link = linkField != null ? csv.GetField(linkField) ?? string.Empty : string.Empty,
            Keywords = string.Empty
        };
    }

    private string? FindField(List<string> headers, params string[] candidates)
    {
        foreach (var candidate in candidates)
        {
            var match = headers.FirstOrDefault(h => 
                h.Equals(candidate, StringComparison.OrdinalIgnoreCase));
            if (match != null)
                return match;
        }
        return null;
    }

    private int? ParseYear(string? yearText)
    {
        if (string.IsNullOrWhiteSpace(yearText))
            return null;

        if (int.TryParse(yearText, out var year))
            return year;

        // Try to extract year from date strings
        if (DateTime.TryParse(yearText, out var date))
            return date.Year;

        return null;
    }

    private string CombineKeywords(params string?[] keywordFields)
    {
        var allKeywords = keywordFields
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .SelectMany(k => k!.Split(';', ','))
            .Select(k => k.Trim())
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Distinct()
            .ToList();

        return string.Join("; ", allKeywords);
    }
}

/// <summary>
/// Parsed article record from CSV
/// </summary>
public class ArticleRecord
{
    public string Title { get; set; } = string.Empty;
    public string Abstract { get; set; } = string.Empty;
    public string Authors { get; set; } = string.Empty;
    public int? Year { get; set; }
    public string Venue { get; set; } = string.Empty;
    public string DOI { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public string Keywords { get; set; } = string.Empty;
}

/// <summary>
/// Detected CSV format
/// </summary>
public enum CsvFormat
{
    Generic,
    Scopus,
    IeeeXplore
}
