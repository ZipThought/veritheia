using Microsoft.AspNetCore.Mvc;
using veritheia.Common.Models;

namespace veritheia.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;
    private readonly IConfiguration _configuration;

    public HealthController(ILogger<HealthController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Get the health status of the API
    /// </summary>
    /// <returns>Health status information</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<HealthCheckResponse>), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        _logger.LogInformation("Health check endpoint called");
        
        var response = new ApiResponse<HealthCheckResponse>
        {
            Success = true,
            Data = new HealthCheckResponse
            {
                Status = "Healthy",
                Service = "Veritheia API Service",
                Version = "v1",
                Timestamp = DateTime.UtcNow,
                Environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Unknown"
            }
        };
        
        return Ok(response);
    }

    /// <summary>
    /// Get detailed health status including dependencies
    /// </summary>
    /// <returns>Detailed health status information</returns>
    [HttpGet("detailed")]
    [ProducesResponseType(typeof(ApiResponse<DetailedHealthCheckResponse>), StatusCodes.Status200OK)]
    public IActionResult GetDetailed()
    {
        _logger.LogInformation("Detailed health check endpoint called");
        
        var response = new ApiResponse<DetailedHealthCheckResponse>
        {
            Success = true,
            Data = new DetailedHealthCheckResponse
            {
                Status = "Healthy",
                Service = "Veritheia API Service",
                Version = "v1",
                Timestamp = DateTime.UtcNow,
                Environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Unknown",
                Uptime = GetUptime(),
                Dependencies = new Dictionary<string, string>
                {
                    { "Database", "Not configured" },
                    { "CognitiveService", "Not configured" }
                }
            }
        };
        
        return Ok(response);
    }

    private TimeSpan GetUptime()
    {
        return DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime();
    }
}