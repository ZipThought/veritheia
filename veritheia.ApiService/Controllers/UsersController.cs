using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Veritheia.Data.Services;
using Veritheia.Data.DTOs;
using Veritheia.Common.Models;

namespace Veritheia.ApiService.Controllers;

/// <summary>
/// User management API - Implements Pattern A: Simple Identifier Authentication
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : BaseController
{
    private readonly UserService _userService;
    
    public UsersController(UserService userService)
    {
        _userService = userService;
    }
    
    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUser(Guid userId)
    {
        var user = await _userService.GetUserAsync(userId);
        if (user == null)
            return NotFound();
            
        return Ok(user);
    }
    
    /// <summary>
    /// Get user by email
    /// </summary>
    [HttpGet("by-email/{email}")]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var user = await _userService.GetUserByEmailAsync(email);
        if (user == null)
            return NotFound();
            
        return Ok(user);
    }
    
    /// <summary>
    /// Create or get user with optional display name
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrGetUser([FromBody] CreateUserRequest request)
    {
        var displayName = string.IsNullOrWhiteSpace(request.DisplayName) ? null : request.DisplayName;
        var user = await _userService.CreateOrGetUserAsync(request.Email, displayName);
        return Ok(user);
    }
    
    /// <summary>
    /// Update user profile information
    /// </summary>
    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UserUpdateRequest request)
    {
        try
        {
            var displayName = string.IsNullOrWhiteSpace(request.DisplayName) ? null : request.DisplayName;
            await _userService.UpdateUserAsync(userId, displayName);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    /// <summary>
    /// Update user's last active timestamp
    /// </summary>
    [HttpPut("{userId}/activity")]
    public async Task<IActionResult> UpdateLastActive(Guid userId)
    {
        await _userService.UpdateLastActiveAsync(userId);
        return NoContent();
    }
}