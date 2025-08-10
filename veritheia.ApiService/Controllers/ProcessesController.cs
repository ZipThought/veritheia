using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Veritheia.Core.Services;

namespace Veritheia.ApiService.Controllers;

/// <summary>
/// Process Engine API - Execute analytical workflows
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProcessesController : ControllerBase
{
    private readonly ProcessEngine _processEngine;
    
    public ProcessesController(ProcessEngine processEngine)
    {
        _processEngine = processEngine;
    }
    
    /// <summary>
    /// Get available processes
    /// </summary>
    [HttpGet("available")]
    public IActionResult GetAvailableProcesses()
    {
        var processes = _processEngine.GetAvailableProcesses();
        return Ok(processes);
    }
    
    /// <summary>
    /// Execute a process within a journey
    /// </summary>
    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteProcess([FromBody] ExecuteProcessRequest request)
    {
        try
        {
            var result = await _processEngine.ExecuteProcessAsync(
                request.ProcessId,
                request.JourneyId,
                request.Inputs);
            
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Process execution failed", details = ex.Message });
        }
    }
    
    /// <summary>
    /// Get execution history for a journey
    /// </summary>
    [HttpGet("journey/{journeyId}/executions")]
    public async Task<IActionResult> GetJourneyExecutions(Guid journeyId)
    {
        var executions = await _processEngine.GetJourneyExecutionsAsync(journeyId);
        
        return Ok(executions.Select(e => new
        {
            e.Id,
            e.ProcessDefinitionId,
            e.StartedAt,
            e.CompletedAt,
            e.Status,
            e.ErrorMessage,
            ResultCount = e.Results.Count
        }));
    }
    
    public class ExecuteProcessRequest
    {
        public string ProcessId { get; set; } = string.Empty;
        public Guid JourneyId { get; set; }
        public Dictionary<string, object> Inputs { get; set; } = new();
    }
}