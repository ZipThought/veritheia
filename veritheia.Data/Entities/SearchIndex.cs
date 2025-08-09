namespace Veritheia.Data.Entities;

/// <summary>
/// Metadata for segment embeddings - enables polymorphic vector storage
/// </summary>
public class SearchIndex : BaseEntity
{
    public Guid SegmentId { get; set; }
    public string VectorModel { get; set; } = string.Empty; // openai-ada-002, e5-large-v2, etc.
    public int VectorDimension { get; set; } // 1536, 768, 384
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public JourneyDocumentSegment Segment { get; set; } = null!;
    
    // Polymorphic relationship to vector storage tables
    // The actual vector is stored in SearchVector1536, SearchVector768, etc.
    // based on VectorDimension value
}