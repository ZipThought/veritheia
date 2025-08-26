namespace Veritheia.Core.ValueObjects;

/// <summary>
/// User's intellectual patterns and vocabulary for a journey
/// </summary>
public class PersonaContext
{
    /// <summary>
    /// Terms relevant to current context
    /// </summary>
    public List<string> RelevantVocabulary { get; set; } = new();
    
    /// <summary>
    /// Active inquiry patterns
    /// </summary>
    public List<InquiryPattern> ActivePatterns { get; set; } = new();
    
    /// <summary>
    /// Current domain focus if any
    /// </summary>
    public string? DomainFocus { get; set; }
    
    /// <summary>
    /// Methodological preferences
    /// </summary>
    public List<string> MethodologicalPreferences { get; set; } = new();
}

/// <summary>
/// Pattern of inquiry observed in user's work
/// </summary>
public class InquiryPattern
{
    /// <summary>
    /// Type of pattern (e.g., "comparative", "causal", "exploratory")
    /// </summary>
    public string PatternType { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the pattern
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// How often this pattern appears
    /// </summary>
    public int OccurrenceCount { get; set; }
    
    /// <summary>
    /// When last observed
    /// </summary>
    public DateTime LastObserved { get; set; }
}