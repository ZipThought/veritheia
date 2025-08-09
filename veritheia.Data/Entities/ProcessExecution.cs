namespace Veritheia.Data.Entities;

/// <summary>
/// Tracks process runs within journeys
/// </summary>
public class ProcessExecution : BaseEntity
{
    public Guid JourneyId { get; set; }
    public string ProcessType { get; set; } = string.Empty;
    public string State { get; set; } = "Pending"; // Pending, Running, Completed, Failed, Cancelled
    public Dictionary<string, object> Inputs { get; set; } = new();
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    
    // Navigation properties
    public Journey Journey { get; set; } = null!;
    public ProcessResult? Result { get; set; }
}