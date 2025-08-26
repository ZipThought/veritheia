namespace Veritheia.Data.Entities;

/// <summary>
/// Base entity for all domain entities with UUIDv7 primary key
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// UUIDv7 primary key for temporal ordering
    /// Generated via Guid.CreateVersion7() in .NET 9
    /// </summary>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}