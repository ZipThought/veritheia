namespace Veritheia.Core.Enums;

/// <summary>
/// How processes are initiated
/// </summary>
public enum ProcessTriggerType
{
    /// <summary>
    /// User-initiated execution
    /// </summary>
    Manual,

    /// <summary>
    /// Event-triggered execution
    /// </summary>
    Automatic,

    /// <summary>
    /// Time-based execution
    /// </summary>
    Scheduled
}