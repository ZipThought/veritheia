using Veritheia.Data.DTOs;
using Veritheia.Common.Models;

namespace veritheia.Web.Services;

/// <summary>
/// API service wrapper for user operations
/// Implements Pattern A: Simple Identifier Authentication
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
    /// Create or get user with optional display name
    /// </summary>
    public async Task<UserDto> CreateOrGetUserAsync(string email, string? displayName = null)
    {
        var request = new CreateUserRequest
        {
            Email = email,
            DisplayName = displayName ?? string.Empty
        };
        return await _apiClient.PostAsync<UserDto>("api/users", request);
    }

    /// <summary>
    /// Update user profile information
    /// </summary>
    public async Task UpdateUserAsync(Guid userId, UserUpdateRequest request)
    {
        await _apiClient.PutAsync<object>($"api/users/{userId}", request);
    }

    /// <summary>
    /// Update user's last active timestamp
    /// </summary>
    public async Task UpdateLastActiveAsync(Guid userId)
    {
        await _apiClient.PutAsync($"api/users/{userId}/activity");
    }
}
