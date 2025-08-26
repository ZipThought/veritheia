using Veritheia.Data.Interfaces;

namespace Veritheia.Data.Entities;

/// <summary>
/// Represents user engagement with processes - creates projection spaces
/// </summary>
public class Journey : BaseEntity, IUserOwned
{
    // Partition key - required for composite primary key (UserId, Id)
    public Guid UserId { get; set; }

    public Guid PersonaId { get; set; }
    public string ProcessType { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string State { get; set; } = "Active"; // Maps to JourneyState enum
    public Dictionary<string, object> Context { get; set; } = new();

    // Navigation properties
    public User User { get; set; } = null!;
    public Persona Persona { get; set; } = null!;
    public JourneyFramework? Framework { get; set; }
    public ICollection<Journal> Journals { get; set; } = new List<Journal>();
    public ICollection<JourneyDocumentSegment> DocumentSegments { get; set; } = new List<JourneyDocumentSegment>();
    public ICollection<JourneyFormation> Formations { get; set; } = new List<JourneyFormation>();
    public ICollection<ProcessExecution> ProcessExecutions { get; set; } = new List<ProcessExecution>();
}