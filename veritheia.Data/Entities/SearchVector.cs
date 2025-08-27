using Pgvector;
using Veritheia.Data.Interfaces;

namespace Veritheia.Data.Entities;

/// <summary>
/// Unified vector storage with orthogonal transformation isolation
/// Supports 384, 768, and 1536 dimensional vectors
/// </summary>
public class SearchVector : BaseEntity, IUserOwned
{
    // Partition key - required for composite primary key (UserId, Id)
    public Guid UserId { get; set; }
    
    public Guid SegmentId { get; set; }
    public Guid? JourneyId { get; set; } // Optional: for journey filtering within user space
    
    /// <summary>
    /// Vector dimension (384, 768, or 1536)
    /// </summary>
    public int Dimension { get; set; }
    
    /// <summary>
    /// The orthogonally transformed embedding vector
    /// Stored at maximum dimension (1536) but actual dimension specified in Dimension field
    /// </summary>
    public Vector Embedding { get; set; } = null!;
    
    /// <summary>
    /// Model used to generate the embedding (before transformation)
    /// </summary>
    public string VectorModel { get; set; } = string.Empty;
    
    // Navigation properties
    public JourneyDocumentSegment Segment { get; set; } = null!;
}