using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Veritheia.Core.Enums;
using Veritheia.Data.Entities;

namespace Veritheia.Data.Services;

/// <summary>
/// Service for managing user journeys - the core formative experience
/// Implements journey projection spaces where documents are transformed according to user frameworks
/// </summary>
public class JourneyService
{
    private readonly VeritheiaDbContext _context;
    private readonly ILogger<JourneyService> _logger;

    public JourneyService(VeritheiaDbContext context, ILogger<JourneyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all journeys for a specific user
    /// </summary>
    public async Task<List<Journey>> GetUserJourneysAsync(Guid userId)
    {
        _logger.LogInformation("Retrieving journeys for user {UserId}", userId);
        
        return await _context.Journeys
            .Where(j => j.UserId == userId)
            .Include(j => j.Persona)
            .Include(j => j.ProcessExecutions)
            .OrderByDescending(j => j.UpdatedAt ?? j.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get a specific journey by ID for a user
    /// </summary>
    public async Task<Journey?> GetJourneyAsync(Guid userId, Guid journeyId)
    {
        _logger.LogInformation("Retrieving journey {JourneyId} for user {UserId}", journeyId, userId);
        
        return await _context.Journeys
            .Where(j => j.UserId == userId && j.Id == journeyId)
            .Include(j => j.Persona)
            .Include(j => j.ProcessExecutions)
            .Include(j => j.Framework)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Create a new journey for formation through authorship
    /// </summary>
    public async Task<Journey> CreateJourneyAsync(Guid userId, string purpose, Guid personaId, string processType)
    {
        _logger.LogInformation("Creating new journey for user {UserId} with purpose: {Purpose}", userId, purpose);

        // Verify persona belongs to user
        var persona = await _context.Personas
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Id == personaId);
        
        if (persona == null)
        {
            throw new ArgumentException($"Persona {personaId} not found for user {userId}");
        }

        var journey = new Journey
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            PersonaId = personaId,
            ProcessType = processType,
            Purpose = purpose,
            State = JourneyState.Active.ToString(),
            Context = new Dictionary<string, object>(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Journeys.Add(journey);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created journey {JourneyId} for user {UserId}", journey.Id, userId);
        return journey;
    }

    /// <summary>
    /// Update journey state and context
    /// </summary>
    public async Task<Journey> UpdateJourneyAsync(Guid userId, Guid journeyId, string? state = null, Dictionary<string, object>? context = null)
    {
        var journey = await GetJourneyAsync(userId, journeyId);
        if (journey == null)
        {
            throw new ArgumentException($"Journey {journeyId} not found for user {userId}");
        }

        if (state != null)
        {
            journey.State = state;
        }

        if (context != null)
        {
            journey.Context = context;
        }

        journey.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated journey {JourneyId} for user {UserId}", journeyId, userId);
        return journey;
    }

    /// <summary>
    /// Get journey statistics for dashboard display
    /// </summary>
    public async Task<JourneyStatistics> GetJourneyStatisticsAsync(Guid userId)
    {
        var journeys = await _context.Journeys
            .Where(j => j.UserId == userId)
            .Include(j => j.ProcessExecutions)
            .ToListAsync();

        var stats = new JourneyStatistics
        {
            TotalJourneys = journeys.Count,
            ActiveJourneys = journeys.Count(j => j.State == JourneyState.Active.ToString()),
            CompletedJourneys = journeys.Count(j => j.State == JourneyState.Completed.ToString()),
            ProcessingJourneys = journeys.Count(j => j.ProcessExecutions.Any(pe => pe.State == "Running")),
            RecentActivity = journeys
                .SelectMany(j => j.ProcessExecutions)
                .OrderByDescending(pe => pe.CreatedAt)
                .Take(5)
                .Select(pe => new RecentActivityItem
                {
                    JourneyId = pe.JourneyId,
                    ProcessType = pe.ProcessType,
                    Timestamp = pe.CreatedAt,
                    Description = $"{pe.ProcessType}: {pe.State}"
                })
                .ToList()
        };

        return stats;
    }

    /// <summary>
    /// Archive a journey (soft delete)
    /// </summary>
    public async Task ArchiveJourneyAsync(Guid userId, Guid journeyId)
    {
        var journey = await GetJourneyAsync(userId, journeyId);
        if (journey == null)
        {
            throw new ArgumentException($"Journey {journeyId} not found for user {userId}");
        }

        journey.State = JourneyState.Abandoned.ToString();
        journey.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Archived journey {JourneyId} for user {UserId}", journeyId, userId);
    }
}

/// <summary>
/// Journey statistics for dashboard display
/// </summary>
public class JourneyStatistics
{
    public int TotalJourneys { get; set; }
    public int ActiveJourneys { get; set; }
    public int CompletedJourneys { get; set; }
    public int ProcessingJourneys { get; set; }
    public List<RecentActivityItem> RecentActivity { get; set; } = new();
}

/// <summary>
/// Recent activity item for dashboard
/// </summary>
public class RecentActivityItem
{
    public Guid JourneyId { get; set; }
    public string ProcessType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Description { get; set; } = string.Empty;
}