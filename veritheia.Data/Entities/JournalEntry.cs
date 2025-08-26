using Veritheia.Data.Interfaces;

namespace Veritheia.Data.Entities;

/// <summary>
/// Individual entries within journals
/// </summary>
public class JournalEntry : BaseEntity, IUserOwned
{
    // Partition key - required for composite primary key (UserId, Id)
    public Guid UserId { get; set; }

    public Guid JournalId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Significance { get; set; } = string.Empty; // Routine, Notable, Critical, Milestone
    public string[]? Tags { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }

    // Navigation properties
    public Journal Journal { get; set; } = null!;
}