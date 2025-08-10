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
/// Persona development service - MVP 4.4
/// Manages evolving representation of user's intellectual style
/// </summary>
public class PersonaService
{
    private readonly VeritheiaDbContext _db;
    private readonly ILogger<PersonaService> _logger;
    
    public PersonaService(VeritheiaDbContext dbContext, ILogger<PersonaService> logger)
    {
        _db = dbContext;
        _logger = logger;
    }
    
    /// <summary>
    /// Create a new persona for a user
    /// </summary>
    public async Task<Persona> CreatePersonaAsync(Guid userId, string domain)
    {
        var persona = new Persona
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Domain = domain,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ConceptualVocabulary = new Dictionary<string, object>(),
            Patterns = new List<object>(),
            MethodologicalPreferences = new List<object>(),
            Markers = new List<object>()
        };
        
        _db.Personas.Add(persona);
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Created {Domain} persona for user {UserId}", domain, userId);
        
        return persona;
    }
    
    /// <summary>
    /// Get active personas for a user
    /// </summary>
    public async Task<List<Persona>> GetActivePersonasAsync(Guid userId)
    {
        return await _db.Personas
            .Where(p => p.UserId == userId && p.IsActive)
            .OrderBy(p => p.Domain)
            .ToListAsync();
    }
    
    /// <summary>
    /// Get persona with journey history
    /// </summary>
    public async Task<Persona?> GetPersonaWithJourneysAsync(Guid personaId)
    {
        return await _db.Personas
            .Include(p => p.Journeys)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == personaId);
    }
    
    /// <summary>
    /// Update conceptual vocabulary based on journey activity
    /// </summary>
    public async Task UpdateConceptualVocabularyAsync(
        Guid personaId,
        Dictionary<string, int> termFrequencies)
    {
        var persona = await _db.Personas.FindAsync(personaId);
        if (persona == null)
            throw new InvalidOperationException($"Persona {personaId} not found");
        
        // Merge new term frequencies with existing vocabulary
        foreach (var term in termFrequencies)
        {
            if (persona.ConceptualVocabulary.ContainsKey(term.Key))
            {
                var currentFreq = Convert.ToInt32(persona.ConceptualVocabulary[term.Key]);
                persona.ConceptualVocabulary[term.Key] = currentFreq + term.Value;
            }
            else
            {
                persona.ConceptualVocabulary[term.Key] = term.Value;
            }
        }
        
        persona.LastEvolved = DateTime.UtcNow;
        persona.UpdatedAt = DateTime.UtcNow;
        
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Updated vocabulary for persona {PersonaId} with {TermCount} terms", 
            personaId, termFrequencies.Count);
    }
    
    /// <summary>
    /// Identify user's inquiry patterns from journey activity
    /// MVP 4.4.2: Pattern Recognition
    /// </summary>
    public async Task<List<object>> IdentifyPatternsAsync(Guid personaId)
    {
        var persona = await _db.Personas
            .Include(p => p.Journeys)
                .ThenInclude(j => j.Journals)
                    .ThenInclude(j => j.Entries)
            .FirstOrDefaultAsync(p => p.Id == personaId);
        
        if (persona == null)
            throw new InvalidOperationException($"Persona {personaId} not found");
        
        // Basic pattern recognition from journal entries
        var patterns = new List<object>();
        
        // Analyze journal entries for patterns
        var entries = persona.Journeys
            .SelectMany(j => j.Journals)
            .SelectMany(j => j.Entries)
            .OrderBy(e => e.CreatedAt)
            .ToList();
        
        if (entries.Any())
        {
            // Identify common tags
            var commonTags = entries
                .SelectMany(e => e.Tags)
                .GroupBy(t => t)
                .Where(g => g.Count() > 2)
                .Select(g => new { Tag = g.Key, Count = g.Count() });
            
            foreach (var tag in commonTags)
            {
                patterns.Add(new
                {
                    Type = "recurring_topic",
                    Value = tag.Tag,
                    Frequency = tag.Count,
                    IdentifiedAt = DateTime.UtcNow
                });
            }
        }
        
        // Update persona patterns
        persona.Patterns = patterns;
        persona.LastEvolved = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        
        return patterns;
    }
    
    /// <summary>
    /// Adapt process context to user's style
    /// MVP 4.4.3: Context Personalization
    /// </summary>
    public async Task<Dictionary<string, object>> GetPersonalizedContextAsync(Guid personaId)
    {
        var persona = await _db.Personas.FindAsync(personaId);
        if (persona == null)
            throw new InvalidOperationException($"Persona {personaId} not found");
        
        // Return personalized context based on vocabulary and patterns
        return new Dictionary<string, object>
        {
            ["vocabulary"] = persona.ConceptualVocabulary.Take(100),
            ["patterns"] = persona.Patterns,
            ["domain"] = persona.Domain
        };
    }
    
    /// <summary>
    /// Get persona evolution timeline
    /// </summary>
    public async Task<PersonaEvolution> GetPersonaEvolutionAsync(Guid personaId)
    {
        var persona = await _db.Personas
            .Include(p => p.Journeys)
            .FirstOrDefaultAsync(p => p.Id == personaId);
        
        if (persona == null)
            throw new InvalidOperationException($"Persona {personaId} not found");
        
        return new PersonaEvolution
        {
            PersonaId = personaId,
            Domain = persona.Domain,
            CreatedAt = persona.CreatedAt,
            LastEvolved = persona.LastEvolved,
            JourneyCount = persona.Journeys.Count,
            VocabularySize = persona.ConceptualVocabulary.Count,
            PatternCount = persona.Patterns.Count,
            PreferenceCount = persona.MethodologicalPreferences.Count,
            MarkerCount = persona.Markers.Count
        };
    }
    
}

public class PersonaEvolution
{
    public Guid PersonaId { get; set; }
    public string Domain { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastEvolved { get; set; }
    public int JourneyCount { get; set; }
    public int VocabularySize { get; set; }
    public int PatternCount { get; set; }
    public int PreferenceCount { get; set; }
    public int MarkerCount { get; set; }
}