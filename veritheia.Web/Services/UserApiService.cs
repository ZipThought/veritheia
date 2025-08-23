using Veritheia.Data.DTOs;

namespace veritheia.Web.Services;

/// <summary>
/// API service wrapper for user operations
/// Calls the API service instead of accessing database directly
/// </summary>
public class UserApiService
{
    private readonly ApiClient _apiClient;

    public UserApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// Get current user (demo user for MVP)
    /// </summary>
    public async Task<UserDto?> GetCurrentUserAsync()
    {
        return await _apiClient.GetAsync<UserDto>("api/users/current");
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    public async Task<UserDto?> GetUserAsync(Guid userId)
    {
        return await _apiClient.GetAsync<UserDto>($"api/users/{userId}");
    }

    /// <summary>
    /// Get user by email
    /// </summary>
    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        return await _apiClient.GetAsync<UserDto>($"api/users/by-email/{email}");
    }

    /// <summary>
    /// Create or get user
    /// </summary>
    public async Task<UserDto> CreateOrGetUserAsync(string email, string displayName)
    {
        var request = new CreateUserRequest
        {
            Email = email,
            DisplayName = displayName
        };
        return await _apiClient.PostAsync<UserDto>("api/users", request);
    }

    /// <summary>
    /// Update user's last active timestamp
    /// </summary>
    public async Task UpdateLastActiveAsync(Guid userId)
    {
        await _apiClient.PutAsync($"api/users/{userId}/activity");
    }


}
