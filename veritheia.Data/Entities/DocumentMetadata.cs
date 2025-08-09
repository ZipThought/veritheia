namespace Veritheia.Data.Entities;

/// <summary>
/// Extracted document properties
/// </summary>
public class DocumentMetadata : BaseEntity
{
    public Guid DocumentId { get; set; }
    public string? Title { get; set; }
    public List<string> Authors { get; set; } = new();
    public DateTime? PublicationDate { get; set; }
    public Dictionary<string, object> ExtendedMetadata { get; set; } = new();
    
    // Navigation properties
    public Document Document { get; set; } = null!;
}