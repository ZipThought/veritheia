using NpgsqlTypes;

namespace Veritheia.Data.Entities;

/// <summary>
/// Documents projected into journey-specific segments
/// This is where documents gain meaning through journey frameworks
/// </summary>
public class JourneyDocumentSegment : BaseEntity
{
    public Guid JourneyId { get; set; }
    public Guid DocumentId { get; set; }
    
    // Content shaped by journey's projection rules
    public string SegmentContent { get; set; } = string.Empty;
    public string? SegmentType { get; set; } // abstract, methodology, paragraph, etc.
    public string? SegmentPurpose { get; set; } // Why this segment exists for this journey
    
    // Structure and position
    public Dictionary<string, object>? StructuralPath { get; set; } // {"path": ["section-2", "subsection-3", "paragraph-5"]}
    public int SequenceIndex { get; set; }
    public NpgsqlRange<int>? ByteRange { get; set; } // Original position in document
    
    // Projection metadata
    public string? CreatedByRule { get; set; } // Which segmentation rule created this
    public string? CreatedForQuestion { get; set; } // Which research question drove this
    
    // Navigation properties
    public Journey Journey { get; set; } = null!;
    public Document Document { get; set; } = null!;
    public ICollection<SearchIndex> SearchIndexes { get; set; } = new List<SearchIndex>();
    public ICollection<JourneySegmentAssessment> Assessments { get; set; } = new List<JourneySegmentAssessment>();
}