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
/// Systematic Screening Process implementing neurosymbolic transcendence for formation through authorship.
/// Enables researchers to author their intellectual frameworks in natural language, which become
/// the symbolic systems governing document processing. Formation accumulates through engagement
/// with documents projected through these user-authored frameworks.
/// 
/// Based on LLAssist methodology (arXiv:2407.13993v3) but transcended to enable ANY researcher
/// to achieve systematic screening without coding - their natural language IS the symbolic system.
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
    public string Description => "Formation through systematic engagement with documents projected through your authored framework";
    public string Category => "Research";

    public InputDefinition GetInputDefinition()
    {
        return new InputDefinition()
            .AddTextArea("intellectual_framework", "Your complete intellectual framework (research questions, definitions, criteria)", true)
            .AddTextArea("research_questions", "Specific research questions (one per line)", true)
            .AddTextArea("definitions", "Your theoretical definitions and vocabulary (optional)", false)
            .AddTextArea("assessment_criteria", "Your assessment criteria and methodology (optional)", false)
            .AddTextArea("csv_upload", "Upload CSV to add documents to corpus (optional)", false);
    }

    public bool ValidateInputs(ProcessContext context)
    {
        // User must author their framework - this becomes the symbolic system
        if (!context.Inputs.ContainsKey("intellectual_framework") &&
            !context.Inputs.ContainsKey("research_questions"))
        {
            _logger.LogError("Missing user-authored framework - cannot create symbolic system");
            return false;
        }

        return true;
    }

    public async Task<AnalyticalProcessResult> ExecuteAsync(
        ProcessContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initiating formation journey {JourneyId} through user-authored framework",
            context.JourneyId);

        try
        {
            // Step 1: Construct the user's authored symbolic framework
            var framework = ConstructUserFramework(context);

            _logger.LogInformation("User-authored framework established with {Count} research questions",
                framework.ResearchQuestions.Count);

            // Get services for mechanical orchestration
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<VeritheiaDbContext>();
            var documentService = scope.ServiceProvider.GetRequiredService<DocumentService>();
            var csvParser = scope.ServiceProvider.GetRequiredService<CsvParserService>();
            var csvWriter = scope.ServiceProvider.GetRequiredService<CsvWriterService>();
            var cognitiveAdapter = scope.ServiceProvider.GetRequiredService<ICognitiveAdapter>();

            // Step 2: Ensure documents exist in user's corpus for projection
            await EnsureCorpusDocuments(context, csvParser, documentService);

            // Step 3: Load documents from user's corpus for journey projection
            var corpusMetadata = await dbContext.DocumentMetadata
                .Where(dm => dm.UserId == context.UserId)
                .ToListAsync(cancellationToken);

            if (!corpusMetadata.Any())
            {
                throw new InvalidOperationException(
                    "No documents in corpus. Please add documents to begin your formation journey.");
            }

            var documents = corpusMetadata.Select(dm => new ProjectableDocument
            {
                Id = dm.Id,
                Title = dm.Title ?? "",
                Abstract = dm.Abstract ?? "",
                Authors = dm.Authors != null ? string.Join(", ", dm.Authors) : "",
                Year = dm.PublicationDate?.Year,
                Venue = dm.ExtendedMetadata?.ContainsKey("venue") == true
                    ? dm.ExtendedMetadata["venue"]?.ToString() ?? ""
                    : "",
                DOI = dm.DOI ?? "",
                Keywords = dm.Keywords != null ? string.Join(", ", dm.Keywords) : ""
            }).ToList();

            _logger.LogInformation("Projecting {Count} documents through user-authored framework",
                documents.Count);

            // Step 4: Mechanical orchestration - EVERY document gets identical treatment
            var formationResults = new FormationResults
            {
                ProcessingStarted = DateTime.UtcNow,
                TotalDocuments = documents.Count,
                Framework = framework
            };

            // Initialize progress reporting
            var startTime = DateTime.UtcNow;
            var progress = new Veritheia.Core.Models.ProcessProgress
            {
                TotalCount = documents.Count,
                StartTime = startTime,
                ProcessedCount = 0,
                FailedCount = 0,
                MustReadCount = 0
            };

            // Process each document through the user's authored framework
            for (int docIndex = 0; docIndex < documents.Count; docIndex++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var document = documents[docIndex];

                _logger.LogInformation("Projecting document {Index}/{Total} through user framework: {Title}",
                    docIndex + 1, documents.Count, document.Title);

                // Report progress to UI in real-time
                if (context.ProgressReporter != null)
                {
                    progress.CurrentIndex = docIndex;
                    progress.CurrentDocument = document.Title;
                    progress.StatusMessage = $"Processing: {document.Title}";
                    context.ProgressReporter.Report(progress);
                }

                try
                {
                    // Project this document through the user's authored framework
                    var projection = await ProjectDocumentThroughFramework(
                        document, framework, cognitiveAdapter, docIndex);

                    formationResults.DocumentProjections.Add(projection);
                    formationResults.SuccessfulProjections++;
                    
                    // Update progress after successful projection
                    progress.ProcessedCount++;
                    if (projection.MustRead)
                        progress.MustReadCount++;
                    
                    // Report updated progress
                    if (context.ProgressReporter != null)
                    {
                        progress.StatusMessage = $"Processed: {document.Title}";
                        context.ProgressReporter.Report(progress);
                    }
                    
                    // Save intermediate results every 10 documents
                    if (progress.ProcessedCount % 10 == 0)
                    {
                        // TODO: Implement actual intermediate saving after adding to DbContext
                        // await SaveIntermediateResults(context, formationResults, progress);
                        progress.LastSaveTime = DateTime.UtcNow;
                        if (context.ProgressReporter != null)
                        {
                            context.ProgressReporter.Report(progress);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Transparent failure tracking - continue processing
                    RecordProjectionFailure(formationResults, docIndex, document, ex);
                    
                    // Update failed count in progress
                    progress.FailedCount++;
                    if (context.ProgressReporter != null)
                    {
                        progress.StatusMessage = $"Failed: {document.Title}";
                        context.ProgressReporter.Report(progress);
                    }
                }
            }

            formationResults.ProcessingCompleted = DateTime.UtcNow;

            // Step 5: Generate outputs that enable formation through engagement
            var formationOutput = GenerateFormationOutput(formationResults, framework, csvWriter);

            _logger.LogInformation(
                "Formation journey prepared: {Success}/{Total} documents projected. User engagement can now begin.",
                formationResults.SuccessfulProjections, formationResults.TotalDocuments);

            return new AnalyticalProcessResult
            {
                Success = true,
                Data = formationOutput,
                ErrorMessage = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Formation journey failed - framework could not be applied");

            // Capture more details about the error
            var errorDetails = new Dictionary<string, object>
            {
                ["exception_type"] = ex.GetType().Name,
                ["message"] = ex.Message,
                ["stack_trace"] = ex.StackTrace ?? ""
            };

            if (ex.InnerException != null)
            {
                errorDetails["inner_exception"] = ex.InnerException.Message;
                errorDetails["inner_type"] = ex.InnerException.GetType().Name;
            }

            return new AnalyticalProcessResult
            {
                Success = false,
                Data = errorDetails,
                ErrorMessage = $"Formation journey could not begin: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Construct the user's intellectual framework from their natural language authorship.
    /// This becomes the symbolic system governing all document processing.
    /// </summary>
    private JourneyFramework ConstructUserFramework(ProcessContext context)
    {
        var framework = new JourneyFramework();

        // The user's complete intellectual stance (if provided)
        if (context.Inputs.ContainsKey("intellectual_framework"))
        {
            framework.IntellectualStance = context.Inputs["intellectual_framework"].ToString() ?? "";
        }

        // Parse research questions - these are the core of the symbolic system
        var questionsText = context.Inputs["research_questions"]?.ToString() ?? "";
        framework.ResearchQuestions = questionsText
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(q => q.Trim())
            .Where(q => !string.IsNullOrWhiteSpace(q))
            .ToList();

        // User's definitions become part of the symbolic vocabulary
        if (context.Inputs.ContainsKey("definitions"))
        {
            var definitionsText = context.Inputs["definitions"].ToString() ?? "";
            framework.Definitions = ParseDefinitions(definitionsText);
        }

        // User's assessment criteria shape the evaluation lens
        if (context.Inputs.ContainsKey("assessment_criteria"))
        {
            framework.AssessmentCriteria = context.Inputs["assessment_criteria"].ToString() ?? "";
        }

        // Construct the complete framework narrative that will guide neural understanding
        framework.CompleteFrameworkNarrative = ConstructFrameworkNarrative(framework);

        return framework;
    }

    /// <summary>
    /// Project a document through the user's authored framework.
    /// The framework acts as the symbolic system, applied through neural semantic understanding.
    /// </summary>
    private async Task<DocumentProjection> ProjectDocumentThroughFramework(
        ProjectableDocument document,
        JourneyFramework framework,
        ICognitiveAdapter cognitiveAdapter,
        int documentIndex)
    {
        var projection = new DocumentProjection
        {
            DocumentId = document.Id,
            Title = document.Title,
            Authors = document.Authors,
            Year = document.Year,
            Venue = document.Venue,
            DOI = document.DOI
        };

        // Phase 1: Semantic extraction THROUGH the user's framework lens
        var extractionPrompt = $@"
You are applying this researcher's intellectual framework to analyze a document.

RESEARCHER'S FRAMEWORK:
{framework.CompleteFrameworkNarrative}

DOCUMENT TO ANALYZE:
Title: {document.Title}
Abstract: {document.Abstract}

Extract the KEY SEMANTICS that are relevant to THIS RESEARCHER'S framework.
Focus on topics, entities, and concepts that matter for THEIR research questions.
Do not extract generic topics - extract what matters for THEIR specific inquiry.

Return ONLY a valid JSON object (no markdown, no explanation, no code blocks, just the JSON):
{{
  ""topics"": [""topic1"", ""topic2""],
  ""entities"": [""entity1"", ""entity2""],
  ""keywords"": [""keyword1"", ""keyword2""]
}}";

        try
        {
            var extractionResponse = await cognitiveAdapter.GenerateTextAsync(extractionPrompt);
            var semantics = ParseSemanticExtraction(extractionResponse);
            projection.FrameworkSpecificTopics = semantics.Topics;
            projection.FrameworkSpecificEntities = semantics.Entities;
            projection.FrameworkSpecificKeywords = semantics.Keywords;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Semantic extraction through framework failed for document {Index}",
                documentIndex);
            // Continue with assessment even if extraction fails
        }

        // Phase 2: Assess through each research question in the framework
        foreach (var (question, qIndex) in framework.ResearchQuestions.Select((q, i) => (q, i)))
        {
            var assessment = await AssessThroughFramework(
                document, question, qIndex, framework, cognitiveAdapter);
            projection.FrameworkAssessments.Add(assessment);
        }

        // Determine formation indicators based on user's framework
        projection.RequiresEngagement = projection.FrameworkAssessments.Any(a =>
            a.RelevanceScore >= 0.7 || a.ContributionScore >= 0.7);

        return projection;
    }

    /// <summary>
    /// Assess a document through the user's specific research question and framework.
    /// This implements the neurosymbolic transcendence - natural language becomes symbolic rules.
    /// </summary>
    private async Task<FrameworkAssessment> AssessThroughFramework(
        ProjectableDocument document,
        string researchQuestion,
        int questionIndex,
        JourneyFramework framework,
        ICognitiveAdapter cognitiveAdapter)
    {
        // Build assessment prompt that applies user's framework as symbolic system
        var assessmentPrompt = $@"
You are applying this researcher's intellectual framework to assess a document.

RESEARCHER'S COMPLETE FRAMEWORK:
{framework.CompleteFrameworkNarrative}

SPECIFIC RESEARCH QUESTION BEING ASSESSED:
{researchQuestion}

DOCUMENT:
Title: {document.Title}
Abstract: {document.Abstract}

Assess this document THROUGH THE RESEARCHER'S LENS:

1. RELEVANCE Assessment:
   Does this document discuss topics related to the researcher's question?
   Use THEIR definitions and vocabulary to judge relevance.
   Score 0.0-1.0 where >0.7 means highly relevant to THEIR framework.

2. CONTRIBUTION Assessment:
   Does this document directly research the question with findings?
   Use THEIR criteria for what constitutes a contribution.
   Score 0.0-1.0 where >0.7 means strong contribution to THEIR research.

Provide reasoning that shows how you applied THEIR framework, not generic assessment.

Return ONLY a valid JSON object (no markdown, no explanation, no code blocks, just the JSON):
{{
  ""relevance_score"": 0.8,
  ""relevance_reasoning"": ""How this relates to THEIR framework..."",
  ""contribution_score"": 0.6,
  ""contribution_reasoning"": ""How this contributes to THEIR research..."",
  ""framework_application"": ""How I applied their specific definitions and criteria...""
}}";

        var response = await cognitiveAdapter.GenerateTextAsync(assessmentPrompt);
        var assessment = ParseFrameworkAssessment(response);
        assessment.ResearchQuestion = researchQuestion;
        assessment.QuestionIndex = questionIndex;

        return assessment;
    }

    /// <summary>
    /// Generate outputs that enable formation through engagement, not consumption.
    /// </summary>
    private Dictionary<string, object> GenerateFormationOutput(
        FormationResults results,
        JourneyFramework framework,
        CsvWriterService csvWriter)
    {
        // Calculate formation metrics
        var documentsRequiringEngagement = results.DocumentProjections
            .Count(p => p.RequiresEngagement);

        var highRelevanceCount = results.DocumentProjections
            .Count(p => p.FrameworkAssessments.Any(a => a.RelevanceScore >= 0.7));

        var highContributionCount = results.DocumentProjections
            .Count(p => p.FrameworkAssessments.Any(a => a.ContributionScore >= 0.7));

        // Generate CSV for external tools if needed
        var csvOutput = GenerateFormationCsv(results, framework, csvWriter);

        // Create formation prompts to guide engagement
        var formationPrompts = GenerateFormationPrompts(results, framework);

        return new Dictionary<string, object>
        {
            // Formation journey metadata
            ["journey_framework"] = new
            {
                intellectual_stance = framework.IntellectualStance,
                research_questions = framework.ResearchQuestions,
                definitions = framework.Definitions,
                assessment_criteria = framework.AssessmentCriteria
            },

            // Processing transparency
            ["processing_summary"] = new
            {
                total_documents = results.TotalDocuments,
                successful_projections = results.SuccessfulProjections,
                failed_projections = results.ProjectionFailures.Count,
                processing_duration_seconds = results.ProcessingDuration?.TotalSeconds ?? 0
            },

            // Formation indicators
            ["formation_indicators"] = new
            {
                documents_requiring_engagement = documentsRequiringEngagement,
                high_relevance_documents = highRelevanceCount,
                high_contribution_documents = highContributionCount,
                engagement_percentage = results.DocumentProjections.Any()
                    ? (double)documentsRequiringEngagement / results.DocumentProjections.Count * 100
                    : 0
            },

            // Detailed projections for engagement
            ["document_projections"] = results.DocumentProjections.Select(p => new
            {
                title = p.Title,
                authors = p.Authors,
                year = p.Year,
                requires_engagement = p.RequiresEngagement,
                framework_topics = p.FrameworkSpecificTopics,
                framework_entities = p.FrameworkSpecificEntities,
                framework_keywords = p.FrameworkSpecificKeywords,
                assessments = p.FrameworkAssessments.Select(a => new
                {
                    question = a.ResearchQuestion,
                    relevance_score = a.RelevanceScore,
                    relevance_reasoning = a.RelevanceReasoning,
                    contribution_score = a.ContributionScore,
                    contribution_reasoning = a.ContributionReasoning,
                    framework_application = a.FrameworkApplication
                }).ToList()
            }).ToList(),

            // Formation prompts to guide synthesis
            ["formation_prompts"] = formationPrompts,

            // Failure transparency
            ["projection_failures"] = results.ProjectionFailures.Select(f => new
            {
                document_index = f.DocumentIndex,
                document_title = f.DocumentTitle,
                failure_reason = f.FailureReason,
                impact_on_formation = f.FormationImpact
            }).ToList(),

            // CSV for external tools
            ["csv_output"] = csvOutput != null ? Convert.ToBase64String(csvOutput) : null
        };
    }

    #region Helper Methods

    private async Task EnsureCorpusDocuments(
        ProcessContext context,
        CsvParserService csvParser,
        DocumentService documentService)
    {
        if (context.Inputs.ContainsKey("csv_upload") && context.Inputs["csv_upload"] != null)
        {
            var csvContent = context.Inputs["csv_upload"].ToString();
            if (!string.IsNullOrWhiteSpace(csvContent))
            {
                _logger.LogInformation("Adding documents to corpus for journey projection");
                var csvBytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
                using var csvStream = new MemoryStream(csvBytes);
                var csvArticles = csvParser.ParseCsv(csvStream);

                foreach (var article in csvArticles)
                {
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

    private Dictionary<string, string> ParseDefinitions(string definitionsText)
    {
        var definitions = new Dictionary<string, string>();
        var lines = definitionsText.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var parts = line.Split(':', 2);
            if (parts.Length == 2)
            {
                definitions[parts[0].Trim()] = parts[1].Trim();
            }
        }

        return definitions;
    }

    private string ConstructFrameworkNarrative(JourneyFramework framework)
    {
        var narrative = new List<string>();

        if (!string.IsNullOrWhiteSpace(framework.IntellectualStance))
        {
            narrative.Add($"Intellectual Stance: {framework.IntellectualStance}");
        }

        if (framework.ResearchQuestions.Any())
        {
            narrative.Add($"Research Questions:\n{string.Join("\n", framework.ResearchQuestions.Select((q, i) => $"{i + 1}. {q}"))}");
        }

        if (framework.Definitions?.Any() == true)
        {
            narrative.Add($"Key Definitions:\n{string.Join("\n", framework.Definitions.Select(kvp => $"- {kvp.Key}: {kvp.Value}"))}");
        }

        if (!string.IsNullOrWhiteSpace(framework.AssessmentCriteria))
        {
            narrative.Add($"Assessment Criteria: {framework.AssessmentCriteria}");
        }

        return string.Join("\n\n", narrative);
    }

    private SemanticExtraction ParseSemanticExtraction(string jsonResponse)
    {
        try
        {
            // Log the raw response for debugging
            _logger.LogDebug("Raw semantic extraction response: {Response}", jsonResponse);

            // Try to extract JSON from the response if it contains extra text or markdown
            var cleanedResponse = ExtractJsonFromResponse(jsonResponse);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<SemanticExtraction>(cleanedResponse, options)
                ?? new SemanticExtraction();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse semantic extraction response. Raw response: {Response}", jsonResponse);
            return new SemanticExtraction();
        }
    }

    private string ExtractJsonFromResponse(string response)
    {
        // Try to extract JSON from a response that may contain markdown or extra text
        var trimmed = response.Trim();

        // Remove markdown code blocks if present
        if (trimmed.StartsWith("```json"))
        {
            trimmed = trimmed.Substring(7); // Remove ```json
            var endIndex = trimmed.LastIndexOf("```");
            if (endIndex > 0)
            {
                trimmed = trimmed.Substring(0, endIndex);
            }
        }
        else if (trimmed.StartsWith("```"))
        {
            trimmed = trimmed.Substring(3); // Remove ```
            var endIndex = trimmed.LastIndexOf("```");
            if (endIndex > 0)
            {
                trimmed = trimmed.Substring(0, endIndex);
            }
        }

        // Find the first { and last } to extract JSON object
        var startIndex = trimmed.IndexOf('{');
        var endIndex2 = trimmed.LastIndexOf('}');

        if (startIndex >= 0 && endIndex2 > startIndex)
        {
            return trimmed.Substring(startIndex, endIndex2 - startIndex + 1);
        }

        return trimmed;
    }

    private FrameworkAssessment ParseFrameworkAssessment(string jsonResponse)
    {
        try
        {
            // Try to extract JSON from the response if it contains extra text or markdown
            var cleanedResponse = ExtractJsonFromResponse(jsonResponse);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var json = JsonSerializer.Deserialize<JsonElement>(cleanedResponse, options);

            return new FrameworkAssessment
            {
                RelevanceScore = json.TryGetProperty("relevance_score", out var rs)
                    ? (float)rs.GetDouble() : 0f,
                RelevanceReasoning = json.TryGetProperty("relevance_reasoning", out var rr)
                    ? rr.GetString() ?? "" : "",
                ContributionScore = json.TryGetProperty("contribution_score", out var cs)
                    ? (float)cs.GetDouble() : 0f,
                ContributionReasoning = json.TryGetProperty("contribution_reasoning", out var cr)
                    ? cr.GetString() ?? "" : "",
                FrameworkApplication = json.TryGetProperty("framework_application", out var fa)
                    ? fa.GetString() ?? "" : ""
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse framework assessment response. Raw response: {Response}", jsonResponse);
            return new FrameworkAssessment();
        }
    }

    private List<string> GenerateFormationPrompts(FormationResults results, JourneyFramework framework)
    {
        var prompts = new List<string>();

        // Identify patterns in high-scoring documents
        var highRelevanceDocs = results.DocumentProjections
            .Where(p => p.FrameworkAssessments.Any(a => a.RelevanceScore >= 0.7))
            .ToList();

        if (highRelevanceDocs.Any())
        {
            prompts.Add($"Review the {highRelevanceDocs.Count} high-relevance documents to identify patterns in how they address your research questions.");
        }

        // Suggest framework refinement opportunities
        var lowScoringDocs = results.DocumentProjections
            .Where(p => p.FrameworkAssessments.All(a => a.RelevanceScore < 0.3))
            .ToList();

        if (lowScoringDocs.Count > results.DocumentProjections.Count * 0.5)
        {
            prompts.Add("Many documents scored low on relevance. Consider refining your research questions or expanding your definitions.");
        }

        // Encourage synthesis
        if (results.DocumentProjections.Any(p => p.RequiresEngagement))
        {
            prompts.Add("Begin synthesizing insights by examining how different documents approach your research questions.");
        }

        return prompts;
    }

    private byte[]? GenerateFormationCsv(
        FormationResults results,
        JourneyFramework framework,
        CsvWriterService csvWriter)
    {
        if (!results.DocumentProjections.Any())
            return null;

        var screeningResults = results.DocumentProjections.Select(p => new ScreeningResult
        {
            Title = p.Title,
            Authors = p.Authors,
            Year = p.Year,
            Venue = p.Venue,
            DOI = p.DOI,
            Topics = p.FrameworkSpecificTopics,
            Entities = p.FrameworkSpecificEntities,
            Keywords = p.FrameworkSpecificKeywords,
            MustRead = p.RequiresEngagement,
            RQAssessments = p.FrameworkAssessments.Select(a => new RQAssessment
            {
                QuestionIndex = a.QuestionIndex,
                RelevanceScore = a.RelevanceScore,
                ContributionScore = a.ContributionScore,
                RelevanceIndicator = a.RelevanceScore >= 0.7,
                ContributionIndicator = a.ContributionScore >= 0.7,
                RelevanceReasoning = a.RelevanceReasoning,
                ContributionReasoning = a.ContributionReasoning
            }).ToList()
        }).ToList();

        return csvWriter.WriteToCsv(screeningResults, framework.ResearchQuestions);
    }

    private void RecordProjectionFailure(
        FormationResults results,
        int documentIndex,
        ProjectableDocument document,
        Exception exception)
    {
        var failure = new ProjectionFailure
        {
            DocumentIndex = documentIndex,
            DocumentTitle = document.Title,
            FailureReason = exception.Message,
            FormationImpact = "Document could not be projected through your framework. Manual review recommended.",
            FailedAt = DateTime.UtcNow
        };

        results.ProjectionFailures.Add(failure);

        _logger.LogError(exception,
            "Failed to project document {Index} ({Title}) through user framework",
            documentIndex + 1, document.Title);
    }

    #endregion

    public Dictionary<string, object> GetCapabilities()
    {
        return new Dictionary<string, object>
        {
            ["supports_batch"] = true,
            ["supports_streaming"] = false,
            ["requires_llm"] = true,
            ["enables_formation"] = true,
            ["supports_framework_authorship"] = true,
            ["implements_neurosymbolic_transcendence"] = true
        };
    }

    #region Internal Classes

    // TODO: Implement after adding ProcessIntermediateResult to DbContext
    // private async Task SaveIntermediateResults(ProcessContext context, FormationResults formationResults, Veritheia.Core.Models.ProcessProgress progress)
    // {
    //     try
    //     {
    //         // Save current projection results to database for recovery
    //         var db = context.GetService<VeritheiaDbContext>();
    //         if (db != null)
    //         {
    //             // Create intermediate result record
    //             var intermediateResult = new ProcessIntermediateResult
    //             {
    //                 Id = Guid.NewGuid(),
    //                 ExecutionId = context.ExecutionId,
    //                 JourneyId = context.JourneyId,
    //                 SavedAt = DateTime.UtcNow,
    //                 ProcessedCount = progress.ProcessedCount,
    //                 FailedCount = progress.FailedCount,
    //                 MustReadCount = progress.MustReadCount,
    //                 ProjectionData = System.Text.Json.JsonSerializer.Serialize(formationResults.DocumentProjections.TakeLast(10))
    //             };
    //             
    //             db.ProcessIntermediateResults.Add(intermediateResult);
    //             await db.SaveChangesAsync();
    //             
    //             _logger.LogInformation("Saved intermediate results at document {Count}", progress.ProcessedCount);
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogWarning(ex, "Failed to save intermediate results");
    //         // Don't fail the whole process for intermediate save failure
    //     }
    // }

    private class JourneyFramework
    {
        public string IntellectualStance { get; set; } = "";
        public List<string> ResearchQuestions { get; set; } = new();
        public Dictionary<string, string> Definitions { get; set; } = new();
        public string AssessmentCriteria { get; set; } = "";
        public string CompleteFrameworkNarrative { get; set; } = "";
    }

    private class ProjectableDocument
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Abstract { get; set; } = "";
        public string Authors { get; set; } = "";
        public int? Year { get; set; }
        public string Venue { get; set; } = "";
        public string DOI { get; set; } = "";
        public string Keywords { get; set; } = "";
    }

    private class DocumentProjection
    {
        public Guid DocumentId { get; set; }
        public string Title { get; set; } = "";
        public string Authors { get; set; } = "";
        public int? Year { get; set; }
        public string Venue { get; set; } = "";
        public string DOI { get; set; } = "";
        public List<string> FrameworkSpecificTopics { get; set; } = new();
        public List<string> FrameworkSpecificEntities { get; set; } = new();
        public List<string> FrameworkSpecificKeywords { get; set; } = new();
        public List<FrameworkAssessment> FrameworkAssessments { get; set; } = new();
        public bool RequiresEngagement { get; set; }
        public bool MustRead => FrameworkAssessments.Any(a => a.RelevanceIndicator || a.ContributionIndicator);
    }

    private class FrameworkAssessment
    {
        public string ResearchQuestion { get; set; } = "";
        public int QuestionIndex { get; set; }
        public float RelevanceScore { get; set; }
        public string RelevanceReasoning { get; set; } = "";
        public float ContributionScore { get; set; }
        public string ContributionReasoning { get; set; } = "";
        public string FrameworkApplication { get; set; } = "";
        public bool RelevanceIndicator => RelevanceScore >= 0.7f;
        public bool ContributionIndicator => ContributionScore >= 0.7f;
    }

    private class FormationResults
    {
        public DateTime ProcessingStarted { get; set; }
        public DateTime ProcessingCompleted { get; set; }
        public int TotalDocuments { get; set; }
        public int SuccessfulProjections { get; set; }
        public List<DocumentProjection> DocumentProjections { get; set; } = new();
        public List<ProjectionFailure> ProjectionFailures { get; set; } = new();
        public JourneyFramework Framework { get; set; } = new();

        public TimeSpan? ProcessingDuration =>
            ProcessingCompleted != default ? ProcessingCompleted - ProcessingStarted : null;
    }

    private class ProjectionFailure
    {
        public int DocumentIndex { get; set; }
        public string DocumentTitle { get; set; } = "";
        public string FailureReason { get; set; } = "";
        public string FormationImpact { get; set; } = "";
        public DateTime FailedAt { get; set; }
    }

    private class SemanticExtraction
    {
        public List<string> Topics { get; set; } = new();
        public List<string> Entities { get; set; } = new();
        public List<string> Keywords { get; set; } = new();
    }

    #endregion
}