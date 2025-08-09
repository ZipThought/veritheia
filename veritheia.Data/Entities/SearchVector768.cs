using Pgvector;

namespace Veritheia.Data.Entities;

/// <summary>
/// 768-dimensional vector storage (E5, BGE models)
/// </summary>
public class SearchVector768
{
    public Guid IndexId { get; set; }
    public Vector Embedding { get; set; } = null!;
    
    // Navigation properties
    public SearchIndex Index { get; set; } = null!;
}