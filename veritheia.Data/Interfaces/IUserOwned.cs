namespace Veritheia.Data.Interfaces;

/// <summary>
/// Marker interface for entities that belong to a specific user partition
/// </summary>
public interface IUserOwned
{
    /// <summary>
    /// The user ID that owns this entity - used for partition enforcement
    /// </summary>
    Guid UserId { get; set; }
}
