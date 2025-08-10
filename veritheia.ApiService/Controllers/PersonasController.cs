using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Veritheia.Core.Services;

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
    /// Record method preference
    /// </summary>
    [HttpPost("{personaId}/preferences")]
    public async Task<IActionResult> RecordMethodPreference(
        Guid personaId,
        [FromBody] MethodPreferenceRequest request)
    {
        try
        {
            await _personaService.RecordMethodPreferenceAsync(
                personaId,
                request.MethodName,
                request.Context,
                request.Effectiveness);
            
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Add pattern observation
    /// </summary>
    [HttpPost("{personaId}/patterns")]
    public async Task<IActionResult> AddPattern(
        Guid personaId,
        [FromBody] PatternRequest request)
    {
        try
        {
            await _personaService.AddPatternObservationAsync(
                personaId,
                request.PatternType,
                request.Description,
                request.Evidence);
            
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Add intellectual marker
    /// </summary>
    [HttpPost("{personaId}/markers")]
    public async Task<IActionResult> AddMarker(
        Guid personaId,
        [FromBody] MarkerRequest request)
    {
        try
        {
            await _personaService.AddIntellectualMarkerAsync(
                personaId,
                request.MarkerType,
                request.Value,
                request.Confidence);
            
            return NoContent();
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
    
    /// <summary>
    /// Clone persona for new domain
    /// </summary>
    [HttpPost("{personaId}/clone")]
    public async Task<IActionResult> ClonePersona(
        Guid personaId,
        [FromBody] ClonePersonaRequest request)
    {
        try
        {
            var clone = await _personaService.ClonePersonaForDomainAsync(
                personaId,
                request.NewDomain);
            
            return CreatedAtAction(nameof(GetPersona), new { personaId = clone.Id }, clone);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Deactivate persona
    /// </summary>
    [HttpPost("{personaId}/deactivate")]
    public async Task<IActionResult> DeactivatePersona(Guid personaId)
    {
        try
        {
            await _personaService.DeactivatePersonaAsync(personaId);
            return NoContent();
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
    
    public class MethodPreferenceRequest
    {
        public string MethodName { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public double Effectiveness { get; set; }
    }
    
    public class PatternRequest
    {
        public string PatternType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, object>? Evidence { get; set; }
    }
    
    public class MarkerRequest
    {
        public string MarkerType { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }
    
    public class ClonePersonaRequest
    {
        public string NewDomain { get; set; } = string.Empty;
    }
}