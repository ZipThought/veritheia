namespace Veritheia.Data.Entities;

/// <summary>
/// User account entity - the constant in the system
/// </summary>
public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime? LastActiveAt { get; set; }
    
    // Navigation properties
    public ICollection<Persona> Personas { get; set; } = new List<Persona>();
    public ICollection<Journey> Journeys { get; set; } = new List<Journey>();
    public ICollection<ProcessCapability> ProcessCapabilities { get; set; } = new List<ProcessCapability>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}