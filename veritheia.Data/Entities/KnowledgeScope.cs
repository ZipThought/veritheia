using Veritheia.Data.Interfaces;

namespace Veritheia.Data.Entities;

/// <summary>
/// Organizational structure for documents
/// </summary>
public class KnowledgeScope : BaseEntity, IUserOwned
{
    // Partition key - required for composite primary key (UserId, Id)
    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Project, Topic, Subject, Custom
    public Guid? ParentScopeId { get; set; }

    // Navigation properties
    public KnowledgeScope? ParentScope { get; set; }
    public ICollection<KnowledgeScope> ChildScopes { get; set; } = new List<KnowledgeScope>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}