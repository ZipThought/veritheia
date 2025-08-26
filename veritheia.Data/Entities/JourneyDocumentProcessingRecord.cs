using System;
using System.Collections.Generic;
using Veritheia.Data.Interfaces;

namespace Veritheia.Data.Entities;

/// <summary>
/// Records the processing outcome for each document within a journey.
/// Tracks what was attempted, what succeeded, and what could not be processed
/// for the user's formation journey. This is NOT for system errors - those go to logs.
/// This is about the journey's formation integrity and completeness.
/// </summary>
public class JourneyDocumentProcessingRecord : BaseEntity, IUserOwned
{
    /// <summary>
    /// The user who owns this processing record
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the user
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// The journey where this document was processed
    /// </summary>
    public Guid JourneyId { get; set; }

    /// <summary>
    /// Navigation property to the journey
    /// </summary>
    public Journey Journey { get; set; } = null!;

    /// <summary>
    /// The process execution during which this document was processed
    /// </summary>
    public Guid ProcessExecutionId { get; set; }

    /// <summary>
    /// Navigation property to the process execution
    /// </summary>
    public ProcessExecution ProcessExecution { get; set; } = null!;

    /// <summary>
    /// Index of the document in the processing batch (0-based)
    /// </summary>
    public int DocumentIndex { get; set; }

    /// <summary>
    /// Document identifier (DOI, URL, or other unique identifier)
    /// </summary>
    public string? DocumentIdentifier { get; set; }

    /// <summary>
    /// Document title for user-friendly identification
    /// </summary>
    public string DocumentTitle { get; set; } = string.Empty;

    /// <summary>
    /// Authors of the document
    /// </summary>
    public string? Authors { get; set; }

    /// <summary>
    /// Year of publication
    /// </summary>
    public int? Year { get; set; }

    /// <summary>
    /// Venue (journal, conference, etc.)
    /// </summary>
    public string? Venue { get; set; }

    /// <summary>
    /// Whether this document was successfully processed
    /// </summary>
    public bool WasProcessedSuccessfully { get; set; }

    /// <summary>
    /// If processing failed, at what stage did it fail?
    /// (e.g., "Semantic Extraction", "Relevance Assessment for RQ1", etc.)
    /// NULL if processing succeeded
    /// </summary>
    public string? FailureStage { get; set; }

    /// <summary>
    /// If processing failed, why? Brief user-friendly explanation.
    /// (e.g., "Could not extract semantic content from abstract", 
    ///  "Language model unavailable for assessment")
    /// NULL if processing succeeded
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// What this means for the user's formation journey
    /// (e.g., "Document excluded from screening results",
    ///  "Partial assessment available - 3 of 4 research questions evaluated")
    /// </summary>
    public string FormationImpact { get; set; } = string.Empty;

    /// <summary>
    /// Processing results if successful (or partial results if partially successful)
    /// Stored as JSON containing assessment scores, must-read determination, etc.
    /// </summary>
    public Dictionary<string, object>? ProcessingResults { get; set; }

    /// <summary>
    /// Which research questions were successfully assessed (if applicable)
    /// </summary>
    public List<int>? AssessedResearchQuestions { get; set; }

    /// <summary>
    /// Which research questions could not be assessed (if applicable)
    /// </summary>
    public List<int>? UnassessedResearchQuestions { get; set; }

    /// <summary>
    /// Was this document determined to be must-read (if assessment completed)?
    /// NULL if assessment incomplete
    /// </summary>
    public bool? MustRead { get; set; }

    /// <summary>
    /// Topics extracted from the document (if extraction succeeded)
    /// </summary>
    public List<string>? ExtractedTopics { get; set; }

    /// <summary>
    /// Entities extracted from the document (if extraction succeeded)
    /// </summary>
    public List<string>? ExtractedEntities { get; set; }

    /// <summary>
    /// Keywords from the document (combined extracted and original)
    /// </summary>
    public List<string>? Keywords { get; set; }

    /// <summary>
    /// When this document's processing started
    /// </summary>
    public DateTime ProcessingStartedAt { get; set; }

    /// <summary>
    /// When this document's processing completed (success or failure)
    /// </summary>
    public DateTime ProcessingCompletedAt { get; set; }

    /// <summary>
    /// Duration of processing for this specific document
    /// </summary>
    public TimeSpan ProcessingDuration => ProcessingCompletedAt - ProcessingStartedAt;

    /// <summary>
    /// Whether the user has reviewed this processing outcome
    /// </summary>
    public bool IsReviewed { get; set; } = false;

    /// <summary>
    /// When the user reviewed this outcome
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// User's notes about this document's processing
    /// (for their own formation tracking)
    /// </summary>
    public string? UserNotes { get; set; }
}