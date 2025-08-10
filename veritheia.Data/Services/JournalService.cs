using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Veritheia.Data;
using Veritheia.Data.Entities;

namespace Veritheia.Data.Services;

/// <summary>
/// Journal management service - MVP 4.3
/// Handles structured journaling with edge-linking and long-memory
/// </summary>
public class JournalService
{
    private readonly VeritheiaDbContext _db;
    private readonly ILogger<JournalService> _logger;
    
    public JournalService(VeritheiaDbContext dbContext, ILogger<JournalService> logger)
    {
        _db = dbContext;
        _logger = logger;
    }
    
    /// <summary>
    /// Create a new journal within a journey
    /// </summary>
    public async Task<Journal> CreateJournalAsync(Guid journeyId, string type)
    {
        // Validate journal type
        var validTypes = new[] { "Research", "Method", "Decision", "Reflection" };
        if (!validTypes.Contains(type))
            throw new ArgumentException($"Invalid journal type: {type}");
        
        var journal = new Journal
        {
            Id = Guid.CreateVersion7(),
            JourneyId = journeyId,
            Type = type,
            IsShareable = false, // Default to private
            CreatedAt = DateTime.UtcNow
        };
        
        _db.Journals.Add(journal);
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Created {Type} journal for journey {JourneyId}", type, journeyId);
        
        return journal;
    }
    
    /// <summary>
    /// Add entry to journal
    /// </summary>
    public async Task<JournalEntry> AddJournalEntryAsync(
        Guid journalId,
        string content,
        string significance = "Routine",
        List<string>? tags = null,
        Dictionary<string, object>? metadata = null)
    {
        var entry = new JournalEntry
        {
            Id = Guid.CreateVersion7(),
            JournalId = journalId,
            Content = content,
            Significance = significance,
            Tags = tags ?? new List<string>(),
            Metadata = metadata ?? new Dictionary<string, object>(),
            CreatedAt = DateTime.UtcNow
        };
        
        _db.JournalEntries.Add(entry);
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Added {Significance} entry to journal {JournalId}", significance, journalId);
        
        return entry;
    }
    
    /// <summary>
    /// Get journals for a journey
    /// </summary>
    public async Task<List<Journal>> GetJourneyJournalsAsync(Guid journeyId)
    {
        return await _db.Journals
            .Where(j => j.JourneyId == journeyId)
            .Include(j => j.Entries)
            .OrderBy(j => j.CreatedAt)
            .ToListAsync();
    }
    
    /// <summary>
    /// Get journal with all entries
    /// </summary>
    public async Task<Journal?> GetJournalWithEntriesAsync(Guid journalId)
    {
        return await _db.Journals
            .Include(j => j.Entries.OrderBy(e => e.CreatedAt))
            .Include(j => j.Journey)
                .ThenInclude(j => j.User)
            .FirstOrDefaultAsync(j => j.Id == journalId);
    }
    
    /// <summary>
    /// Record method decision in Method journal
    /// </summary>
    public async Task<JournalEntry> RecordMethodDecisionAsync(
        Guid journeyId,
        string methodName,
        string rationale,
        Dictionary<string, object>? parameters = null)
    {
        // Find or create Method journal
        var journal = await _db.Journals
            .FirstOrDefaultAsync(j => j.JourneyId == journeyId && j.Type == "Method");
        
        if (journal == null)
        {
            journal = await CreateJournalAsync(journeyId, "Method");
        }
        
        var metadata = new Dictionary<string, object>
        {
            ["method"] = methodName,
            ["timestamp"] = DateTime.UtcNow
        };
        
        if (parameters != null)
        {
            metadata["parameters"] = parameters;
        }
        
        return await AddJournalEntryAsync(
            journal.Id,
            $"Selected method: {methodName}\n\nRationale: {rationale}",
            "Notable",
            new List<string> { "method", methodName },
            metadata
        );
    }
    
    /// <summary>
    /// Record research insight
    /// </summary>
    public async Task<JournalEntry> RecordResearchInsightAsync(
        Guid journeyId,
        string insight,
        string? source = null,
        string significance = "Notable")
    {
        // Find or create Research journal
        var journal = await _db.Journals
            .FirstOrDefaultAsync(j => j.JourneyId == journeyId && j.Type == "Research");
        
        if (journal == null)
        {
            journal = await CreateJournalAsync(journeyId, "Research");
        }
        
        var metadata = new Dictionary<string, object>
        {
            ["timestamp"] = DateTime.UtcNow
        };
        
        if (!string.IsNullOrEmpty(source))
        {
            metadata["source"] = source;
        }
        
        return await AddJournalEntryAsync(
            journal.Id,
            insight,
            significance,
            new List<string> { "insight", "research" },
            metadata
        );
    }
    
    /// <summary>
    /// Record critical decision
    /// </summary>
    public async Task<JournalEntry> RecordDecisionAsync(
        Guid journeyId,
        string decision,
        string reasoning,
        List<string>? alternatives = null)
    {
        // Find or create Decision journal
        var journal = await _db.Journals
            .FirstOrDefaultAsync(j => j.JourneyId == journeyId && j.Type == "Decision");
        
        if (journal == null)
        {
            journal = await CreateJournalAsync(journeyId, "Decision");
        }
        
        var content = $"Decision: {decision}\n\nReasoning: {reasoning}";
        
        if (alternatives != null && alternatives.Any())
        {
            content += $"\n\nConsidered alternatives:\n- {string.Join("\n- ", alternatives)}";
        }
        
        var metadata = new Dictionary<string, object>
        {
            ["timestamp"] = DateTime.UtcNow,
            ["decision"] = decision
        };
        
        if (alternatives != null)
        {
            metadata["alternatives"] = alternatives;
        }
        
        return await AddJournalEntryAsync(
            journal.Id,
            content,
            "Critical",
            new List<string> { "decision", "critical" },
            metadata
        );
    }
    
    /// <summary>
    /// Add reflection entry
    /// </summary>
    public async Task<JournalEntry> AddReflectionAsync(
        Guid journeyId,
        string reflection,
        string? topic = null)
    {
        // Find or create Reflection journal
        var journal = await _db.Journals
            .FirstOrDefaultAsync(j => j.JourneyId == journeyId && j.Type == "Reflection");
        
        if (journal == null)
        {
            journal = await CreateJournalAsync(journeyId, "Reflection");
        }
        
        var tags = new List<string> { "reflection" };
        if (!string.IsNullOrEmpty(topic))
        {
            tags.Add(topic);
        }
        
        return await AddJournalEntryAsync(
            journal.Id,
            reflection,
            "Routine",
            tags,
            new Dictionary<string, object> { ["timestamp"] = DateTime.UtcNow }
        );
    }
    
    /// <summary>
    /// Search journal entries across a journey
    /// </summary>
    public async Task<List<JournalEntry>> SearchJournalEntriesAsync(
        Guid journeyId,
        string? searchText = null,
        string? significance = null,
        string? journalType = null)
    {
        var query = _db.JournalEntries
            .Include(e => e.Journal)
            .Where(e => e.Journal.JourneyId == journeyId);
        
        if (!string.IsNullOrEmpty(searchText))
        {
            query = query.Where(e => e.Content.Contains(searchText));
        }
        
        if (!string.IsNullOrEmpty(significance))
        {
            query = query.Where(e => e.Significance == significance);
        }
        
        if (!string.IsNullOrEmpty(journalType))
        {
            query = query.Where(e => e.Journal.Type == journalType);
        }
        
        return await query
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }
    
    /// <summary>
    /// Get milestone entries (Critical or Milestone significance)
    /// </summary>
    public async Task<List<JournalEntry>> GetMilestoneEntriesAsync(Guid journeyId)
    {
        return await _db.JournalEntries
            .Include(e => e.Journal)
            .Where(e => e.Journal.JourneyId == journeyId &&
                       (e.Significance == "Critical" || e.Significance == "Milestone"))
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();
    }
    
    /// <summary>
    /// Mark journal as shareable
    /// </summary>
    public async Task SetJournalShareabilityAsync(Guid journalId, bool isShareable)
    {
        var journal = await _db.Journals.FindAsync(journalId);
        if (journal == null)
            throw new InvalidOperationException($"Journal {journalId} not found");
        
        journal.IsShareable = isShareable;
        journal.UpdatedAt = DateTime.UtcNow;
        
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Set journal {JournalId} shareability to {IsShareable}", 
            journalId, isShareable);
    }
}