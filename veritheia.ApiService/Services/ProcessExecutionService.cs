using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Veritheia.Core.Interfaces;
using Veritheia.Core.ValueObjects;
using Veritheia.Data;
using Veritheia.Data.Entities;
using Veritheia.Data.Services;

namespace Veritheia.ApiService.Services;

/// <summary>
/// Service for managing process execution within journeys
/// Coordinates between Web layer and Process Engine
/// </summary>
public class ProcessExecutionService
{
    private readonly VeritheiaDbContext _db;
    private readonly ProcessEngine _processEngine;
    private readonly ILogger<ProcessExecutionService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ProcessExecutionService(
        VeritheiaDbContext dbContext,
        ProcessEngine processEngine,
        ILogger<ProcessExecutionService> logger,
        IServiceProvider serviceProvider)
    {
        _db = dbContext;
        _processEngine = processEngine;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Execute SystematicScreeningProcess for a journey
    /// </summary>
    public async Task<ProcessExecutionResult> ExecuteSystematicScreeningAsync(
        Guid userId,
        Guid journeyId,
        List<Guid> documentIds,
        string researchQuestions,
        double relevanceThreshold = 0.7,
        double contributionThreshold = 0.7)
    {
        _logger.LogInformation("Starting systematic screening for journey {JourneyId} with {Count} documents", 
            journeyId, documentIds.Count);

        // Validate journey exists and belongs to user
        var journey = await _db.Journeys
            .FirstOrDefaultAsync(j => j.Id == journeyId && j.UserId == userId);
        
        if (journey == null)
            throw new UnauthorizedAccessException($"Journey {journeyId} not found or access denied");

        // Build inputs for SystematicScreeningProcess
        var inputs = new Dictionary<string, object>
        {
            ["research_questions"] = researchQuestions,
            ["document_ids"] = documentIds,
            ["relevance_threshold"] = relevanceThreshold.ToString(),
            ["contribution_threshold"] = contributionThreshold.ToString()
        };

        // Queue the process for execution
        var executionId = await _processEngine.QueueProcessAsync(
            "systematic_screening",
            userId,
            journeyId,
            inputs);

        _logger.LogInformation("Queued systematic screening process with execution ID {ExecutionId}", executionId);

        // For now, execute synchronously (later can be moved to background worker)
        var result = await _processEngine.ExecuteProcessAsync(
            "systematic_screening",
            journeyId,
            inputs);

        return result;
    }

    /// <summary>
    /// Get process execution status for a journey
    /// </summary>
    public async Task<ProcessExecution?> GetLatestExecutionAsync(Guid userId, Guid journeyId)
    {
        return await _db.ProcessExecutions
            .Where(e => e.JourneyId == journeyId && e.UserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get process results for a completed execution
    /// </summary>
    public async Task<SystematicScreeningResults?> GetScreeningResultsAsync(Guid userId, Guid executionId)
    {
        var result = await _db.ProcessResults
            .Where(r => r.ExecutionId == executionId && r.UserId == userId)
            .FirstOrDefaultAsync();

        if (result == null)
            return null;

        // Parse the results from the stored data
        var screeningResults = new SystematicScreeningResults
        {
            ExecutionId = executionId,
            TotalDocuments = 0,
            DocumentResults = new List<DocumentScreeningResult>()
        };

        // Extract results from the Data dictionary
        if (result.Data.ContainsKey("results") && result.Data["results"] is List<Dictionary<string, object>> results)
        {
            foreach (var docResult in results)
            {
                var docScreening = new DocumentScreeningResult
                {
                    DocumentId = Guid.Parse(docResult["document_id"].ToString()!),
                    Title = docResult["title"]?.ToString() ?? "",
                    Authors = docResult["authors"]?.ToString() ?? "",
                    RelevanceScore = Convert.ToDouble(docResult["relevance_score"]),
                    ContributionScore = Convert.ToDouble(docResult["contribution_score"]),
                    IsRelevant = Convert.ToBoolean(docResult["is_relevant"]),
                    IsContributing = Convert.ToBoolean(docResult["is_contributing"]),
                    IsMustRead = Convert.ToBoolean(docResult["is_must_read"]),
                    RelevanceReasoning = docResult["relevance_reasoning"]?.ToString(),
                    ContributionReasoning = docResult["contribution_reasoning"]?.ToString()
                };

                // Extract semantics if present
                if (docResult.ContainsKey("topics") && docResult["topics"] is List<string> topics)
                    docScreening.Topics = topics;
                
                if (docResult.ContainsKey("keywords") && docResult["keywords"] is List<string> keywords)
                    docScreening.Keywords = keywords;

                screeningResults.DocumentResults.Add(docScreening);
            }
        }

        screeningResults.TotalDocuments = screeningResults.DocumentResults.Count;
        screeningResults.MustReadCount = screeningResults.DocumentResults.Count(r => r.IsMustRead);
        screeningResults.RelevantCount = screeningResults.DocumentResults.Count(r => r.IsRelevant);
        screeningResults.ContributingCount = screeningResults.DocumentResults.Count(r => r.IsContributing);

        return screeningResults;
    }

    /// <summary>
    /// Get all executions for a journey
    /// </summary>
    public async Task<List<ProcessExecution>> GetJourneyExecutionsAsync(Guid userId, Guid journeyId)
    {
        return await _db.ProcessExecutions
            .Where(e => e.JourneyId == journeyId && e.UserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }
}

/// <summary>
/// Results from systematic screening process
/// </summary>
public class SystematicScreeningResults
{
    public Guid ExecutionId { get; set; }
    public int TotalDocuments { get; set; }
    public int MustReadCount { get; set; }
    public int RelevantCount { get; set; }
    public int ContributingCount { get; set; }
    public List<DocumentScreeningResult> DocumentResults { get; set; } = new();
}

/// <summary>
/// Individual document screening result
/// </summary>
public class DocumentScreeningResult
{
    public Guid DocumentId { get; set; }
    public string Title { get; set; } = "";
    public string Authors { get; set; } = "";
    public double RelevanceScore { get; set; }
    public double ContributionScore { get; set; }
    public bool IsRelevant { get; set; }
    public bool IsContributing { get; set; }
    public bool IsMustRead { get; set; }
    public string? RelevanceReasoning { get; set; }
    public string? ContributionReasoning { get; set; }
    public List<string>? Topics { get; set; }
    public List<string>? Keywords { get; set; }
    public List<string>? Entities { get; set; }
}