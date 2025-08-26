using Veritheia.Data.Interfaces;

namespace Veritheia.Data.Entities;

/// <summary>
/// AI measurements of segments within journey projection space
/// </summary>
public class JourneySegmentAssessment : BaseEntity, IUserOwned
{
    // Partition key - required for composite primary key (UserId, Id)
    public Guid UserId { get; set; }

    public Guid SegmentId { get; set; }
    public string AssessmentType { get; set; } = string.Empty; // relevance, contribution, quality, etc.
    public double Score { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public Dictionary<string, object>? RubricScores { get; set; }
    public Dictionary<string, object>? ReasoningChain { get; set; }
    public string? AssessedByModel { get; set; }

    // Navigation properties
    public JourneyDocumentSegment Segment { get; set; } = null!;
}