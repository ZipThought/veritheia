using Veritheia.Data.DTOs;

namespace veritheia.Web.Services;

/// <summary>
/// API service wrapper for persona operations
/// Calls the API service instead of accessing database directly
/// </summary>
public class PersonaApiService
{
    private readonly ApiClient _apiClient;

    public PersonaApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// Get all personas for user
    /// </summary>
    public async Task<List<PersonaDto>> GetUserPersonasAsync(Guid? userId = null)
    {
        var endpoint = userId.HasValue ? $"api/personas?userId={userId}" : "api/personas";
        return await _apiClient.GetListAsync<PersonaDto>(endpoint);
    }

    /// <summary>
    /// Get active personas for user (for dropdown selection)
    /// </summary>
    public async Task<List<PersonaDto>> GetActivePersonasAsync(Guid? userId = null)
    {
        var endpoint = userId.HasValue ? $"api/personas/active?userId={userId}" : "api/personas/active";
        return await _apiClient.GetListAsync<PersonaDto>(endpoint);
    }

    /// <summary>
    /// Get persona by ID
    /// </summary>
    public async Task<PersonaDto?> GetPersonaAsync(Guid personaId, Guid? userId = null)
    {
        var endpoint = userId.HasValue ? $"api/personas/{personaId}?userId={userId}" : $"api/personas/{personaId}";
        return await _apiClient.GetAsync<PersonaDto>(endpoint);
    }

    /// <summary>
    /// Create custom persona
    /// </summary>
    public async Task<PersonaDto> CreatePersonaAsync(string domain, Guid? userId = null, Dictionary<string, object>? vocabulary = null)
    {
        var request = new CreatePersonaRequest
        {
            UserId = userId,
            Domain = domain,
            ConceptualVocabulary = vocabulary
        };
        return await _apiClient.PostAsync<PersonaDto>("api/personas", request);
    }
}
