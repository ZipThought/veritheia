namespace Veritheia.Core.ValueObjects;

/// <summary>
/// Execution context passed through a process
/// </summary>
public class ProcessContext
{
    /// <summary>
    /// Unique identifier for this execution
    /// </summary>
    public Guid ExecutionId { get; set; }

    /// <summary>
    /// User executing the process
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Journey context for this execution
    /// </summary>
    public Guid JourneyId { get; set; }

    /// <summary>
    /// Optional knowledge scope constraint
    /// </summary>
    public Guid? ScopeId { get; set; }

    /// <summary>
    /// Process-specific input parameters
    /// </summary>
    public Dictionary<string, object> Inputs { get; set; } = new();

    /// <summary>
    /// Journey context with recent entries and persona
    /// </summary>
    public JourneyContext? JourneyContext { get; set; }

    /// <summary>
    /// Service provider for dependency injection
    /// </summary>
    public IServiceProvider? Services { get; set; }

    /// <summary>
    /// Progress reporter for real-time updates
    /// </summary>
    public IProgress<Models.ProcessProgress>? ProgressReporter { get; set; }

    /// <summary>
    /// Get typed input value
    /// </summary>
    public T? GetInput<T>(string key)
    {
        if (Inputs.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default;
    }

    /// <summary>
    /// Get service from DI container
    /// </summary>
    public T? GetService<T>() where T : class
    {
        return Services?.GetService(typeof(T)) as T;
    }
}