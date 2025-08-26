using Veritheia.Data.Entities;
using Veritheia.Data.Services;

namespace Veritheia.ApiService.Services;

/// <summary>
/// ApiService layer for persona operations
/// Handles business logic, access control, and coordinates with Data layer
/// </summary>
public class PersonaApiService
{
    private readonly PersonaService _personaService;

    public PersonaApiService(PersonaService personaService)
    {
        _personaService = personaService;
    }

    /// <summary>
    /// Get personas for user with access control
    /// </summary>
    public async Task<List<Persona>> GetUserPersonasAsync(Guid userId)
    {
        // TODO: Add access control logic here
        return await _personaService.GetUserPersonasAsync(userId);
    }

    /// <summary>
    /// Get persona by ID with access control
    /// </summary>
    public async Task<Persona?> GetPersonaAsync(Guid userId, Guid personaId)
    {
        // TODO: Add access control logic here
        return await _personaService.GetPersonaAsync(userId, personaId);
    }

    /// <summary>
    /// Create persona with business logic validation
    /// </summary>
    public async Task<Persona> CreatePersonaAsync(Guid userId, string domain, Dictionary<string, object>? vocabulary = null)
    {
        // TODO: Add business logic validation here
        return await _personaService.CreatePersonaAsync(userId, domain, vocabulary);
    }

    // TODO: Add UpdatePersonaAsync and DeletePersonaAsync methods to PersonaService in Data layer
    // These methods don't exist in the Data layer PersonaService yet
}