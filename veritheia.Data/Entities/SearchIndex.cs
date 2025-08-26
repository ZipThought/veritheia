using Veritheia.Data.Interfaces;

namespace Veritheia.Data.Entities;

/// <summary>
/// Vector search indexes for journey document segments
/// </summary>
public class SearchIndex : BaseEntity, IUserOwned
{
    // Partition key - required for composite primary key (UserId, Id)
    public Guid UserId { get; set; }
    
    public Guid SegmentId { get; set; }
    public string VectorModel { get; set; } = string.Empty; // text-embedding-3-small, text-embedding-3-large, etc.
    
    // Navigation properties
    public JourneyDocumentSegment Segment { get; set; } = null!;
}