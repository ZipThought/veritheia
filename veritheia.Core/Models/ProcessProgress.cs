namespace Veritheia.Core.Models;

/// <summary>
/// Progress information for analytical process execution
/// </summary>
public class ProcessProgress
{
    /// <summary>
    /// Current document being processed
    /// </summary>
    public string CurrentDocument { get; set; } = string.Empty;
    
    /// <summary>
    /// Index of current document (0-based)
    /// </summary>
    public int CurrentIndex { get; set; }
    
    /// <summary>
    /// Total number of documents to process
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Number of documents successfully processed
    /// </summary>
    public int ProcessedCount { get; set; }
    
    /// <summary>
    /// Number of documents that failed processing
    /// </summary>
    public int FailedCount { get; set; }
    
    /// <summary>
    /// Number of must-read documents found so far
    /// </summary>
    public int MustReadCount { get; set; }
    
    /// <summary>
    /// Progress percentage (0-100)
    /// </summary>
    public int PercentComplete => TotalCount > 0 ? (int)((ProcessedCount + FailedCount) * 100.0 / TotalCount) : 0;
    
    /// <summary>
    /// Start time of processing
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// Estimated time remaining
    /// </summary>
    public TimeSpan EstimatedTimeRemaining 
    { 
        get
        {
            if (ProcessedCount == 0) return TimeSpan.Zero;
            var elapsed = DateTime.UtcNow - StartTime;
            var averageTimePerDoc = elapsed.TotalSeconds / ProcessedCount;
            var remainingDocs = TotalCount - ProcessedCount - FailedCount;
            return TimeSpan.FromSeconds(averageTimePerDoc * remainingDocs);
        }
    }
    
    /// <summary>
    /// Elapsed time since start
    /// </summary>
    public TimeSpan ElapsedTime => DateTime.UtcNow - StartTime;
    
    /// <summary>
    /// Optional status message
    /// </summary>
    public string? StatusMessage { get; set; }
    
    /// <summary>
    /// Last save time for intermediate results
    /// </summary>
    public DateTime? LastSaveTime { get; set; }
}