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
/// Service for writing CSV files with dynamic columns based on research questions
/// </summary>
public class CsvWriterService
{
    private readonly ILogger<CsvWriterService> _logger;

    public CsvWriterService(ILogger<CsvWriterService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Write screening results to CSV with dynamic RQ columns
    /// </summary>
    public byte[] WriteToCsv(List<ScreeningResult> results, List<string> researchQuestions)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        });

        // Write headers
        WriteHeaders(csv, researchQuestions);

        // Write data rows
        foreach (var result in results)
        {
            WriteDataRow(csv, result, researchQuestions);
        }

        writer.Flush();
        return memoryStream.ToArray();
    }

    private void WriteHeaders(CsvWriter csv, List<string> researchQuestions)
    {
        // Fixed columns
        csv.WriteField("Authors");
        csv.WriteField("Year");
        csv.WriteField("Title");
        csv.WriteField("DOI");
        csv.WriteField("Link");
        csv.WriteField("Abstract");
        csv.WriteField("Topics");
        csv.WriteField("Entities");
        csv.WriteField("Keywords");
        csv.WriteField("MustRead");

        // Dynamic RQ columns
        for (int i = 0; i < researchQuestions.Count; i++)
        {
            var rqNum = i + 1;
            csv.WriteField($"RQ{rqNum}_Q");  // Research Question text
            csv.WriteField($"RQ{rqNum}_RI"); // Relevance Indicator (boolean)
            csv.WriteField($"RQ{rqNum}_CI"); // Contribution Indicator (boolean)
            csv.WriteField($"RQ{rqNum}_RS"); // Relevance Score (float)
            csv.WriteField($"RQ{rqNum}_CS"); // Contribution Score (float)
            csv.WriteField($"RQ{rqNum}_RR"); // Relevance Reasoning (text)
            csv.WriteField($"RQ{rqNum}_CR"); // Contribution Reasoning (text)
        }

        csv.NextRecord();
    }

    private void WriteDataRow(CsvWriter csv, ScreeningResult result, List<string> researchQuestions)
    {
        // Fixed columns
        csv.WriteField(result.Authors);
        csv.WriteField(result.Year?.ToString() ?? "");
        csv.WriteField(result.Title);
        csv.WriteField(result.DOI);
        csv.WriteField(result.Link);
        csv.WriteField(result.Abstract);
        csv.WriteField(string.Join(";", result.Topics));
        csv.WriteField(string.Join(";", result.Entities));
        csv.WriteField(string.Join(";", result.Keywords));
        csv.WriteField(result.MustRead.ToString());

        // Dynamic RQ columns
        for (int i = 0; i < researchQuestions.Count; i++)
        {
            var rqAssessment = result.RQAssessments.FirstOrDefault(a => a.QuestionIndex == i);

            if (rqAssessment != null)
            {
                csv.WriteField(researchQuestions[i]); // RQ text
                csv.WriteField(rqAssessment.RelevanceIndicator.ToString());
                csv.WriteField(rqAssessment.ContributionIndicator.ToString());
                csv.WriteField(rqAssessment.RelevanceScore.ToString("F1"));
                csv.WriteField(rqAssessment.ContributionScore.ToString("F1"));
                csv.WriteField(rqAssessment.RelevanceReasoning);
                csv.WriteField(rqAssessment.ContributionReasoning);
            }
            else
            {
                // Empty values if no assessment for this RQ
                csv.WriteField(researchQuestions[i]);
                csv.WriteField("False");
                csv.WriteField("False");
                csv.WriteField("0.0");
                csv.WriteField("0.0");
                csv.WriteField("");
                csv.WriteField("");
            }
        }

        csv.NextRecord();
    }
}

/// <summary>
/// Complete screening result for CSV output
/// </summary>
public class ScreeningResult
{
    public string Title { get; set; } = string.Empty;
    public string Abstract { get; set; } = string.Empty;
    public string Authors { get; set; } = string.Empty;
    public int? Year { get; set; }
    public string Venue { get; set; } = string.Empty;
    public string DOI { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;

    // Extracted semantics
    public List<string> Topics { get; set; } = new();
    public List<string> Entities { get; set; } = new();
    public List<string> Keywords { get; set; } = new();

    // Overall decision
    public bool MustRead { get; set; }

    // Per-RQ assessments
    public List<RQAssessment> RQAssessments { get; set; } = new();
}

/// <summary>
/// Assessment for a single research question
/// </summary>
public class RQAssessment
{
    public int QuestionIndex { get; set; }
    public bool RelevanceIndicator { get; set; }
    public bool ContributionIndicator { get; set; }
    public float RelevanceScore { get; set; }
    public float ContributionScore { get; set; }
    public string RelevanceReasoning { get; set; } = string.Empty;
    public string ContributionReasoning { get; set; } = string.Empty;
}
