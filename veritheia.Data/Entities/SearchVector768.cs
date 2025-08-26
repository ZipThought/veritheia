using Pgvector;
using Veritheia.Data.Interfaces;

namespace Veritheia.Data.Entities;

/// <summary>
/// 768-dimensional vector storage (E5, BGE models)
/// </summary>
public class SearchVector768 : BaseEntity, IUserOwned
{
    // Partition key - required for composite primary key (UserId, IndexId)
    public Guid UserId { get; set; }

    public Guid IndexId { get; set; }
    public Vector Embedding { get; set; } = null!;

    // Navigation properties
    public SearchIndex Index { get; set; } = null!;
}