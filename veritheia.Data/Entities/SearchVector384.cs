using Pgvector;
using Veritheia.Data.Interfaces;

namespace Veritheia.Data.Entities;

/// <summary>
/// 384-dimensional vector storage (sentence-transformers models)
/// </summary>
public class SearchVector384 : BaseEntity, IUserOwned
{
    // Partition key - required for composite primary key (UserId, IndexId)
    public Guid UserId { get; set; }

    public Guid IndexId { get; set; }
    public Vector Embedding { get; set; } = null!;

    // Navigation properties
    public SearchIndex Index { get; set; } = null!;
}