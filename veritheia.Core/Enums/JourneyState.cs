namespace Veritheia.Core.Enums;

/// <summary>
/// Current state of a user's journey through a process
/// </summary>
public enum JourneyState
{
    /// <summary>
    /// Journey is actively being worked on
    /// </summary>
    Active,
    
    /// <summary>
    /// Journey temporarily suspended
    /// </summary>
    Paused,
    
    /// <summary>
    /// Journey finished successfully
    /// </summary>
    Completed,
    
    /// <summary>
    /// Journey discontinued by user
    /// </summary>
    Abandoned
}