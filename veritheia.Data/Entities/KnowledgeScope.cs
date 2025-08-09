namespace Veritheia.Data.Entities;

/// <summary>
/// Organizational boundaries for documents
/// </summary>
public class KnowledgeScope : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty; // Project, Topic, Subject, Custom
    public Guid? ParentScopeId { get; set; }
    
    // Navigation properties
    public KnowledgeScope? ParentScope { get; set; }
    public ICollection<KnowledgeScope> ChildScopes { get; set; } = new List<KnowledgeScope>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}