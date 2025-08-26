namespace Veritheia.Core.Enums;

/// <summary>
/// Importance level for journal entries
/// </summary>
public enum EntrySignificance
{
    /// <summary>
    /// Regular progress entry
    /// </summary>
    Routine,

    /// <summary>
    /// Worth highlighting for future reference
    /// </summary>
    Notable,

    /// <summary>
    /// Key decision or insight
    /// </summary>
    Critical,

    /// <summary>
    /// Major achievement or breakthrough
    /// </summary>
    Milestone
}