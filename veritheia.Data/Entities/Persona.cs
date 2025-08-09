namespace Veritheia.Data.Entities;

/// <summary>
/// Evolving representation of user's intellectual style
/// </summary>
public class Persona : BaseEntity
{
    public Guid UserId { get; set; }
    public string Domain { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    // JSONB fields for flexible schema
    public Dictionary<string, object> ConceptualVocabulary { get; set; } = new();
    public List<object> Patterns { get; set; } = new();
    public List<object> MethodologicalPreferences { get; set; } = new();
    public List<object> Markers { get; set; } = new();
    
    public DateTime? LastEvolved { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Journey> Journeys { get; set; } = new List<Journey>();
}