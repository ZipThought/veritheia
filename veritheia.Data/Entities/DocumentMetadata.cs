using Veritheia.Data.Interfaces;

namespace Veritheia.Data.Entities;

/// <summary>
/// Extracted metadata from documents
/// </summary>
public class DocumentMetadata : BaseEntity, IUserOwned
{
    // Partition key - required for composite primary key (UserId, Id)
    public Guid UserId { get; set; }
    
    public Guid DocumentId { get; set; }
    public string? Title { get; set; }
    public string[]? Authors { get; set; }
    public string? Abstract { get; set; }
    public string[]? Keywords { get; set; }
    public DateTime? PublicationDate { get; set; }
    public string? Publisher { get; set; }
    public string? DOI { get; set; }
    public Dictionary<string, object>? ExtendedMetadata { get; set; }
    
    // Navigation properties
    public Document Document { get; set; } = null!;
}