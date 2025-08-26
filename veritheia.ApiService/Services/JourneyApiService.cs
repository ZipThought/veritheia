using Veritheia.Data.Entities;
using Veritheia.Data.Services;

namespace Veritheia.ApiService.Services;

/// <summary>
/// ApiService layer for journey operations
/// Handles business logic, access control, and coordinates with Data layer
/// </summary>
public class JourneyApiService
{
    private readonly JourneyService _journeyService;

    public JourneyApiService(JourneyService journeyService)
    {
        _journeyService = journeyService;
    }

    /// <summary>
    /// Get journeys for user with access control
    /// </summary>
    public async Task<List<Journey>> GetUserJourneysAsync(Guid userId)
    {
        // TODO: Add access control logic here
        return await _journeyService.GetUserJourneysAsync(userId);
    }

    /// <summary>
    /// Get journey by ID with access control
    /// </summary>
    public async Task<Journey?> GetJourneyAsync(Guid userId, Guid journeyId)
    {
        // TODO: Add access control logic here
        return await _journeyService.GetJourneyAsync(userId, journeyId);
    }

    /// <summary>
    /// Create journey with business logic validation
    /// </summary>
    public async Task<Journey> CreateJourneyAsync(Guid userId, Guid personaId, string processType, string purpose)
    {
        // TODO: Add business logic validation here
        return await _journeyService.CreateJourneyAsync(userId, purpose, personaId, processType);
    }

    /// <summary>
    /// Update journey with access control and validation
    /// </summary>
    public async Task<Journey> UpdateJourneyAsync(Guid userId, Guid journeyId, string? state = null, Dictionary<string, object>? context = null)
    {
        // TODO: Add access control and business logic here
        return await _journeyService.UpdateJourneyAsync(userId, journeyId, state, context);
    }

    /// <summary>
    /// Get journey statistics for user dashboard
    /// </summary>
    public async Task<JourneyStatistics> GetJourneyStatisticsAsync(Guid userId)
    {
        // TODO: Add access control logic here
        return await _journeyService.GetJourneyStatisticsAsync(userId);
    }

    /// <summary>
    /// Delete journey with access control
    /// </summary>
    public async Task ArchiveJourneyAsync(Guid userId, Guid journeyId)
    {
        // TODO: Add access control logic here
        await _journeyService.ArchiveJourneyAsync(userId, journeyId);
    }
}