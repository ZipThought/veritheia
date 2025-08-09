namespace Veritheia.Data.Entities;

/// <summary>
/// Journey-specific assessment of segments
/// AI measures within the user's projection space
/// </summary>
public class JourneySegmentAssessment : BaseEntity
{
    public Guid SegmentId { get; set; }
    
    // Assessment details
    public string AssessmentType { get; set; } = string.Empty; // relevance, contribution, rubric_match
    public int? ResearchQuestionId { get; set; } // Which RQ this assesses
    
    // Scores based on journey type
    public float? RelevanceScore { get; set; }
    public float? ContributionScore { get; set; }
    public Dictionary<string, object>? RubricScores { get; set; } // For educational journeys
    
    // Reasoning preservation
    public string? AssessmentReasoning { get; set; }
    public Dictionary<string, object>? ReasoningChain { get; set; } // Chain-of-thought steps
    
    // Model tracking
    public string? AssessedByModel { get; set; }
    public DateTime AssessedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public JourneyDocumentSegment Segment { get; set; } = null!;
}