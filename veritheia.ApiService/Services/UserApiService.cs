using Veritheia.Data.Entities;
using Veritheia.Data.Services;

namespace Veritheia.ApiService.Services;

/// <summary>
/// ApiService layer for user operations
/// Handles business logic, access control, and coordinates with Data layer
/// Implements Pattern A: Simple Identifier Authentication
/// </summary>
public class UserApiService
{
    private readonly UserService _userService;

    public UserApiService(UserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Get user by ID with access control
    /// </summary>
    public async Task<User?> GetUserAsync(Guid userId)
    {
        // TODO: Add access control logic here
        return await _userService.GetUserAsync(userId);
    }

    /// <summary>
    /// Get user by email with access control
    /// </summary>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        // TODO: Add access control logic here
        return await _userService.GetUserByEmailAsync(email);
    }

    /// <summary>
    /// Create or get user with business logic
    /// </summary>
    public async Task<User> CreateOrGetUserAsync(string email, string? displayName = null)
    {
        // TODO: Add business logic validation here
        return await _userService.CreateOrGetUserAsync(email, displayName);
    }

    /// <summary>
    /// Update user with access control and validation
    /// </summary>
    public async Task UpdateUserAsync(Guid userId, string? displayName)
    {
        // TODO: Add access control and business logic here
        await _userService.UpdateUserAsync(userId, displayName);
    }

    /// <summary>
    /// Update user's last active timestamp
    /// </summary>
    public async Task UpdateLastActiveAsync(Guid userId)
    {
        // TODO: Add access control logic here
        await _userService.UpdateLastActiveAsync(userId);
    }
}