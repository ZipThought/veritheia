using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Veritheia.Core.Interfaces;
using Veritheia.Core.ValueObjects;
using Veritheia.Core.Models;
using Veritheia.Data.Entities;
using Veritheia.Data.Services;

namespace Veritheia.Data.Processes;

/// <summary>
/// Systematic Screening Process implementing LLAssist methodology
/// Dual-phase assessment: semantic extraction + per-RQ relevance/contribution evaluation
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
    
    public string ProcessId => "systematic-screening";
    public string Name => "Systematic Literature Screening";
    public string Description => "LLAssist methodology for systematic literature review with dual assessment";
    public string Category => "Research";
    
    public InputDefinition GetInputDefinition()
    {
        return new InputDefinition()
            .AddTextArea("research_questions", "Research questions (one per line)", true)
            .AddTextArea("csv_upload", "Upload CSV to add documents to corpus (optional)", false);
    }
    
    public bool ValidateInputs(ProcessContext context)
    {
        if (!context.Inputs.ContainsKey("research_questions"))
        {
            _logger.LogError("Missing required input: research_questions");
            return false;
        }
        
        // CSV file is optional - we process corpus documents
        
        return true;
    }
    
    public async Task<AnalyticalProcessResult> ExecuteAsync(
        ProcessContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing LLAssist systematic screening for journey {JourneyId}", 
            context.JourneyId);
        
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

            // Get services
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<VeritheiaDbContext>();
            var documentService = scope.ServiceProvider.GetRequiredService<DocumentService>();
            var csvParser = scope.ServiceProvider.GetRequiredService<CsvParserService>();
            var csvWriter = scope.ServiceProvider.GetRequiredService<CsvWriterService>();
            var semanticExtraction = scope.ServiceProvider.GetRequiredService<SemanticExtractionService>();
            var cognitiveAdapter = scope.ServiceProvider.GetRequiredService<ICognitiveAdapter>();

            // Check if CSV was provided to add to corpus
            if (context.Inputs.ContainsKey("csv_upload") && context.Inputs["csv_upload"] != null)
            {
                var csvContent = context.Inputs["csv_upload"].ToString();
                if (!string.IsNullOrWhiteSpace(csvContent))
                {
                    _logger.LogInformation("Adding CSV documents to corpus");
                    var csvBytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
                    using (var csvStream = new MemoryStream(csvBytes))
                    {
                        var csvArticles = csvParser.ParseCsv(csvStream);
                        foreach (var article in csvArticles)
                        {
                            // Add to corpus with deduplication by DOI
                            await documentService.AddDocumentToCorpusAsync(
                                context.UserId,
                                article.Title,
                                article.Abstract,
                                article.Authors,
                                article.DOI,
                                article.Year,
                                article.Venue,
                                article.Keywords);
                        }
                        _logger.LogInformation("Added {Count} documents to corpus", csvArticles.Count);
                    }
                }
            }

            // Get all documents from user corpus
            var corpusDocuments = await dbContext.Documents
                .Where(d => d.UserId == context.UserId)
                .Select(d => new ArticleRecord
                {
                    Title = d.Title,
                    Abstract = d.Abstract ?? "",
                    Authors = d.Authors ?? "",
                    Year = d.Year,
                    Venue = d.Venue ?? "",
                    DOI = d.DOI ?? "",
                    Keywords = d.Keywords ?? ""
                })
                .ToListAsync();

            if (!corpusDocuments.Any())
            {
                throw new InvalidOperationException("No documents in corpus. Please add documents first.");
            }

            var articles = corpusDocuments;

            _logger.LogInformation("Processing {Count} articles with {RQCount} research questions", 
                articles.Count, researchQuestions.Count);

            var screeningResults = new List<ScreeningResult>();
            var failureSummary = new ProcessingFailureSummary
            {
                ProcessingStarted = DateTime.UtcNow
            };

            // Process each article with explicit failure tracking
            for (int articleIndex = 0; articleIndex < articles.Count; articleIndex++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var article = articles[articleIndex];
                
                _logger.LogInformation("Processing article {Index}/{Total}: {Title}", 
                    articleIndex + 1, articles.Count, article.Title);

                try
                {
                    // Phase 1: Extract key semantics (once per article)
                    SemanticExtraction semantics;
                    try
                    {
                        semantics = await semanticExtraction.ExtractSemanticsAsync(article.Abstract);
                    }
                    catch (Exception ex)
                    {
                        RecordDocumentFailure(failureSummary, articleIndex, article, "Semantic Extraction", null, ex);
                        continue; // Skip to next document but continue processing
                    }

                    var screeningResult = new ScreeningResult
                    {
                        Title = article.Title,
                        Abstract = article.Abstract,
                        Authors = article.Authors,
                        Year = article.Year,
                        Venue = article.Venue,
                        DOI = article.DOI,
                        Link = article.Link,
                        Topics = semantics.Topics,
                        Entities = semantics.Entities,
                        Keywords = semantics.Keywords.Concat(article.Keywords.Split(';', StringSplitOptions.RemoveEmptyEntries)
                            .Select(k => k.Trim())
                            .Where(k => !string.IsNullOrWhiteSpace(k))).Distinct().ToList()
                    };

                    // Phase 2: Assess relevance and contribution for each research question
                    bool hasAssessmentFailure = false;
                    for (int rqIndex = 0; rqIndex < researchQuestions.Count; rqIndex++)
                    {
                        var researchQuestion = researchQuestions[rqIndex];
                        
                        try
                        {
                            var assessment = await AssessArticleForResearchQuestion(
                                article, researchQuestion, cognitiveAdapter, rqIndex);
                            
                            screeningResult.RQAssessments.Add(assessment);
                        }
                        catch (Exception ex)
                        {
                            RecordDocumentFailure(failureSummary, articleIndex, article, 
                                $"Research Question Assessment (RQ{rqIndex + 1})", rqIndex, ex);
                            hasAssessmentFailure = true;
                            
                            // Continue with next RQ - partial assessment data is valuable
                        }
                    }
                    
                    // Only include results if we have at least some assessment data
                    if (screeningResult.RQAssessments.Any())
                    {
                        // Determine must-read (logical OR of all successful RQ indicators)
                        screeningResult.MustRead = screeningResult.RQAssessments.Any(a => 
                            a.RelevanceIndicator && a.ContributionIndicator);

                        screeningResults.Add(screeningResult);
                        failureSummary.SuccessfulCount++;
                    }
                    else if (hasAssessmentFailure)
                    {
                        // Document had semantic extraction but all assessments failed
                        _logger.LogWarning("Document {Index} ({Title}) had complete assessment failure", 
                            articleIndex + 1, article.Title);
                    }
                }
                catch (Exception ex)
                {
                    // Unexpected error processing this document
                    RecordDocumentFailure(failureSummary, articleIndex, article, "Document Processing", null, ex);
                }
            }

            // Finalize failure tracking
            failureSummary.ProcessingFinished = DateTime.UtcNow;
            failureSummary.BatchCompleted = true;
            failureSummary.FailedCount = failureSummary.Failures.Count;
            
            // Generate failure type counts for summary
            failureSummary.FailureTypeCounts = failureSummary.Failures
                .GroupBy(f => f.ProcessingStage)
                .ToDictionary(g => g.Key, g => g.Count());

            // Generate CSV output
            var csvOutput = csvWriter.WriteToCsv(screeningResults, researchQuestions);

            // Create summary statistics with transparent failure reporting
            var mustReadCount = screeningResults.Count(r => r.MustRead);
            var totalArticlesAttempted = articles.Count;
            
            var summary = new Dictionary<string, object>
            {
                ["total_articles_attempted"] = totalArticlesAttempted,
                ["total_articles_processed"] = screeningResults.Count,
                ["processing_success_count"] = failureSummary.SuccessfulCount,
                ["processing_failure_count"] = failureSummary.FailedCount,
                ["processing_success_rate"] = totalArticlesAttempted > 0 ? (double)failureSummary.SuccessfulCount / totalArticlesAttempted * 100 : 0,
                ["must_read_count"] = mustReadCount,
                ["must_read_percentage"] = screeningResults.Count > 0 ? (double)mustReadCount / screeningResults.Count * 100 : 0,
                ["processing_summary_message"] = failureSummary.GenerateSummaryMessage(),
                ["processing_duration_seconds"] = failureSummary.ProcessingDuration?.TotalSeconds ?? 0,
                ["failure_breakdown"] = failureSummary.FailureTypeCounts,
                ["detailed_failures"] = failureSummary.Failures.Select(f => new
                {
                    document_index = f.DocumentIndex,
                    document_title = f.DocumentTitle,
                    processing_stage = f.ProcessingStage,
                    research_question_index = f.ResearchQuestionIndex,
                    exception_type = f.ExceptionType,
                    exception_message = f.ExceptionMessage,
                    failed_at = f.FailedAt,
                    formation_impact = f.FormationImpact
                }).ToList(),
                ["research_questions"] = researchQuestions,
                ["csv_output"] = Convert.ToBase64String(csvOutput),
                ["results"] = screeningResults.Select(r => new
                {
                    title = r.Title,
                    authors = r.Authors,
                    year = r.Year,
                    must_read = r.MustRead,
                    topics = r.Topics,
                    entities = r.Entities,
                    keywords = r.Keywords,
                    assessments = r.RQAssessments.Select(a => new
                    {
                        question_index = a.QuestionIndex,
                        relevance_score = a.RelevanceScore,
                        contribution_score = a.ContributionScore,
                        relevance_indicator = a.RelevanceIndicator,
                        contribution_indicator = a.ContributionIndicator,
                        relevance_reasoning = a.RelevanceReasoning,
                        contribution_reasoning = a.ContributionReasoning
                    }).ToList()
                }).ToList()
            };

            _logger.LogInformation("Completed screening: {Total} articles, {MustRead} must-read ({Percentage:F1}%)", 
                screeningResults.Count, mustReadCount, (double)mustReadCount / screeningResults.Count * 100);

            return new AnalyticalProcessResult
            {
                Success = true,
                Data = summary,
                ErrorMessage = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LLAssist systematic screening failed");
            return new AnalyticalProcessResult
            {
                Success = false,
                Data = new Dictionary<string, object>(),
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task<RQAssessment> AssessArticleForResearchQuestion(
        ArticleRecord article, 
        string researchQuestion, 
        ICognitiveAdapter cognitiveAdapter, 
        int questionIndex)
    {
        // Create dual assessment prompts
        var relevancePrompt = CreateRelevancePrompt(article, researchQuestion);
        var contributionPrompt = CreateContributionPrompt(article, researchQuestion);

        // Get assessments
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

    private string CreateRelevancePrompt(ArticleRecord article, string researchQuestion)
    {
        return $@"Assess the RELEVANCE of this research paper to the research question. 
Relevance means: Does this paper discuss topics related to the research question?

Research Question: {researchQuestion}

Paper Title: {article.Title}
Abstract: {article.Abstract}

Provide your assessment as a score from 0.0 to 1.0 and reasoning:
- 0.0-0.3: Not relevant - paper does not discuss related topics
- 0.4-0.6: Somewhat relevant - paper mentions related concepts
- 0.7-1.0: Highly relevant - paper directly discusses related topics

Format your response as:
Score: [0.0-1.0]
Reasoning: [Your explanation of why this score was assigned]";
    }

    private string CreateContributionPrompt(ArticleRecord article, string researchQuestion)
    {
        return $@"Assess the CONTRIBUTION of this research paper to the research question.
Contribution means: Does this paper directly research and provide findings for the research question?

Research Question: {researchQuestion}

Paper Title: {article.Title}
Abstract: {article.Abstract}

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
            var scoreMatch = System.Text.RegularExpressions.Regex.Match(response, @"Score:\s*([0-9]*\.?[0-9]+)");
            if (scoreMatch.Success && float.TryParse(scoreMatch.Groups[1].Value, out var parsedScore))
            {
                score = Math.Clamp(parsedScore, 0.0f, 1.0f);
            }

            // Look for reasoning pattern
            var reasoningMatch = System.Text.RegularExpressions.Regex.Match(response, @"Reasoning:\s*(.+)", System.Text.RegularExpressions.RegexOptions.Singleline);
            if (reasoningMatch.Success)
            {
                reasoning = reasoningMatch.Groups[1].Value.Trim();
            }
        }
        catch (Exception)
        {
            // Fallback: use the full response as reasoning
        }

        return (score, reasoning);
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
    
    /// <summary>
    /// Record a document processing failure with complete context for transparency
    /// </summary>
    private void RecordDocumentFailure(
        ProcessingFailureSummary failureSummary, 
        int articleIndex, 
        ArticleRecord article, 
        string processingStage, 
        int? researchQuestionIndex, 
        Exception exception)
    {
        var failure = new DocumentProcessingFailure
        {
            DocumentIndex = articleIndex,
            DocumentTitle = article.Title,
            DocumentIdentifier = article.DOI,
            ProcessingStage = processingStage,
            ResearchQuestionIndex = researchQuestionIndex,
            ExceptionType = exception.GetType().Name,
            ExceptionMessage = exception.Message,
            StackTrace = exception.StackTrace,
            InnerExceptionDetails = exception.InnerException?.ToString(),
            FormationImpact = GetFormationImpact(processingStage, exception),
            Context = new Dictionary<string, object>
            {
                ["article_authors"] = article.Authors,
                ["article_year"] = article.Year,
                ["article_venue"] = article.Venue,
                ["abstract_length"] = article.Abstract?.Length ?? 0
            }
        };
        
        failureSummary.Failures.Add(failure);
        
        _logger.LogError(exception, 
            "Document processing failure: Article {Index} ({Title}) failed at {Stage}. Impact: {Impact}",
            articleIndex + 1, article.Title, processingStage, failure.FormationImpact);
    }
    
    /// <summary>
    /// Determine the formation impact of a specific processing failure
    /// </summary>
    private string GetFormationImpact(string processingStage, Exception exception)
    {
        return processingStage switch
        {
            "Semantic Extraction" => "Document cannot be semantically analyzed. Topics, entities, and keywords unavailable for assessment. Document excluded from systematic screening.",
            "Research Question Assessment" when processingStage.Contains("RQ") => "Research question assessment incomplete. Document may still be partially evaluated against other research questions.",
            "Document Processing" => "Complete document processing failure. Document excluded from systematic screening results.",
            _ => "Processing stage failed. Document may be partially processed or excluded depending on failure type."
        };
    }
}