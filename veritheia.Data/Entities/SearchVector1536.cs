using Pgvector;

namespace Veritheia.Data.Entities;

/// <summary>
/// 1536-dimensional vector storage (OpenAI, Cohere models)
/// </summary>
public class SearchVector1536
{
    public Guid IndexId { get; set; }
    public Vector Embedding { get; set; } = null!;
    
    // Navigation properties
    public SearchIndex Index { get; set; } = null!;
}