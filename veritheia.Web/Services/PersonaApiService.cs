using Veritheia.Data.DTOs;
using Veritheia.Data.Services;
using AutoMapper;

namespace veritheia.Web.Services;

/// <summary>
/// Persona service wrapper for Web component
/// Uses direct method calls to business logic services
/// </summary>
public class PersonaApiService
{
    private readonly PersonaService _personaService;
    private readonly IMapper _mapper;

    public PersonaApiService(PersonaService personaService, IMapper mapper)
    {
        _personaService = personaService;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all personas for user
    /// </summary>
    public async Task<List<PersonaDto>> GetUserPersonasAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required");

        var personas = await _personaService.GetUserPersonasAsync(userId);
        return _mapper.Map<List<PersonaDto>>(personas);
    }

    /// <summary>
    /// Get active personas for user (for dropdown selection)
    /// </summary>
    public async Task<List<PersonaDto>> GetActivePersonasAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required");

        var personas = await _personaService.GetActivePersonasAsync(userId);
        return _mapper.Map<List<PersonaDto>>(personas);
    }

    /// <summary>
    /// Get persona by ID
    /// </summary>
    public async Task<PersonaDto?> GetPersonaAsync(Guid userId, Guid personaId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required");

        var persona = await _personaService.GetPersonaAsync(userId, personaId);
        return persona != null ? _mapper.Map<PersonaDto>(persona) : null;
    }

    /// <summary>
    /// Create custom persona
    /// </summary>
    public async Task<PersonaDto> CreatePersonaAsync(Guid userId, string domain, Dictionary<string, object>? vocabulary = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required");

        var persona = await _personaService.CreatePersonaAsync(userId, domain, vocabulary ?? new Dictionary<string, object>());
        return _mapper.Map<PersonaDto>(persona);
    }
}
