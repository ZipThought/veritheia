using Pgvector;

namespace Veritheia.Data.Entities;

/// <summary>
/// 384-dimensional vector storage (lightweight models)
/// </summary>
public class SearchVector384
{
    public Guid IndexId { get; set; }
    public Vector Embedding { get; set; } = null!;
    
    // Navigation properties
    public SearchIndex Index { get; set; } = null!;
}