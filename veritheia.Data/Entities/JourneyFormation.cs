namespace Veritheia.Data.Entities;

/// <summary>
/// Accumulated insights from journeys - formation through engagement
/// </summary>
public class JourneyFormation : BaseEntity
{
    public Guid JourneyId { get; set; }
    
    // What was formed
    public string InsightType { get; set; } = string.Empty; // conceptual, methodological, theoretical
    public string InsightContent { get; set; } = string.Empty;
    
    // How it was formed
    public Dictionary<string, object>? FormedFromSegments { get; set; } // {"segments": [uuid1, uuid2, ...]}
    public Dictionary<string, object>? FormedThroughQuestions { get; set; } // {"questions": ["RQ1", "RQ2", ...]}
    public string? FormationReasoning { get; set; }
    
    // When in the journey
    public string? FormationMarker { get; set; } // Milestone or marker reached
    public DateTime FormedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Journey Journey { get; set; } = null!;
}