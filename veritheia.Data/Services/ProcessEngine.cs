using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Veritheia.Core.Interfaces;
using Veritheia.Core.ValueObjects;
using Veritheia.Data;
using Veritheia.Data.Entities;

namespace Veritheia.Data.Services;

/// <summary>
/// Process Engine - Orchestrates analytical workflow execution
/// Operates through journey projection spaces
/// </summary>
public class ProcessEngine
{
    private readonly VeritheiaDbContext _db;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProcessEngine> _logger;
    private readonly Dictionary<string, Type> _registeredProcesses;

    public ProcessEngine(
        VeritheiaDbContext dbContext,
        IServiceProvider serviceProvider,
        ILogger<ProcessEngine> logger)
    {
        _db = dbContext;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _registeredProcesses = new Dictionary<string, Type>();
    }

    /// <summary>
    /// Register a process type for execution
    /// </summary>
    public void RegisterProcess<TProcess>() where TProcess : IAnalyticalProcess
    {
        var processType = typeof(TProcess);
        var instance = ActivatorUtilities.CreateInstance<TProcess>(_serviceProvider);
        _registeredProcesses[instance.ProcessId] = processType;

        _logger.LogInformation("Registered process: {ProcessId} ({Name})",
            instance.ProcessId, instance.Name);
    }

    /// <summary>
    /// Get all registered processes
    /// </summary>
    public List<ProcessInfo> GetAvailableProcesses()
    {
        var processes = new List<ProcessInfo>();

        foreach (var kvp in _registeredProcesses)
        {
            var instance = CreateProcessInstance(kvp.Key);
            if (instance != null)
            {
                processes.Add(new ProcessInfo
                {
                    ProcessId = instance.ProcessId,
                    Name = instance.Name,
                    Description = instance.Description,
                    Category = instance.Category,
                    InputDefinition = instance.GetInputDefinition()
                });
            }
        }

        return processes;
    }

    /// <summary>
    /// Queue a process for background execution
    /// </summary>
    public async Task<Guid> QueueProcessAsync(
        string processId,
        Guid userId,
        Guid journeyId,
        Dictionary<string, object> inputs,
        CancellationToken cancellationToken = default)
    {
        // Validate process exists
        var process = CreateProcessInstance(processId);
        if (process == null)
            throw new InvalidOperationException($"Process {processId} not registered");

        // Create execution record in Pending state
        var execution = new ProcessExecution
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            JourneyId = journeyId,
            ProcessType = processId,
            State = "Pending",
            Inputs = inputs,
            CreatedAt = DateTime.UtcNow
        };

        _db.ProcessExecutions.Add(execution);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Queued process {ProcessId} for journey {JourneyId} with execution {ExecutionId}",
            processId, journeyId, execution.Id);

        return execution.Id;
    }

    /// <summary>
    /// Execute a process synchronously (for immediate execution)
    /// </summary>
    public async Task<ProcessExecutionResult> ExecuteProcessAsync(
        string processId,
        Guid journeyId,
        Dictionary<string, object> inputs,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteProcessAsync(processId, journeyId, inputs, null, cancellationToken);
    }

    /// <summary>
    /// Execute a process synchronously with progress reporting
    /// </summary>
    public async Task<ProcessExecutionResult> ExecuteProcessAsync(
        string processId,
        Guid journeyId,
        Dictionary<string, object> inputs,
        IProgress<Veritheia.Core.Models.ProcessProgress>? progressReporter,
        CancellationToken cancellationToken = default)
    {
        // Get journey with full context
        var journey = await _db.Journeys
            .Include(j => j.User)
            .Include(j => j.Persona)
            .FirstOrDefaultAsync(j => j.Id == journeyId, cancellationToken);

        if (journey == null)
            throw new InvalidOperationException($"Journey {journeyId} not found");

        // Create process instance
        var process = CreateProcessInstance(processId);
        if (process == null)
            throw new InvalidOperationException($"Process {processId} not registered");

        // Create execution record
        var execution = new ProcessExecution
        {
            Id = Guid.CreateVersion7(),
            UserId = journey.UserId,
            JourneyId = journeyId,
            ProcessType = processId,
            StartedAt = DateTime.UtcNow,
            State = "Running",
            Inputs = inputs,
            CreatedAt = DateTime.UtcNow
        };

        _db.ProcessExecutions.Add(execution);
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            // Build process context
            var context = new ProcessContext
            {
                ExecutionId = execution.Id,
                UserId = journey.UserId,
                JourneyId = journeyId,
                Inputs = inputs,
                Services = _serviceProvider,
                ProgressReporter = progressReporter
            };

            // Validate inputs
            if (!process.ValidateInputs(context))
            {
                throw new InvalidOperationException("Process input validation failed");
            }

            // Execute process
            _logger.LogInformation("Executing process {ProcessId} for journey {JourneyId}",
                processId, journeyId);

            var result = await process.ExecuteAsync(context, cancellationToken);

            // Update execution record
            execution.CompletedAt = DateTime.UtcNow;
            execution.State = result.Success ? "Completed" : "Failed";
            if (!result.Success)
            {
                execution.ErrorMessage = result.ErrorMessage;
            }

            // Store result
            var processResult = new ProcessResult
            {
                Id = Guid.CreateVersion7(),
                UserId = journey.UserId,
                ExecutionId = execution.Id,
                ProcessType = processId,
                Data = result.Data ?? new Dictionary<string, object>(),
                CreatedAt = DateTime.UtcNow
            };

            _db.ProcessResults.Add(processResult);
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Process {ProcessId} completed with status: {Status}",
                processId, execution.State);

            return new ProcessExecutionResult
            {
                ExecutionId = execution.Id,
                Success = result.Success,
                Data = result.Data ?? new Dictionary<string, object>(),
                ErrorMessage = result.ErrorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Process {ProcessId} failed for journey {JourneyId}",
                processId, journeyId);

            // Update execution record
            execution.CompletedAt = DateTime.UtcNow;
            execution.State = "Failed";
            execution.ErrorMessage = ex.Message;
            await _db.SaveChangesAsync(cancellationToken);

            throw;
        }
    }

    /// <summary>
    /// Get execution history for a journey
    /// </summary>
    public async Task<List<ProcessExecution>> GetJourneyExecutionsAsync(Guid journeyId)
    {
        return await _db.ProcessExecutions
            .Where(e => e.JourneyId == journeyId)
            .Include(e => e.Result)
            .OrderByDescending(e => e.StartedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Create a process instance (exposed for ProcessWorkerService)
    /// </summary>
    public IAnalyticalProcess? CreateProcessInstance(string processId)
    {
        if (!_registeredProcesses.TryGetValue(processId, out var processType))
            return null;

        return (IAnalyticalProcess)ActivatorUtilities.CreateInstance(_serviceProvider, processType);
    }

    /// <summary>
    /// Get registered processes (exposed for ProcessWorkerService)
    /// </summary>
    public List<ProcessInfo> GetRegisteredProcesses()
    {
        return GetAvailableProcesses();
    }
}

/// <summary>
/// Process information for discovery
/// </summary>
public class ProcessInfo
{
    public string ProcessId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public InputDefinition InputDefinition { get; set; } = new();
}

/// <summary>
/// Result of process execution
/// </summary>
public class ProcessExecutionResult
{
    public Guid ExecutionId { get; set; }
    public bool Success { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public string? ErrorMessage { get; set; }
}