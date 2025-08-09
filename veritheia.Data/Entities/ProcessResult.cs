namespace Veritheia.Data.Entities;

/// <summary>
/// Stores process outputs - extensible through JSONB data field
/// </summary>
public class ProcessResult : BaseEntity
{
    public Guid ExecutionId { get; set; }
    public string ProcessType { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new(); // JSONB for flexible results
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime ExecutedAt { get; set; }
    
    // Navigation properties
    public ProcessExecution Execution { get; set; } = null!;
}