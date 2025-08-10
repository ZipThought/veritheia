using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Veritheia.Data.Services;

namespace Veritheia.ApiService.Controllers;

/// <summary>
/// Persona development API - MVP 4.4
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PersonasController : ControllerBase
{
    private readonly PersonaService _personaService;
    
    public PersonasController(PersonaService personaService)
    {
        _personaService = personaService;
    }
    
    /// <summary>
    /// Create new persona
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreatePersona([FromBody] CreatePersonaRequest request)
    {
        var persona = await _personaService.CreatePersonaAsync(request.UserId, request.Domain);
        return CreatedAtAction(nameof(GetPersona), new { personaId = persona.Id }, persona);
    }
    
    /// <summary>
    /// Get persona with journeys
    /// </summary>
    [HttpGet("{personaId}")]
    public async Task<IActionResult> GetPersona(Guid personaId)
    {
        var persona = await _personaService.GetPersonaWithJourneysAsync(personaId);
        if (persona == null)
            return NotFound();
        
        return Ok(persona);
    }
    
    /// <summary>
    /// Get active personas for user
    /// </summary>
    [HttpGet("user/{userId}/active")]
    public async Task<IActionResult> GetActivePersonas(Guid userId)
    {
        var personas = await _personaService.GetActivePersonasAsync(userId);
        return Ok(personas);
    }
    
    /// <summary>
    /// Update conceptual vocabulary
    /// </summary>
    [HttpPost("{personaId}/vocabulary")]
    public async Task<IActionResult> UpdateVocabulary(
        Guid personaId,
        [FromBody] Dictionary<string, int> termFrequencies)
    {
        try
        {
            await _personaService.UpdateConceptualVocabularyAsync(personaId, termFrequencies);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Identify patterns in persona
    /// </summary>
    [HttpPost("{personaId}/patterns/identify")]
    public async Task<IActionResult> IdentifyPatterns(Guid personaId)
    {
        try
        {
            var patterns = await _personaService.IdentifyPatternsAsync(personaId);
            return Ok(patterns);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Get personalized context
    /// </summary>
    [HttpGet("{personaId}/context")]
    public async Task<IActionResult> GetPersonalizedContext(Guid personaId)
    {
        try
        {
            var context = await _personaService.GetPersonalizedContextAsync(personaId);
            return Ok(context);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Get persona evolution
    /// </summary>
    [HttpGet("{personaId}/evolution")]
    public async Task<IActionResult> GetEvolution(Guid personaId)
    {
        try
        {
            var evolution = await _personaService.GetPersonaEvolutionAsync(personaId);
            return Ok(evolution);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
    
    
    public class CreatePersonaRequest
    {
        public Guid UserId { get; set; }
        public string Domain { get; set; } = string.Empty;
    }
    
}