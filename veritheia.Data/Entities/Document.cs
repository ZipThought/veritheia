using Veritheia.Data.Interfaces;

namespace Veritheia.Data.Entities;

/// <summary>
/// Canonical document entry in user's research library (corpus)
/// Maps from various sources (CSV, manual entry, PDF upload) to consistent schema
/// </summary>
public class Document : BaseEntity, IUserOwned
{
    // Partition key - required for composite primary key (UserId, Id)
    public Guid UserId { get; set; }
    
    // Core bibliographic data (canonical library schema)
    public string Title { get; set; } = string.Empty;
    public string? Abstract { get; set; }
    public string? Authors { get; set; }  // Comma-separated
    public int? Year { get; set; }
    public string? DOI { get; set; }  // Unique identifier for deduplication
    public string? Venue { get; set; }  // Journal/Conference
    public string? Keywords { get; set; }  // Comma-separated
    public string? Link { get; set; }  // Original source URL if available
    
    // Full text storage (null until PDF uploaded)
    public string? FullTextPath { get; set; }
    public string? FullTextMimeType { get; set; }
    public long? FullTextSize { get; set; }
    
    // Metadata
    public DateTime AddedToCorpus { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = "manual";  // csv_import, manual, pdf_upload
    
    // Organization
    public Guid? ScopeId { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public KnowledgeScope? Scope { get; set; }
    public DocumentMetadata? Metadata { get; set; }
    public ICollection<JourneyDocumentSegment> JourneySegments { get; set; } = new List<JourneyDocumentSegment>();
}