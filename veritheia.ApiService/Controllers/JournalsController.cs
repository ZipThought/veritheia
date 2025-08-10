using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Veritheia.Core.Services;

namespace Veritheia.ApiService.Controllers;

/// <summary>
/// Journal management API - MVP 4.3
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class JournalsController : ControllerBase
{
    private readonly JournalService _journalService;
    
    public JournalsController(JournalService journalService)
    {
        _journalService = journalService;
    }
    
    /// <summary>
    /// Create new journal for journey
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateJournal([FromBody] CreateJournalRequest request)
    {
        try
        {
            var journal = await _journalService.CreateJournalAsync(
                request.JourneyId,
                request.Type);
            
            return CreatedAtAction(nameof(GetJournal), new { journalId = journal.Id }, journal);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Get journal with entries
    /// </summary>
    [HttpGet("{journalId}")]
    public async Task<IActionResult> GetJournal(Guid journalId)
    {
        var journal = await _journalService.GetJournalWithEntriesAsync(journalId);
        if (journal == null)
            return NotFound();
        
        return Ok(journal);
    }
    
    /// <summary>
    /// Get journals for journey
    /// </summary>
    [HttpGet("journey/{journeyId}")]
    public async Task<IActionResult> GetJourneyJournals(Guid journeyId)
    {
        var journals = await _journalService.GetJourneyJournalsAsync(journeyId);
        return Ok(journals);
    }
    
    /// <summary>
    /// Add journal entry
    /// </summary>
    [HttpPost("{journalId}/entries")]
    public async Task<IActionResult> AddJournalEntry(
        Guid journalId,
        [FromBody] AddEntryRequest request)
    {
        var entry = await _journalService.AddJournalEntryAsync(
            journalId,
            request.Content,
            request.Significance ?? "Routine",
            request.Tags,
            request.Metadata);
        
        return CreatedAtAction(nameof(GetJournal), new { journalId }, entry);
    }
    
    /// <summary>
    /// Record method decision
    /// </summary>
    [HttpPost("journey/{journeyId}/method")]
    public async Task<IActionResult> RecordMethodDecision(
        Guid journeyId,
        [FromBody] MethodDecisionRequest request)
    {
        var entry = await _journalService.RecordMethodDecisionAsync(
            journeyId,
            request.MethodName,
            request.Rationale,
            request.Parameters);
        
        return Ok(entry);
    }
    
    /// <summary>
    /// Record research insight
    /// </summary>
    [HttpPost("journey/{journeyId}/insight")]
    public async Task<IActionResult> RecordInsight(
        Guid journeyId,
        [FromBody] InsightRequest request)
    {
        var entry = await _journalService.RecordResearchInsightAsync(
            journeyId,
            request.Insight,
            request.Source,
            request.Significance ?? "Notable");
        
        return Ok(entry);
    }
    
    /// <summary>
    /// Record critical decision
    /// </summary>
    [HttpPost("journey/{journeyId}/decision")]
    public async Task<IActionResult> RecordDecision(
        Guid journeyId,
        [FromBody] DecisionRequest request)
    {
        var entry = await _journalService.RecordDecisionAsync(
            journeyId,
            request.Decision,
            request.Reasoning,
            request.Alternatives);
        
        return Ok(entry);
    }
    
    /// <summary>
    /// Add reflection
    /// </summary>
    [HttpPost("journey/{journeyId}/reflection")]
    public async Task<IActionResult> AddReflection(
        Guid journeyId,
        [FromBody] ReflectionRequest request)
    {
        var entry = await _journalService.AddReflectionAsync(
            journeyId,
            request.Reflection,
            request.Topic);
        
        return Ok(entry);
    }
    
    /// <summary>
    /// Search journal entries
    /// </summary>
    [HttpGet("journey/{journeyId}/search")]
    public async Task<IActionResult> SearchEntries(
        Guid journeyId,
        [FromQuery] string? searchText = null,
        [FromQuery] string? significance = null,
        [FromQuery] string? journalType = null)
    {
        var entries = await _journalService.SearchJournalEntriesAsync(
            journeyId,
            searchText,
            significance,
            journalType);
        
        return Ok(entries);
    }
    
    /// <summary>
    /// Get milestone entries
    /// </summary>
    [HttpGet("journey/{journeyId}/milestones")]
    public async Task<IActionResult> GetMilestones(Guid journeyId)
    {
        var entries = await _journalService.GetMilestoneEntriesAsync(journeyId);
        return Ok(entries);
    }
    
    /// <summary>
    /// Set journal shareability
    /// </summary>
    [HttpPut("{journalId}/shareability")]
    public async Task<IActionResult> SetShareability(
        Guid journalId,
        [FromBody] ShareabilityRequest request)
    {
        try
        {
            await _journalService.SetJournalShareabilityAsync(journalId, request.IsShareable);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
    
    public class CreateJournalRequest
    {
        public Guid JourneyId { get; set; }
        public string Type { get; set; } = string.Empty;
    }
    
    public class AddEntryRequest
    {
        public string Content { get; set; } = string.Empty;
        public string? Significance { get; set; }
        public List<string>? Tags { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }
    
    public class MethodDecisionRequest
    {
        public string MethodName { get; set; } = string.Empty;
        public string Rationale { get; set; } = string.Empty;
        public Dictionary<string, object>? Parameters { get; set; }
    }
    
    public class InsightRequest
    {
        public string Insight { get; set; } = string.Empty;
        public string? Source { get; set; }
        public string? Significance { get; set; }
    }
    
    public class DecisionRequest
    {
        public string Decision { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;
        public List<string>? Alternatives { get; set; }
    }
    
    public class ReflectionRequest
    {
        public string Reflection { get; set; } = string.Empty;
        public string? Topic { get; set; }
    }
    
    public class ShareabilityRequest
    {
        public bool IsShareable { get; set; }
    }
}