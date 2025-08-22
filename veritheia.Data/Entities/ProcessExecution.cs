using Veritheia.Data.Interfaces;

namespace Veritheia.Data.Entities;

/// <summary>
/// Execution instances of processes within journeys
/// </summary>
public class ProcessExecution : BaseEntity, IUserOwned
{
    // Partition key - required for composite primary key (UserId, Id)
    public Guid UserId { get; set; }
    
    public Guid JourneyId { get; set; }
    public string ProcessType { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty; // Pending, Running, Completed, Failed, Cancelled
    public Dictionary<string, object> Inputs { get; set; } = new();
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    
    // Navigation properties
    public Journey Journey { get; set; } = null!;
    public ProcessResult? Result { get; set; }
}