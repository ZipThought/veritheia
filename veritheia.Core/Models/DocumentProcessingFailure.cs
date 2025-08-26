using System;

namespace Veritheia.Core.Models;

/// <summary>
/// Represents a failure that occurred during document processing.
/// Maintains complete transparency about what failed, when, and why
/// to preserve formation integrity and enable user review.
/// </summary>
public class DocumentProcessingFailure
{
    /// <summary>
    /// Unique identifier for this failure instance
    /// </summary>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Index of the document that failed (0-based)
    /// </summary>
    public int DocumentIndex { get; set; }

    /// <summary>
    /// Title of the document that failed, for user identification
    /// </summary>
    public string DocumentTitle { get; set; } = string.Empty;

    /// <summary>
    /// DOI or other identifier of the document that failed
    /// </summary>
    public string? DocumentIdentifier { get; set; }

    /// <summary>
    /// The specific processing stage that failed
    /// (e.g., "Semantic Extraction", "Relevance Assessment", "Contribution Assessment")
    /// </summary>
    public string ProcessingStage { get; set; } = string.Empty;

    /// <summary>
    /// The research question index that was being processed when failure occurred (if applicable)
    /// </summary>
    public int? ResearchQuestionIndex { get; set; }

    /// <summary>
    /// The exception type that caused the failure
    /// </summary>
    public string ExceptionType { get; set; } = string.Empty;

    /// <summary>
    /// The exception message describing what went wrong
    /// </summary>
    public string ExceptionMessage { get; set; } = string.Empty;

    /// <summary>
    /// Full stack trace for debugging purposes
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Inner exception details if present
    /// </summary>
    public string? InnerExceptionDetails { get; set; }

    /// <summary>
    /// When the failure occurred
    /// </summary>
    public DateTime FailedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Impact on the formation process
    /// </summary>
    public string FormationImpact { get; set; } = string.Empty;

    /// <summary>
    /// Whether processing was able to continue after this failure
    /// </summary>
    public bool ProcessingContinued { get; set; } = true;

    /// <summary>
    /// Additional context data that might help with debugging or user understanding
    /// </summary>
    public Dictionary<string, object> Context { get; set; } = new();
}

/// <summary>
/// Summary of all failures that occurred during a processing batch.
/// Provides transparent reporting to maintain formation integrity.
/// </summary>
public class ProcessingFailureSummary
{
    /// <summary>
    /// Total number of documents that were processed successfully
    /// </summary>
    public int SuccessfulCount { get; set; }

    /// <summary>
    /// Total number of documents that failed processing
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// Detailed list of all failures that occurred
    /// </summary>
    public List<DocumentProcessingFailure> Failures { get; set; } = new();

    /// <summary>
    /// Summary of failure types and their frequencies
    /// </summary>
    public Dictionary<string, int> FailureTypeCounts { get; set; } = new();

    /// <summary>
    /// Whether the entire processing batch completed despite individual failures
    /// </summary>
    public bool BatchCompleted { get; set; }

    /// <summary>
    /// When the processing batch started
    /// </summary>
    public DateTime ProcessingStarted { get; set; }

    /// <summary>
    /// When the processing batch finished (success or failure)
    /// </summary>
    public DateTime? ProcessingFinished { get; set; }

    /// <summary>
    /// Total processing duration
    /// </summary>
    public TimeSpan? ProcessingDuration => ProcessingFinished?.Subtract(ProcessingStarted);

    /// <summary>
    /// Generate a transparent summary message for user display
    /// </summary>
    public string GenerateSummaryMessage()
    {
        if (FailedCount == 0)
        {
            return $"Processing completed successfully: {SuccessfulCount} documents processed, no failures.";
        }

        var message = $"Processing completed with transparency: {SuccessfulCount} documents processed successfully, {FailedCount} failed.";

        if (FailureTypeCounts.Any())
        {
            message += " Failure breakdown: " + string.Join(", ",
                FailureTypeCounts.Select(kvp => $"{kvp.Value} {kvp.Key} failures"));
        }

        return message;
    }
}