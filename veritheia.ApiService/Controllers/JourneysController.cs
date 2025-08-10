using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Veritheia.Core.Services;

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
        // For MVP, use default user if not specified
        var userId = request.UserId ?? (await _userService.GetOrCreateDefaultUserAsync()).Id;
        
        var journey = await _journeyService.CreateJourneyAsync(
            userId,
            request.PersonaId,
            request.Purpose);
        
        return CreatedAtAction(nameof(GetJourney), new { journeyId = journey.Id }, journey);
    }
    
    /// <summary>
    /// Get journey by ID
    /// </summary>
    [HttpGet("{journeyId}")]
    public async Task<IActionResult> GetJourney(Guid journeyId)
    {
        var journey = await _journeyService.GetJourneyWithProjectionsAsync(journeyId);
        if (journey == null)
            return NotFound();
        
        return Ok(journey);
    }
    
    /// <summary>
    /// Get active journeys for user
    /// </summary>
    [HttpGet("user/{userId}/active")]
    public async Task<IActionResult> GetActiveJourneys(Guid userId)
    {
        var journeys = await _journeyService.GetActiveJourneysAsync(userId);
        return Ok(journeys);
    }
    
    /// <summary>
    /// Resume a journey
    /// </summary>
    [HttpPost("{journeyId}/resume")]
    public async Task<IActionResult> ResumeJourney(Guid journeyId)
    {
        var journey = await _journeyService.ResumeJourneyAsync(journeyId);
        if (journey == null)
            return NotFound();
        
        return Ok(journey);
    }
    
    /// <summary>
    /// Update journey state
    /// </summary>
    [HttpPut("{journeyId}/state")]
    public async Task<IActionResult> UpdateJourneyState(
        Guid journeyId,
        [FromBody] UpdateStateRequest request)
    {
        try
        {
            await _journeyService.UpdateJourneyStateAsync(journeyId, request.State);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Archive a journey
    /// </summary>
    [HttpPost("{journeyId}/archive")]
    public async Task<IActionResult> ArchiveJourney(Guid journeyId)
    {
        try
        {
            await _journeyService.ArchiveJourneyAsync(journeyId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
    
    public class CreateJourneyRequest
    {
        public Guid? UserId { get; set; }
        public Guid PersonaId { get; set; }
        public string Purpose { get; set; } = string.Empty;
    }
    
    public class UpdateStateRequest
    {
        public string State { get; set; } = string.Empty;
    }
}