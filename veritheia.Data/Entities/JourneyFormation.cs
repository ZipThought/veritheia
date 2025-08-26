using Veritheia.Data.Interfaces;

namespace Veritheia.Data.Entities;

/// <summary>
/// Insights and understanding developed through journey engagement
/// </summary>
public class JourneyFormation : BaseEntity, IUserOwned
{
    // Partition key - required for composite primary key (UserId, Id)
    public Guid UserId { get; set; }
    
    public Guid JourneyId { get; set; }
    public string InsightType { get; set; } = string.Empty; // pattern, connection, synthesis, etc.
    public string InsightContent { get; set; } = string.Empty;
    public Dictionary<string, object>? FormedFromSegments { get; set; }
    public Dictionary<string, object>? FormedThroughQuestions { get; set; }
    
    // Navigation properties
    public Journey Journey { get; set; } = null!;
}