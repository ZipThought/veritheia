using Veritheia.Data.Interfaces;

namespace Veritheia.Data.Entities;

/// <summary>
/// User's intellectual framework for a specific journey
/// </summary>
public class JourneyFramework : BaseEntity, IUserOwned
{
    // Partition key - required for composite primary key (UserId, Id)
    public Guid UserId { get; set; }

    public Guid JourneyId { get; set; }
    public string JourneyType { get; set; } = string.Empty;
    public Dictionary<string, object> FrameworkElements { get; set; } = new();
    public Dictionary<string, object> ProjectionRules { get; set; } = new();

    // Navigation properties
    public Journey Journey { get; set; } = null!;
}