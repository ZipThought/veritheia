namespace Veritheia.Core.ValueObjects;

/// <summary>
/// Runtime context assembled from journey, journals, and persona
/// </summary>
public class JourneyContext
{
    /// <summary>
    /// The journey's stated purpose
    /// </summary>
    public string Purpose { get; set; } = string.Empty;

    /// <summary>
    /// Current journey state/progress
    /// </summary>
    public Dictionary<string, object> State { get; set; } = new();

    /// <summary>
    /// Recent journal entries for context
    /// </summary>
    public List<JournalEntryContext> RecentEntries { get; set; } = new();

    /// <summary>
    /// User's persona context for this journey
    /// </summary>
    public PersonaContext? PersonaContext { get; set; }

    /// <summary>
    /// Research questions if defined
    /// </summary>
    public List<string> ResearchQuestions { get; set; } = new();

    /// <summary>
    /// Conceptual vocabulary for this journey
    /// </summary>
    public Dictionary<string, string> ConceptualVocabulary { get; set; } = new();
}

/// <summary>
/// Simplified journal entry for context
/// </summary>
public class JournalEntryContext
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Significance { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<string> Tags { get; set; } = new();
}