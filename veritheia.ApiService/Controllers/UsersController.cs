using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Veritheia.Data.Services;
using Veritheia.Data.DTOs;

namespace Veritheia.ApiService.Controllers;

/// <summary>
/// User management API - MVP 4.1
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    
    public UsersController(UserService userService)
    {
        _userService = userService;
    }
    
    /// <summary>
    /// Get current user (demo user for MVP)
    /// </summary>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userService.GetDemoUserAsync();
        return Ok(user);
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
    /// Create or get user
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrGetUser([FromBody] CreateUserRequest request)
    {
        var user = await _userService.CreateOrGetUserAsync(request.Email, request.DisplayName);
        return Ok(user);
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