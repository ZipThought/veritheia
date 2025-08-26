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
            .AddTextArea("csv_content", "CSV file content (paste CSV data here)", true);
    }
    
    public bool ValidateInputs(ProcessContext context)
    {
        if (!context.Inputs.ContainsKey("research_questions"))
        {
            _logger.LogError("Missing required input: research_questions");
            return false;
        }
        
        if (!context.Inputs.ContainsKey("csv_content"))
        {
            _logger.LogError("Missing required input: csv_content");
            return false;
        }
        
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

            // Get CSV content as string
            var csvContentString = context.Inputs["csv_content"]?.ToString();
            if (string.IsNullOrWhiteSpace(csvContentString))
            {
                throw new InvalidOperationException("CSV content is empty");
            }

            var csvFileContent = System.Text.Encoding.UTF8.GetBytes(csvContentString);

            // Get services
            using var scope = _serviceProvider.CreateScope();
            var csvParser = scope.ServiceProvider.GetRequiredService<CsvParserService>();
            var csvWriter = scope.ServiceProvider.GetRequiredService<CsvWriterService>();
            var semanticExtraction = scope.ServiceProvider.GetRequiredService<SemanticExtractionService>();
            var cognitiveAdapter = scope.ServiceProvider.GetRequiredService<ICognitiveAdapter>();

            // Parse CSV to get articles
            List<ArticleRecord> articles;
            using (var csvStream = new MemoryStream(csvFileContent))
            {
                articles = csvParser.ParseCsv(csvStream);
            }

            if (!articles.Any())
            {
                throw new InvalidOperationException("No articles found in CSV file");
            }

            _logger.LogInformation("Processing {Count} articles with {RQCount} research questions", 
                articles.Count, researchQuestions.Count);

            var screeningResults = new List<ScreeningResult>();

            // Process each article
            for (int articleIndex = 0; articleIndex < articles.Count; articleIndex++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var article = articles[articleIndex];
                
                _logger.LogInformation("Processing article {Index}/{Total}: {Title}", 
                    articleIndex + 1, articles.Count, article.Title);

                // Phase 1: Extract key semantics (once per article)
                var semantics = await semanticExtraction.ExtractSemanticsAsync(article.Abstract);

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
                for (int rqIndex = 0; rqIndex < researchQuestions.Count; rqIndex++)
                {
                    var researchQuestion = researchQuestions[rqIndex];
                    
                    var assessment = await AssessArticleForResearchQuestion(
                        article, researchQuestion, cognitiveAdapter, rqIndex);
                    
                    screeningResult.RQAssessments.Add(assessment);
                }

                // Determine must-read (logical OR of all RQ indicators)
                screeningResult.MustRead = screeningResult.RQAssessments.Any(a => 
                    a.RelevanceIndicator && a.ContributionIndicator);

                screeningResults.Add(screeningResult);
            }

            // Generate CSV output
            var csvOutput = csvWriter.WriteToCsv(screeningResults, researchQuestions);

            // Create summary statistics
            var mustReadCount = screeningResults.Count(r => r.MustRead);
            var summary = new Dictionary<string, object>
            {
                ["total_articles"] = screeningResults.Count,
                ["must_read_count"] = mustReadCount,
                ["must_read_percentage"] = screeningResults.Count > 0 ? (double)mustReadCount / screeningResults.Count * 100 : 0,
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
}