namespace Veritheia.Data.Entities;

/// <summary>
/// Narrative records within journeys
/// </summary>
public class Journal : BaseEntity
{
    public Guid JourneyId { get; set; }
    public string Type { get; set; } = string.Empty; // Research, Method, Decision, Reflection
    public bool IsShareable { get; set; } = false;
    
    // Navigation properties
    public Journey Journey { get; set; } = null!;
    public ICollection<JournalEntry> Entries { get; set; } = new List<JournalEntry>();
}