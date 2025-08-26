namespace Veritheia.Core.Enums;

/// <summary>
/// Execution state of a process
/// </summary>
public enum ProcessState
{
    /// <summary>
    /// Not yet started
    /// </summary>
    Pending,
    
    /// <summary>
    /// Currently executing
    /// </summary>
    Running,
    
    /// <summary>
    /// Finished successfully
    /// </summary>
    Completed,
    
    /// <summary>
    /// Terminated with error
    /// </summary>
    Failed,
    
    /// <summary>
    /// User cancelled execution
    /// </summary>
    Cancelled
}