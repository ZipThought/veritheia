using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Veritheia.Data.Services;

namespace Veritheia.ApiService.Controllers;

/// <summary>
/// Journey management API - MVP 4.2
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class JourneysController : ControllerBase
{
    private readonly JourneyService _journeyService;
    private readonly UserService _userService;
    
    public JourneysController(JourneyService journeyService, UserService userService)
    {
        _journeyService = journeyService;
        _userService = userService;
    }
    
    /// <summary>
    /// Create new journey
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateJourney([FromBody] CreateJourneyRequest request)
    {
        try
        {
            // For MVP, use demo user if not specified
            var userId = request.UserId ?? (await _userService.GetDemoUserAsync()).Id;
            
            var journey = await _journeyService.CreateJourneyAsync(
                userId,
                request.Purpose,
                request.PersonaId,
                request.ProcessType);
            
            return CreatedAtAction(nameof(GetJourney), new { journeyId = journey.Id }, journey);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Get journey by ID
    /// </summary>
    [HttpGet("{journeyId}")]
    public async Task<IActionResult> GetJourney(Guid journeyId, [FromQuery] Guid? userId = null)
    {
        // For MVP, use demo user if not specified
        var actualUserId = userId ?? (await _userService.GetDemoUserAsync()).Id;
        
        var journey = await _journeyService.GetJourneyAsync(actualUserId, journeyId);
        if (journey == null)
            return NotFound();
            
        return Ok(journey);
    }
    
    /// <summary>
    /// Get all journeys for user
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUserJourneys([FromQuery] Guid? userId = null)
    {
        // For MVP, use demo user if not specified
        var actualUserId = userId ?? (await _userService.GetDemoUserAsync()).Id;
        
        var journeys = await _journeyService.GetUserJourneysAsync(actualUserId);
        return Ok(journeys);
    }
    
    /// <summary>
    /// Update journey
    /// </summary>
    [HttpPut("{journeyId}")]
    public async Task<IActionResult> UpdateJourney(Guid journeyId, [FromBody] UpdateJourneyRequest request)
    {
        try
        {
            // For MVP, use demo user if not specified
            var userId = request.UserId ?? (await _userService.GetDemoUserAsync()).Id;
            
            var journey = await _journeyService.UpdateJourneyAsync(
                userId, 
                journeyId, 
                request.State, 
                request.Context);
                
            return Ok(journey);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Archive journey
    /// </summary>
    [HttpDelete("{journeyId}")]
    public async Task<IActionResult> ArchiveJourney(Guid journeyId, [FromQuery] Guid? userId = null)
    {
        try
        {
            // For MVP, use demo user if not specified
            var actualUserId = userId ?? (await _userService.GetDemoUserAsync()).Id;
            
            await _journeyService.ArchiveJourneyAsync(actualUserId, journeyId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Get journey statistics for user
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetJourneyStatistics([FromQuery] Guid? userId = null)
    {
        // For MVP, use demo user if not specified
        var actualUserId = userId ?? (await _userService.GetDemoUserAsync()).Id;
        
        var statistics = await _journeyService.GetJourneyStatisticsAsync(actualUserId);
        return Ok(statistics);
    }

    public class CreateJourneyRequest
    {
        public Guid? UserId { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public Guid PersonaId { get; set; }
        public string ProcessType { get; set; } = string.Empty;
    }

    public class UpdateJourneyRequest
    {
        public Guid? UserId { get; set; }
        public string? State { get; set; }
        public Dictionary<string, object>? Context { get; set; }
    }
}