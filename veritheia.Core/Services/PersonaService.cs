using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Veritheia.Data;
using Veritheia.Data.Entities;

namespace Veritheia.Core.Services;

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
    /// Record methodological preference
    /// </summary>
    public async Task RecordMethodPreferenceAsync(
        Guid personaId,
        string methodName,
        string context,
        double effectiveness)
    {
        var persona = await _db.Personas.FindAsync(personaId);
        if (persona == null)
            throw new InvalidOperationException($"Persona {personaId} not found");
        
        var preference = new
        {
            Method = methodName,
            Context = context,
            Effectiveness = effectiveness,
            RecordedAt = DateTime.UtcNow
        };
        
        persona.MethodologicalPreferences.Add(preference);
        persona.LastEvolved = DateTime.UtcNow;
        persona.UpdatedAt = DateTime.UtcNow;
        
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Recorded {Method} preference for persona {PersonaId}", 
            methodName, personaId);
    }
    
    /// <summary>
    /// Add pattern observation
    /// </summary>
    public async Task AddPatternObservationAsync(
        Guid personaId,
        string patternType,
        string description,
        Dictionary<string, object>? evidence = null)
    {
        var persona = await _db.Personas.FindAsync(personaId);
        if (persona == null)
            throw new InvalidOperationException($"Persona {personaId} not found");
        
        var pattern = new
        {
            Type = patternType,
            Description = description,
            Evidence = evidence ?? new Dictionary<string, object>(),
            ObservedAt = DateTime.UtcNow
        };
        
        persona.Patterns.Add(pattern);
        persona.LastEvolved = DateTime.UtcNow;
        persona.UpdatedAt = DateTime.UtcNow;
        
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Added {PatternType} pattern to persona {PersonaId}", 
            patternType, personaId);
    }
    
    /// <summary>
    /// Add intellectual marker
    /// </summary>
    public async Task AddIntellectualMarkerAsync(
        Guid personaId,
        string markerType,
        string value,
        double confidence)
    {
        var persona = await _db.Personas.FindAsync(personaId);
        if (persona == null)
            throw new InvalidOperationException($"Persona {personaId} not found");
        
        var marker = new
        {
            Type = markerType,
            Value = value,
            Confidence = confidence,
            IdentifiedAt = DateTime.UtcNow
        };
        
        persona.Markers.Add(marker);
        persona.LastEvolved = DateTime.UtcNow;
        persona.UpdatedAt = DateTime.UtcNow;
        
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Added {MarkerType} marker to persona {PersonaId}", 
            markerType, personaId);
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
    
    /// <summary>
    /// Clone persona for new domain exploration
    /// </summary>
    public async Task<Persona> ClonePersonaForDomainAsync(
        Guid sourcePersonaId,
        string newDomain)
    {
        var source = await _db.Personas.FindAsync(sourcePersonaId);
        if (source == null)
            throw new InvalidOperationException($"Source persona {sourcePersonaId} not found");
        
        var clone = new Persona
        {
            Id = Guid.CreateVersion7(),
            UserId = source.UserId,
            Domain = newDomain,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            // Clone intellectual patterns but reset domain-specific elements
            ConceptualVocabulary = new Dictionary<string, object>(),
            Patterns = new List<object>(source.Patterns),
            MethodologicalPreferences = new List<object>(source.MethodologicalPreferences),
            Markers = new List<object>()
        };
        
        _db.Personas.Add(clone);
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Cloned persona {SourceId} to new domain {Domain}", 
            sourcePersonaId, newDomain);
        
        return clone;
    }
    
    /// <summary>
    /// Deactivate persona
    /// </summary>
    public async Task DeactivatePersonaAsync(Guid personaId)
    {
        var persona = await _db.Personas.FindAsync(personaId);
        if (persona == null)
            throw new InvalidOperationException($"Persona {personaId} not found");
        
        persona.IsActive = false;
        persona.UpdatedAt = DateTime.UtcNow;
        
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Deactivated persona {PersonaId}", personaId);
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