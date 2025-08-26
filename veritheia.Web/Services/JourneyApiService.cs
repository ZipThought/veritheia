using Veritheia.Data.DTOs;
using Veritheia.Data.Services;
using Veritheia.Data.Entities;
using AutoMapper;

namespace veritheia.Web.Services;

/// <summary>
/// Journey service wrapper for Web component
/// Uses direct method calls to business logic services
/// </summary>
public class JourneyApiService
{
    private readonly JourneyService _journeyService;
    private readonly IMapper _mapper;

    public JourneyApiService(JourneyService journeyService, IMapper mapper)
    {
        _journeyService = journeyService;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all journeys for user
    /// </summary>
    public async Task<List<JourneyDto>> GetUserJourneysAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required");

        var journeys = await _journeyService.GetUserJourneysAsync(userId);
        return _mapper.Map<List<JourneyDto>>(journeys);
    }

    /// <summary>
    /// Get journey by ID
    /// </summary>
    public async Task<JourneyDto?> GetJourneyAsync(Guid userId, Guid journeyId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required");

        var journey = await _journeyService.GetJourneyAsync(userId, journeyId);
        return journey != null ? _mapper.Map<JourneyDto>(journey) : null;
    }

    /// <summary>
    /// Create new journey
    /// </summary>
    public async Task<JourneyDto> CreateJourneyAsync(Guid userId, string purpose, Guid personaId, string processType)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required");

        var journey = await _journeyService.CreateJourneyAsync(userId, purpose, personaId, processType);
        return _mapper.Map<JourneyDto>(journey);
    }

    /// <summary>
    /// Update journey
    /// </summary>
    public async Task<JourneyDto> UpdateJourneyAsync(Guid userId, Guid journeyId, string? state = null, Dictionary<string, object>? context = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required");

        var journey = await _journeyService.UpdateJourneyAsync(userId, journeyId, state, context);
        return _mapper.Map<JourneyDto>(journey);
    }

    /// <summary>
    /// Archive journey
    /// </summary>
    public async Task ArchiveJourneyAsync(Guid userId, Guid journeyId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required");

        await _journeyService.ArchiveJourneyAsync(userId, journeyId);
    }

    /// <summary>
    /// Get journey statistics for user
    /// </summary>
    public async Task<JourneyStatisticsDto> GetJourneyStatisticsAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required");

        var statistics = await _journeyService.GetJourneyStatisticsAsync(userId);
        return _mapper.Map<JourneyStatisticsDto>(statistics);
    }
}
