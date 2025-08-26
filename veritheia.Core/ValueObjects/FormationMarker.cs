namespace Veritheia.Core.ValueObjects;

/// <summary>
/// Marks a moment of intellectual formation - when understanding crystallizes
/// </summary>
public class FormationMarker
{
    /// <summary>
    /// When the formation occurred
    /// </summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>
    /// Description of the insight or understanding
    /// </summary>
    public string InsightDescription { get; set; } = string.Empty;

    /// <summary>
    /// Journey where this formation happened
    /// </summary>
    public Guid JourneyId { get; set; }

    /// <summary>
    /// Context that led to this formation
    /// </summary>
    public string Context { get; set; } = string.Empty;

    /// <summary>
    /// Segments that contributed to this insight
    /// </summary>
    public List<Guid> ContributingSegmentIds { get; set; } = new();
}