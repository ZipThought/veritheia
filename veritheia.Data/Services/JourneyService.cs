using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Veritheia.Data;
using Veritheia.Data.Entities;

namespace Veritheia.Data.Services;

/// <summary>
/// Journey management service using EF Core directly
/// Post-DDD: No repository abstraction, direct DbContext usage
/// </summary>
public class JourneyService
{
    private readonly VeritheiaDbContext _db;
    
    public JourneyService(VeritheiaDbContext dbContext)
    {
        _db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    
    /// <summary>
    /// Create a new journey for a user
    /// PostgreSQL enforces all constraints via FKs
    /// </summary>
    public async Task<Journey> CreateJourneyAsync(
        Guid userId, 
        Guid personaId,
        string purpose)
    {
        var journey = new Journey
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            PersonaId = personaId,
            Purpose = purpose,
            State = "Active",
            CreatedAt = DateTime.UtcNow
        };
        
        _db.Journeys.Add(journey);
        await _db.SaveChangesAsync();
        
        return journey;
    }
    
    /// <summary>
    /// Get active journeys for a user
    /// Direct LINQ query, no abstraction needed
    /// </summary>
    public async Task<List<Journey>> GetActiveJourneysAsync(Guid userId)
    {
        return await _db.Journeys
            .Where(j => j.UserId == userId && j.State == "Active")
            .Include(j => j.Persona)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
    }
    
    /// <summary>
    /// Get journey with full projection space
    /// </summary>
    public async Task<Journey?> GetJourneyWithProjectionsAsync(Guid journeyId)
    {
        return await _db.Journeys
            .Include(j => j.Persona)
            .Include(j => j.User)
            .Include(j => j.DocumentSegments)
                .ThenInclude(s => s.Document)
            .Include(j => j.Journals)
                .ThenInclude(j => j.Entries)
            .FirstOrDefaultAsync(j => j.Id == journeyId);
    }
    
    /// <summary>
    /// Update journey state
    /// </summary>
    public async Task UpdateJourneyStateAsync(Guid journeyId, string newState)
    {
        var journey = await _db.Journeys.FindAsync(journeyId);
        if (journey == null)
            throw new InvalidOperationException($"Journey {journeyId} not found");
        
        journey.State = newState;
        journey.UpdatedAt = DateTime.UtcNow;
        
        // Journey doesn't have CompletedAt, state tracking is sufficient
        
        await _db.SaveChangesAsync();
    }
    
    /// <summary>
    /// Resume a journey - updates state and last accessed time
    /// </summary>
    public async Task<Journey?> ResumeJourneyAsync(Guid journeyId)
    {
        var journey = await _db.Journeys
            .Include(j => j.Persona)
            .FirstOrDefaultAsync(j => j.Id == journeyId);
        
        if (journey == null)
            return null;
        
        journey.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        
        return journey;
    }
    
    /// <summary>
    /// Archive a journey
    /// </summary>
    public async Task ArchiveJourneyAsync(Guid journeyId)
    {
        await UpdateJourneyStateAsync(journeyId, "Archived");
    }
}