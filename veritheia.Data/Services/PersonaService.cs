using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Veritheia.Data.Entities;

namespace Veritheia.Data.Services;

/// <summary>
/// Service for managing user personas - evolving representations of intellectual style
/// </summary>
public class PersonaService
{
    private readonly VeritheiaDbContext _context;
    private readonly ILogger<PersonaService> _logger;

    public PersonaService(VeritheiaDbContext context, ILogger<PersonaService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all personas for a specific user
    /// </summary>
    public async Task<List<Persona>> GetUserPersonasAsync(Guid userId)
    {
        _logger.LogInformation("Retrieving personas for user {UserId}", userId);

        var personas = await _context.Personas
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Domain)
            .ToListAsync();
            
        // If no personas exist, create defaults (fallback for existing users)
        if (!personas.Any())
        {
            _logger.LogWarning("No personas found for user {UserId}, creating defaults", userId);
            await CreateDefaultPersonasAsync(userId);
            
            // Re-fetch after creation
            personas = await _context.Personas
                .Where(p => p.UserId == userId)
                .OrderBy(p => p.Domain)
                .ToListAsync();
        }
        
        _logger.LogInformation("Found {Count} personas for user {UserId}", personas.Count, userId);
        return personas;
    }

    /// <summary>
    /// Get active personas for a user (for dropdown selection)
    /// </summary>
    public async Task<List<Persona>> GetActivePersonasAsync(Guid userId)
    {
        return await _context.Personas
            .Where(p => p.UserId == userId && p.IsActive)
            .OrderBy(p => p.Domain)
            .ToListAsync();
    }

    /// <summary>
    /// Get a specific persona by ID for a user
    /// </summary>
    public async Task<Persona?> GetPersonaAsync(Guid userId, Guid personaId)
    {
        return await _context.Personas
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Id == personaId);
    }

    /// <summary>
    /// Create default personas for a new user
    /// </summary>
    public async Task CreateDefaultPersonasAsync(Guid userId)
    {
        _logger.LogInformation("Creating default personas for user {UserId}", userId);

        var defaultPersonas = new[]
        {
            new Persona
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                Domain = "Researcher",
                IsActive = true,
                ConceptualVocabulary = new Dictionary<string, object>
                {
                    ["methodological_terms"] = new[] { "systematic review", "meta-analysis", "empirical evidence", "theoretical framework" },
                    ["assessment_criteria"] = new[] { "relevance", "contribution", "methodology", "validity" }
                },
                Patterns = new List<object>
                {
                    new { type = "research_question", structure = "How/What/Why + domain + specific aspect" },
                    new { type = "evidence_evaluation", criteria = new[] { "empirical_basis", "sample_size", "methodology" } }
                },
                MethodologicalPreferences = new List<object>
                {
                    new { approach = "systematic", rigor = "high", evidence_threshold = 0.7 }
                },
                CreatedAt = DateTime.UtcNow
            },
            new Persona
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                Domain = "Student",
                IsActive = true,
                ConceptualVocabulary = new Dictionary<string, object>
                {
                    ["learning_terms"] = new[] { "understanding", "comprehension", "analysis", "synthesis" },
                    ["assessment_focus"] = new[] { "clarity", "relevance", "difficulty", "practical_application" }
                },
                Patterns = new List<object>
                {
                    new { type = "learning_objective", structure = "I want to understand + topic + for purpose" },
                    new { type = "difficulty_assessment", levels = new[] { "beginner", "intermediate", "advanced" } }
                },
                MethodologicalPreferences = new List<object>
                {
                    new { approach = "exploratory", rigor = "medium", evidence_threshold = 0.5 }
                },
                CreatedAt = DateTime.UtcNow
            },
            new Persona
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                Domain = "Entrepreneur",
                IsActive = true,
                ConceptualVocabulary = new Dictionary<string, object>
                {
                    ["business_terms"] = new[] { "market opportunity", "competitive advantage", "scalability", "business model" },
                    ["assessment_focus"] = new[] { "market_potential", "feasibility", "risk", "innovation" }
                },
                Patterns = new List<object>
                {
                    new { type = "opportunity_evaluation", criteria = new[] { "market_size", "competition", "barriers_to_entry" } },
                    new { type = "risk_assessment", factors = new[] { "technical_risk", "market_risk", "execution_risk" } }
                },
                MethodologicalPreferences = new List<object>
                {
                    new { approach = "pragmatic", rigor = "medium", evidence_threshold = 0.6 }
                },
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Personas.AddRange(defaultPersonas);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created {Count} default personas for user {UserId}", defaultPersonas.Length, userId);
    }

    /// <summary>
    /// Create a custom persona
    /// </summary>
    public async Task<Persona> CreatePersonaAsync(Guid userId, string domain, Dictionary<string, object>? vocabulary = null)
    {
        var persona = new Persona
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Domain = domain,
            IsActive = true,
            ConceptualVocabulary = vocabulary ?? new Dictionary<string, object>(),
            Patterns = new List<object>(),
            MethodologicalPreferences = new List<object>(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Personas.Add(persona);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created custom persona {PersonaId} for user {UserId}", persona.Id, userId);
        return persona;
    }
}