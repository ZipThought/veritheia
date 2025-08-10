using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Veritheia.Data.Services;

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
    /// Get current user (single-user MVP)
    /// </summary>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userService.GetOrCreateDefaultUserAsync();
        return Ok(user);
    }
    
    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUser(Guid userId)
    {
        var user = await _userService.GetUserWithContextAsync(userId);
        if (user == null)
            return NotFound();
        
        return Ok(user);
    }
    
    /// <summary>
    /// Update user profile
    /// </summary>
    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserRequest request)
    {
        try
        {
            await _userService.UpdateUserProfileAsync(userId, request.Name, request.Email);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Get user statistics
    /// </summary>
    [HttpGet("{userId}/statistics")]
    public async Task<IActionResult> GetUserStatistics(Guid userId)
    {
        var stats = await _userService.GetUserStatisticsAsync(userId);
        return Ok(stats);
    }
    
    public class UpdateUserRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}