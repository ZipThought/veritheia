using NpgsqlTypes;
using Veritheia.Data.Interfaces;

namespace Veritheia.Data.Entities;

/// <summary>
/// Documents transformed according to journey-specific intellectual framework
/// </summary>
public class JourneyDocumentSegment : BaseEntity, IUserOwned
{
    // Partition key - required for composite primary key (UserId, Id)
    public Guid UserId { get; set; }
    
    public Guid JourneyId { get; set; }
    public Guid DocumentId { get; set; }
    public int SequenceIndex { get; set; }
    public string SegmentContent { get; set; } = string.Empty;
    public string? SegmentType { get; set; } // paragraph, section, concept, etc.
    public Dictionary<string, object> StructuralPath { get; set; } = new();
    public NpgsqlTypes.NpgsqlRange<int>? ByteRange { get; set; }
    public string? CreatedByRule { get; set; }
    public string? CreatedForQuestion { get; set; }
    
    // Navigation properties
    public Journey Journey { get; set; } = null!;
    public Document Document { get; set; } = null!;
    public ICollection<JourneySegmentAssessment> Assessments { get; set; } = new List<JourneySegmentAssessment>();
    public ICollection<SearchIndex> SearchIndexes { get; set; } = new List<SearchIndex>();
}