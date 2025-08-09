namespace Veritheia.Data.Entities;

/// <summary>
/// Tracks which processes users can access
/// </summary>
public class ProcessCapability : BaseEntity
{
    public Guid UserId { get; set; }
    public string ProcessType { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public User User { get; set; } = null!;
}