using Veritheia.Data.Interfaces;

namespace Veritheia.Data.Entities;

/// <summary>
/// Process definitions available to users
/// </summary>
public class ProcessDefinition : BaseEntity, IUserOwned
{
    // Partition key - required for composite primary key (UserId, Id)
    public Guid UserId { get; set; }
    
    public string ProcessType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Methodological, Developmental, Analytical, Compositional, Reflective
    public string TriggerType { get; set; } = string.Empty; // Manual, UserInitiated
    public Dictionary<string, object> Inputs { get; set; } = new();
    public Dictionary<string, object>? Configuration { get; set; }
}