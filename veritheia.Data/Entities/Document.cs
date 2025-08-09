namespace Veritheia.Data.Entities;

/// <summary>
/// Source materials in the knowledge base - raw corpus
/// </summary>
public class Document : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public Guid? ScopeId { get; set; }
    
    // Navigation properties
    public KnowledgeScope? Scope { get; set; }
    public DocumentMetadata? Metadata { get; set; }
    public ICollection<JourneyDocumentSegment> JourneySegments { get; set; } = new List<JourneyDocumentSegment>();
}