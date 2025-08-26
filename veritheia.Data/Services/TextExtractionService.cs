using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Veritheia.Data.Services;

/// <summary>
/// Platform Service: Text Extraction
/// MVP 2.2.2: Text Extraction Service
/// </summary>
public class TextExtractionService
{
    private readonly ILogger<TextExtractionService> _logger;

    public TextExtractionService(ILogger<TextExtractionService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Extract text from various document formats
    /// </summary>
    public async Task<string> ExtractTextAsync(Stream content, string mimeType)
    {
        return mimeType switch
        {
            "text/plain" => await ExtractFromTextAsync(content),
            "text/markdown" => await ExtractFromTextAsync(content),
            "text/csv" => await ExtractFromTextAsync(content),
            "application/pdf" => await ExtractFromPdfAsync(content),
            _ => throw new NotSupportedException($"MIME type {mimeType} not supported for text extraction")
        };
    }

    private async Task<string> ExtractFromTextAsync(Stream content)
    {
        using var reader = new StreamReader(content, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    private async Task<string> ExtractFromPdfAsync(Stream content)
    {
        return await Task.Run(() =>
        {
            try
            {
                var textBuilder = new StringBuilder();

                // Load the PDF document
                using (var document = PdfDocument.Open(content))
                {
                    _logger.LogInformation("Extracting text from PDF with {PageCount} pages", document.NumberOfPages);

                    // Extract text from each page
                    foreach (var page in document.GetPages())
                    {
                        // Use ContentOrderTextExtractor for better layout preservation
                        var pageText = ContentOrderTextExtractor.GetText(page);

                        if (!string.IsNullOrWhiteSpace(pageText))
                        {
                            textBuilder.AppendLine(pageText);
                            textBuilder.AppendLine(); // Add blank line between pages
                        }
                    }

                    // Extract document metadata if available
                    if (document.Information != null)
                    {
                        var info = document.Information;
                        var metadataBuilder = new StringBuilder();

                        if (!string.IsNullOrEmpty(info.Title))
                            metadataBuilder.AppendLine($"Title: {info.Title}");
                        if (!string.IsNullOrEmpty(info.Author))
                            metadataBuilder.AppendLine($"Author: {info.Author}");
                        if (!string.IsNullOrEmpty(info.Subject))
                            metadataBuilder.AppendLine($"Subject: {info.Subject}");
                        if (!string.IsNullOrEmpty(info.Keywords))
                            metadataBuilder.AppendLine($"Keywords: {info.Keywords}");

                        if (metadataBuilder.Length > 0)
                        {
                            textBuilder.Insert(0, "--- Document Metadata ---\n" + metadataBuilder.ToString() + "\n--- Document Content ---\n");
                        }
                    }
                }

                var extractedText = textBuilder.ToString().Trim();

                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    _logger.LogWarning("No text could be extracted from PDF");
                    return "No text content found in PDF document.";
                }

                _logger.LogInformation("Successfully extracted {CharCount} characters from PDF", extractedText.Length);
                return extractedText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract text from PDF");
                throw new InvalidOperationException("Failed to extract text from PDF document", ex);
            }
        });
    }
}