using Veritheia.Data.Interfaces;

namespace Veritheia.Data.Entities;

/// <summary>
/// User's access to specific process types
/// </summary>
public class ProcessCapability : BaseEntity, IUserOwned
{
    // Partition key - required for composite primary key (UserId, Id)
    public Guid UserId { get; set; }

    public string ProcessType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? LastUsed { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}