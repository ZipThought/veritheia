using Veritheia.Data.Interfaces;

namespace Veritheia.Data.Entities;

/// <summary>
/// Results from process executions
/// </summary>
public class ProcessResult : BaseEntity, IUserOwned
{
    // Partition key - required for composite primary key (UserId, Id)
    public Guid UserId { get; set; }

    public Guid ExecutionId { get; set; }
    public string ProcessType { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
    public Dictionary<string, object>? Metadata { get; set; }

    // Navigation properties
    public ProcessExecution Execution { get; set; } = null!;
}