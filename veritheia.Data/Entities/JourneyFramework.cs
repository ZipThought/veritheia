namespace Veritheia.Data.Entities;

/// <summary>
/// Defines how each journey projects documents into its intellectual space
/// </summary>
public class JourneyFramework : BaseEntity
{
    public Guid JourneyId { get; set; }
    public string JourneyType { get; set; } = string.Empty; // systematic_review, educational, research_formation
    
    /// <summary>
    /// The intellectual framework that shapes projections
    /// Contains research_questions, conceptual_vocabulary, assessment_criteria, theoretical_orientation
    /// </summary>
    public Dictionary<string, object> FrameworkElements { get; set; } = new();
    
    /// <summary>
    /// Rules for transforming documents in this journey's space
    /// Contains segmentation, embedding_context, assessment_prompts, discovery_parameters
    /// </summary>
    public Dictionary<string, object> ProjectionRules { get; set; } = new();
    
    // Navigation properties
    public Journey Journey { get; set; } = null!;
}