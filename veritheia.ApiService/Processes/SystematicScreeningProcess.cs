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
using Veritheia.Data;
using Veritheia.Data.Entities;
using Veritheia.Data.Services;
using Veritheia.ApiService.Services;

namespace Veritheia.ApiService.Processes;

/// <summary>
/// Systematic Screening Process implementing LLAssist methodology
/// Processes documents from user corpus with dual-phase assessment
/// </summary>
public class SystematicScreeningProcess : IAnalyticalProcess
{
    private readonly ILogger<SystematicScreeningProcess> _logger;
    private readonly IServiceProvider _serviceProvider;

    public SystematicScreeningProcess(
        ILogger<SystematicScreeningProcess> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public string ProcessId => "systematic-screening";
    public string Name => "Systematic Literature Screening";
    public string Description => "LLAssist methodology for systematic literature review with dual assessment";
    public string Category => "Research";

    public InputDefinition GetInputDefinition()
    {
        return new InputDefinition()
            .AddTextArea("research_questions", "Research questions (one per line)", true)
            .AddMultiSelect("document_ids", "Select documents from corpus", new string[] { }, true)
            .AddTextInput("relevance_threshold", "Relevance threshold (0.0-1.0, default: 0.7)", false)
            .AddTextInput("contribution_threshold", "Contribution threshold (0.0-1.0, default: 0.7)", false);
    }

    public bool ValidateInputs(ProcessContext context)
    {
        if (!context.Inputs.ContainsKey("research_questions"))
        {
            _logger.LogError("Missing required input: research_questions");
            return false;
        }

        if (!context.Inputs.ContainsKey("document_ids"))
        {
            _logger.LogError("Missing required input: document_ids");
            return false;
        }

        return true;
    }

    public async Task<AnalyticalProcessResult> ExecuteAsync(
        ProcessContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing systematic screening for journey {JourneyId}", context.JourneyId);

        try
        {
            // Parse research questions
            var questionsText = context.Inputs["research_questions"].ToString();
            var researchQuestions = questionsText?.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(q => q.Trim())
                .Where(q => !string.IsNullOrWhiteSpace(q))
                .ToList() ?? new List<string>();

            if (!researchQuestions.Any())
            {
                throw new InvalidOperationException("No research questions provided");
            }

            // Parse document IDs
            var documentIdsJson = context.Inputs["document_ids"].ToString();
            var documentIds = JsonSerializer.Deserialize<List<Guid>>(documentIdsJson ?? "[]") ?? new List<Guid>();

            if (!documentIds.Any())
            {
                throw new InvalidOperationException("No documents selected from corpus");
            }

            // Parse thresholds
            var relevanceThreshold = context.Inputs.ContainsKey("relevance_threshold") 
                ? Convert.ToSingle(context.Inputs["relevance_threshold"]) 
                : 0.7f;
            var contributionThreshold = context.Inputs.ContainsKey("contribution_threshold")
                ? Convert.ToSingle(context.Inputs["contribution_threshold"])
                : 0.7f;

            _logger.LogInformation("Processing {DocCount} documents with {RQCount} research questions",
                documentIds.Count, researchQuestions.Count);

            // Get services
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<VeritheiaDbContext>();
            var documentService = scope.ServiceProvider.GetRequiredService<DocumentService>();
            var semanticExtraction = scope.ServiceProvider.GetRequiredService<SemanticExtractionService>();
            var cognitiveAdapter = scope.ServiceProvider.GetRequiredService<ICognitiveAdapter>();
            var csvWriter = scope.ServiceProvider.GetRequiredService<CsvWriterService>();

            // Fetch documents with metadata from corpus
            var documents = await documentService.GetDocumentsByIdsAsync(documentIds, context.UserId);
            
            if (!documents.Any())
            {
                throw new InvalidOperationException("No documents found in corpus for the selected IDs");
            }

            var screeningResults = new List<ScreeningResult>();
            var processedCount = 0;

            // Process each document
            foreach (var document in documents)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                processedCount++;
                _logger.LogInformation("Processing document {Index}/{Total}: {Title}",
                    processedCount, documents.Count, 
                    document.Metadata?.Title ?? document.FileName);

                // Skip documents without metadata or abstract
                if (document.Metadata?.Abstract == null)
                {
                    _logger.LogWarning("Skipping document {DocumentId} - no abstract available", document.Id);
                    continue;
                }

                // Phase 1: Extract key semantics from abstract
                var semantics = await semanticExtraction.ExtractSemanticsAsync(document.Metadata.Abstract);

                var screeningResult = new ScreeningResult
                {
                    DocumentId = document.Id,
                    Title = document.Metadata.Title ?? document.FileName,
                    Abstract = document.Metadata.Abstract,
                    Authors = string.Join("; ", document.Metadata.Authors ?? Array.Empty<string>()),
                    Year = document.Metadata.PublicationDate?.Year,
                    Venue = document.Metadata.Publisher ?? "",
                    DOI = document.Metadata.DOI ?? "",
                    Link = document.Metadata.ExtendedMetadata?.GetValueOrDefault("link")?.ToString() ?? "",
                    Topics = semantics.Topics,
                    Entities = semantics.Entities,
                    Keywords = semantics.Keywords
                        .Concat(document.Metadata.Keywords ?? Array.Empty<string>())
                        .Distinct()
                        .ToList()
                };

                // Phase 2: Assess against each research question
                for (int rqIndex = 0; rqIndex < researchQuestions.Count; rqIndex++)
                {
                    var researchQuestion = researchQuestions[rqIndex];

                    var assessment = await AssessDocumentForResearchQuestion(
                        document.Metadata, researchQuestion, cognitiveAdapter, rqIndex);

                    screeningResult.RQAssessments.Add(assessment);
                }

                // Determine must-read based on thresholds
                screeningResult.MustRead = screeningResult.RQAssessments.Any(a =>
                    a.RelevanceScore >= relevanceThreshold && 
                    a.ContributionScore >= contributionThreshold);

                screeningResults.Add(screeningResult);

                // Update progress (if we had a progress callback)
                var progress = (processedCount * 100) / documents.Count;
                _logger.LogInformation("Progress: {Progress}% ({Processed}/{Total})",
                    progress, processedCount, documents.Count);
            }

            // Generate CSV output - convert to Data layer's ScreeningResult type
            var csvScreeningResults = screeningResults.Select(r => new Veritheia.Data.Services.ScreeningResult
            {
                Title = r.Title,
                Abstract = r.Abstract,
                Authors = r.Authors,
                Year = r.Year,
                Venue = r.Venue,
                DOI = r.DOI,
                Link = r.Link,
                Topics = r.Topics,
                Entities = r.Entities,
                Keywords = r.Keywords,
                RQAssessments = r.RQAssessments.Select(a => new Veritheia.Data.Services.RQAssessment
                {
                    QuestionIndex = a.QuestionIndex,
                    RelevanceScore = a.RelevanceScore,
                    ContributionScore = a.ContributionScore,
                    RelevanceIndicator = a.RelevanceIndicator,
                    ContributionIndicator = a.ContributionIndicator,
                    RelevanceReasoning = a.RelevanceReasoning,
                    ContributionReasoning = a.ContributionReasoning
                }).ToList(),
                MustRead = r.MustRead
            }).ToList();
            
            var csvOutput = csvWriter.WriteToCsv(csvScreeningResults, researchQuestions);

            // Create summary
            var mustReadCount = screeningResults.Count(r => r.MustRead);
            var summary = new Dictionary<string, object>
            {
                ["total_documents"] = documents.Count,
                ["processed_documents"] = screeningResults.Count,
                ["must_read_count"] = mustReadCount,
                ["must_read_percentage"] = screeningResults.Count > 0 
                    ? (double)mustReadCount / screeningResults.Count * 100 
                    : 0,
                ["research_questions"] = researchQuestions,
                ["relevance_threshold"] = relevanceThreshold,
                ["contribution_threshold"] = contributionThreshold,
                ["csv_output"] = Convert.ToBase64String(csvOutput),
                ["results"] = screeningResults.Select(r => new Dictionary<string, object>
                {
                    ["document_id"] = r.DocumentId,
                    ["title"] = r.Title,
                    ["authors"] = r.Authors,
                    ["year"] = r.Year ?? (object)DBNull.Value,
                    ["must_read"] = r.MustRead,
                    ["topics"] = r.Topics,
                    ["entities"] = r.Entities,
                    ["keywords"] = r.Keywords,
                    ["assessments"] = r.RQAssessments.Select(a => new Dictionary<string, object>
                    {
                        ["question_index"] = a.QuestionIndex,
                        ["relevance_score"] = a.RelevanceScore,
                        ["contribution_score"] = a.ContributionScore,
                        ["relevance_indicator"] = a.RelevanceIndicator,
                        ["contribution_indicator"] = a.ContributionIndicator,
                        ["relevance_reasoning"] = a.RelevanceReasoning,
                        ["contribution_reasoning"] = a.ContributionReasoning
                    }).ToList()
                }).ToList()
            };

            _logger.LogInformation("Screening complete: {Total} documents, {MustRead} must-read ({Percentage:F1}%)",
                screeningResults.Count, mustReadCount, 
                (double)mustReadCount / screeningResults.Count * 100);

            return new AnalyticalProcessResult
            {
                Success = true,
                Data = summary,
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

    private async Task<RQAssessment> AssessDocumentForResearchQuestion(
        DocumentMetadata metadata,
        string researchQuestion,
        ICognitiveAdapter cognitiveAdapter,
        int questionIndex)
    {
        // Create assessment prompts
        var relevancePrompt = CreateRelevancePrompt(metadata, researchQuestion);
        var contributionPrompt = CreateContributionPrompt(metadata, researchQuestion);

        // Get assessments from LLM
        var relevanceResponse = await cognitiveAdapter.GenerateTextAsync(relevancePrompt);
        var contributionResponse = await cognitiveAdapter.GenerateTextAsync(contributionPrompt);

        // Parse responses
        var relevanceAssessment = ParseAssessmentResponse(relevanceResponse);
        var contributionAssessment = ParseAssessmentResponse(contributionResponse);

        return new RQAssessment
        {
            QuestionIndex = questionIndex,
            RelevanceScore = relevanceAssessment.Score,
            ContributionScore = contributionAssessment.Score,
            RelevanceIndicator = relevanceAssessment.Score >= 0.7f,
            ContributionIndicator = contributionAssessment.Score >= 0.7f,
            RelevanceReasoning = relevanceAssessment.Reasoning,
            ContributionReasoning = contributionAssessment.Reasoning
        };
    }

    private string CreateRelevancePrompt(DocumentMetadata metadata, string researchQuestion)
    {
        return $@"Assess the RELEVANCE of this research paper to the research question. 
Relevance means: Does this paper discuss topics related to the research question?

Research Question: {researchQuestion}

Paper Title: {metadata.Title}
Abstract: {metadata.Abstract}

Provide your assessment as a score from 0.0 to 1.0 and reasoning:
- 0.0-0.3: Not relevant - paper does not discuss related topics
- 0.4-0.6: Somewhat relevant - paper mentions related concepts
- 0.7-1.0: Highly relevant - paper directly discusses related topics

Format your response as:
Score: [0.0-1.0]
Reasoning: [Your explanation of why this score was assigned]";
    }

    private string CreateContributionPrompt(DocumentMetadata metadata, string researchQuestion)
    {
        return $@"Assess the CONTRIBUTION of this research paper to the research question.
Contribution means: Does this paper directly research and provide findings for the research question?

Research Question: {researchQuestion}

Paper Title: {metadata.Title}
Abstract: {metadata.Abstract}

Provide your assessment as a score from 0.0 to 1.0 and reasoning:
- 0.0-0.3: No contribution - paper does not research this question
- 0.4-0.6: Limited contribution - paper provides some relevant findings
- 0.7-1.0: Strong contribution - paper directly researches this question with clear findings

Format your response as:
Score: [0.0-1.0]
Reasoning: [Your explanation of why this score was assigned]";
    }

    private (float Score, string Reasoning) ParseAssessmentResponse(string response)
    {
        var score = 0.0f;
        var reasoning = response;

        try
        {
            // Look for score pattern
            var scoreMatch = System.Text.RegularExpressions.Regex.Match(
                response, @"Score:\s*([0-9]*\.?[0-9]+)");
            
            if (scoreMatch.Success && float.TryParse(scoreMatch.Groups[1].Value, out var parsedScore))
            {
                score = Math.Clamp(parsedScore, 0.0f, 1.0f);
            }

            // Look for reasoning pattern
            var reasoningMatch = System.Text.RegularExpressions.Regex.Match(
                response, @"Reasoning:\s*(.+)", 
                System.Text.RegularExpressions.RegexOptions.Singleline);
            
            if (reasoningMatch.Success)
            {
                reasoning = reasoningMatch.Groups[1].Value.Trim();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse assessment response");
        }

        return (score, reasoning);
    }

    public Dictionary<string, object> GetCapabilities()
    {
        return new Dictionary<string, object>
        {
            ["supports_batch"] = true,
            ["supports_streaming"] = false,
            ["requires_llm"] = true,
            ["processes_corpus"] = true
        };
    }
}

// Result classes
public class ScreeningResult
{
    public Guid DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Abstract { get; set; } = string.Empty;
    public string Authors { get; set; } = string.Empty;
    public int? Year { get; set; }
    public string Venue { get; set; } = string.Empty;
    public string DOI { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public List<string> Topics { get; set; } = new();
    public List<string> Entities { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
    public List<RQAssessment> RQAssessments { get; set; } = new();
    public bool MustRead { get; set; }
}

public class RQAssessment
{
    public int QuestionIndex { get; set; }
    public float RelevanceScore { get; set; }
    public float ContributionScore { get; set; }
    public bool RelevanceIndicator { get; set; }
    public bool ContributionIndicator { get; set; }
    public string RelevanceReasoning { get; set; } = string.Empty;
    public string ContributionReasoning { get; set; } = string.Empty;
}