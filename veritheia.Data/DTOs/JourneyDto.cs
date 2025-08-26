namespace Veritheia.Data.DTOs;

/// <summary>
/// Data Transfer Object for Journey entity
/// Used for API communication between services
/// </summary>
public class JourneyDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PersonaId { get; set; }
    public string ProcessType { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public Dictionary<string, object> Context { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public PersonaDto? Persona { get; set; }
    public List<ProcessExecutionDto> ProcessExecutions { get; set; } = new();
}

/// <summary>
/// Data Transfer Object for ProcessExecution entity
/// </summary>
public class ProcessExecutionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid JourneyId { get; set; }
    public string ProcessType { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public Dictionary<string, object>? Inputs { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Statistics summary for user's journeys
/// </summary>
public class JourneyStatisticsDto
{
    public int TotalJourneys { get; set; }
    public int ActiveJourneys { get; set; }
    public int CompletedJourneys { get; set; }
    public int ProcessingJourneys { get; set; }
    public List<RecentActivityItemDto> RecentActivity { get; set; } = new();
}

/// <summary>
/// Recent activity item for dashboard
/// </summary>
public class RecentActivityItemDto
{
    public Guid JourneyId { get; set; }
    public string ProcessType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Request model for creating journeys
/// </summary>
public class CreateJourneyRequest
{
    public Guid? UserId { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public Guid PersonaId { get; set; }
    public string ProcessType { get; set; } = string.Empty;
}

/// <summary>
/// Request model for updating journeys
/// </summary>
public class UpdateJourneyRequest
{
    public Guid? UserId { get; set; }
    public string? State { get; set; }
    public Dictionary<string, object>? Context { get; set; }
}
