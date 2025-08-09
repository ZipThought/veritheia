namespace Veritheia.Data.Entities;

/// <summary>
/// Metadata for available processes
/// </summary>
public class ProcessDefinition : BaseEntity
{
    public string ProcessType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty; // Methodological, Developmental, Analytical, Compositional, Reflective
    public string TriggerType { get; set; } = string.Empty; // Manual, UserInitiated
    public Dictionary<string, object> Inputs { get; set; } = new();
    public Dictionary<string, object> Configuration { get; set; } = new();
}