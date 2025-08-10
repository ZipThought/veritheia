namespace Veritheia.Data.Entities;

/// <summary>
/// Source materials in the knowledge base - raw corpus
/// </summary>
public class Document : BaseEntity
{
    // Ownership tracking - REQUIRED even for single-user MVP
    // Legal/copyright clarity, future sharing attribution, audit trail
    public Guid UserId { get; set; }
    
    // File properties
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    // Organization
    public Guid? ScopeId { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public KnowledgeScope? Scope { get; set; }
    public DocumentMetadata? Metadata { get; set; }
    public ICollection<JourneyDocumentSegment> JourneySegments { get; set; } = new List<JourneyDocumentSegment>();
}