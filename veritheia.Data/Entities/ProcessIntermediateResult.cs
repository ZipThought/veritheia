using System;

namespace Veritheia.Data.Entities;

/// <summary>
/// Stores intermediate results during long-running process execution
/// Enables recovery and progress tracking for formation journeys
/// </summary>
public class ProcessIntermediateResult
{
    public Guid Id { get; set; }
    public Guid ExecutionId { get; set; }
    public Guid JourneyId { get; set; }
    public DateTime SavedAt { get; set; }
    public int ProcessedCount { get; set; }
    public int FailedCount { get; set; }
    public int MustReadCount { get; set; }
    
    /// <summary>
    /// JSON serialized projection data for the last batch
    /// </summary>
    public string ProjectionData { get; set; } = "";
    
    // Navigation properties
    public ProcessExecution? ProcessExecution { get; set; }
    public Journey? Journey { get; set; }
}