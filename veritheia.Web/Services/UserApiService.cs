using Veritheia.Data.DTOs;
using Veritheia.Data.Services;
using Veritheia.Common.Models;
using AutoMapper;

namespace veritheia.Web.Services;

/// <summary>
/// User service wrapper for Web component
/// Uses direct method calls to business logic services
/// Implements Pattern A: Simple Identifier Authentication
/// </summary>
public class UserApiService
{
    private readonly UserService _userService;
    private readonly IMapper _mapper;

    public UserApiService(UserService userService, IMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    public async Task<UserDto?> GetUserAsync(Guid userId)
    {
        var user = await _userService.GetUserAsync(userId);
        return user != null ? _mapper.Map<UserDto>(user) : null;
    }

    /// <summary>
    /// Get user by email
    /// </summary>
    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var user = await _userService.GetUserByEmailAsync(email);
        return user != null ? _mapper.Map<UserDto>(user) : null;
    }

    /// <summary>
    /// Create or get user with optional display name
    /// </summary>
    public async Task<UserDto> CreateOrGetUserAsync(string email, string? displayName = null)
    {
        var user = await _userService.CreateOrGetUserAsync(email, displayName ?? string.Empty);
        return _mapper.Map<UserDto>(user);
    }

    /// <summary>
    /// Update user profile information
    /// </summary>
    public async Task UpdateUserAsync(Guid userId, UserUpdateRequest request)
    {
        await _userService.UpdateUserAsync(userId, request.DisplayName ?? string.Empty);
    }

    /// <summary>
    /// Update user's last active timestamp
    /// </summary>
    public async Task UpdateLastActiveAsync(Guid userId)
    {
        await _userService.UpdateLastActiveAsync(userId);
    }
}
