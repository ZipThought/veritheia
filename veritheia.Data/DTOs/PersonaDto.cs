namespace Veritheia.Data.DTOs;

/// <summary>
/// Data Transfer Object for Persona entity
/// Used for API communication between services
/// </summary>
public class PersonaDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Domain { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Dictionary<string, object> ConceptualVocabulary { get; set; } = new();
    public List<object> Patterns { get; set; } = new();
    public List<object> MethodologicalPreferences { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request model for creating personas
/// </summary>
public class CreatePersonaRequest
{
    public Guid? UserId { get; set; }
    public string Domain { get; set; } = string.Empty;
    public Dictionary<string, object>? ConceptualVocabulary { get; set; }
}
