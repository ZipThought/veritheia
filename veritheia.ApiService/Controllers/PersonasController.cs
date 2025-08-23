using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Veritheia.Data.Services;

namespace Veritheia.ApiService.Controllers;

/// <summary>
/// Persona management API - MVP 4.3
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PersonasController : ControllerBase
{
    private readonly PersonaService _personaService;
    private readonly UserService _userService;
    
    public PersonasController(PersonaService personaService, UserService userService)
    {
        _personaService = personaService;
        _userService = userService;
    }
    
    /// <summary>
    /// Get all personas for user
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUserPersonas([FromQuery] Guid? userId = null)
    {
        // For MVP, use demo user if not specified
        var actualUserId = userId ?? (await _userService.GetDemoUserAsync()).Id;
        
        var personas = await _personaService.GetUserPersonasAsync(actualUserId);
        return Ok(personas);
    }
    
    /// <summary>
    /// Get active personas for user
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActivePersonas([FromQuery] Guid? userId = null)
    {
        // For MVP, use demo user if not specified
        var actualUserId = userId ?? (await _userService.GetDemoUserAsync()).Id;
        
        var personas = await _personaService.GetActivePersonasAsync(actualUserId);
        return Ok(personas);
    }
    
    /// <summary>
    /// Get persona by ID
    /// </summary>
    [HttpGet("{personaId}")]
    public async Task<IActionResult> GetPersona(Guid personaId, [FromQuery] Guid? userId = null)
    {
        // For MVP, use demo user if not specified
        var actualUserId = userId ?? (await _userService.GetDemoUserAsync()).Id;
        
        var persona = await _personaService.GetPersonaAsync(actualUserId, personaId);
        if (persona == null)
            return NotFound();
            
        return Ok(persona);
    }
    
    /// <summary>
    /// Create custom persona
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreatePersona([FromBody] CreatePersonaRequest request)
    {
        try
        {
            // For MVP, use demo user if not specified
            var userId = request.UserId ?? (await _userService.GetDemoUserAsync()).Id;
            
            var persona = await _personaService.CreatePersonaAsync(
                userId,
                request.Domain,
                request.ConceptualVocabulary);
                
            return CreatedAtAction(nameof(GetPersona), new { personaId = persona.Id }, persona);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    public class CreatePersonaRequest
    {
        public Guid? UserId { get; set; }
        public string Domain { get; set; } = string.Empty;
        public Dictionary<string, object>? ConceptualVocabulary { get; set; }
    }
}