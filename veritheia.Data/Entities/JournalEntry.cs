namespace Veritheia.Data.Entities;

/// <summary>
/// Individual narrative entries in journals
/// </summary>
public class JournalEntry : BaseEntity
{
    public Guid JournalId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Significance { get; set; } = "Routine"; // Routine, Notable, Critical, Milestone
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    // Navigation properties
    public Journal Journal { get; set; } = null!;
}