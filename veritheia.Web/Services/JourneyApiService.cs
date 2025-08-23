using Veritheia.Data.DTOs;

namespace veritheia.Web.Services;

/// <summary>
/// API service wrapper for journey operations
/// Calls the API service instead of accessing database directly
/// </summary>
public class JourneyApiService
{
    private readonly ApiClient _apiClient;

    public JourneyApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// Get all journeys for user
    /// </summary>
    public async Task<List<JourneyDto>> GetUserJourneysAsync(Guid? userId = null)
    {
        var endpoint = userId.HasValue ? $"api/journeys?userId={userId}" : "api/journeys";
        return await _apiClient.GetListAsync<JourneyDto>(endpoint);
    }

    /// <summary>
    /// Get journey by ID
    /// </summary>
    public async Task<JourneyDto?> GetJourneyAsync(Guid journeyId, Guid? userId = null)
    {
        var endpoint = userId.HasValue ? $"api/journeys/{journeyId}?userId={userId}" : $"api/journeys/{journeyId}";
        return await _apiClient.GetAsync<JourneyDto>(endpoint);
    }

    /// <summary>
    /// Create new journey
    /// </summary>
    public async Task<JourneyDto> CreateJourneyAsync(string purpose, Guid personaId, string processType, Guid? userId = null)
    {
        var request = new CreateJourneyRequest
        {
            UserId = userId,
            Purpose = purpose,
            PersonaId = personaId,
            ProcessType = processType
        };
        return await _apiClient.PostAsync<JourneyDto>("api/journeys", request);
    }

    /// <summary>
    /// Update journey
    /// </summary>
    public async Task<JourneyDto> UpdateJourneyAsync(Guid journeyId, string? state = null, Dictionary<string, object>? context = null, Guid? userId = null)
    {
        var request = new UpdateJourneyRequest
        {
            UserId = userId,
            State = state,
            Context = context
        };
        return await _apiClient.PutAsync<JourneyDto>($"api/journeys/{journeyId}", request);
    }

    /// <summary>
    /// Archive journey
    /// </summary>
    public async Task ArchiveJourneyAsync(Guid journeyId, Guid? userId = null)
    {
        var endpoint = userId.HasValue ? $"api/journeys/{journeyId}?userId={userId}" : $"api/journeys/{journeyId}";
        await _apiClient.DeleteAsync(endpoint);
    }

    /// <summary>
    /// Get journey statistics for user
    /// </summary>
    public async Task<JourneyStatisticsDto> GetJourneyStatisticsAsync(Guid? userId = null)
    {
        var endpoint = userId.HasValue ? $"api/journeys/statistics?userId={userId}" : "api/journeys/statistics";
        return await _apiClient.GetAsync<JourneyStatisticsDto>(endpoint) ?? new JourneyStatisticsDto();
    }
}
