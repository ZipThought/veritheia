using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Veritheia.Core.ValueObjects;
using Veritheia.Data.Entities;

namespace Veritheia.Data.Services;

/// <summary>
/// Simple background worker service that processes pending executions
/// Follows the specification: basic loop, no external job frameworks
/// </summary>
public class ProcessWorkerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProcessWorkerService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);

    public ProcessWorkerService(
        IServiceProvider serviceProvider,
        ILogger<ProcessWorkerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Process Worker Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingExecutions(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in process worker loop");
            }

            // Simple polling - wait before checking again
            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("Process Worker Service stopped");
    }

    private async Task ProcessPendingExecutions(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<VeritheiaDbContext>();
        var processEngine = scope.ServiceProvider.GetRequiredService<ProcessEngine>();

        // Get pending executions
        var pendingExecutions = await dbContext.ProcessExecutions
            .Where(e => e.State == "Pending")
            .OrderBy(e => e.CreatedAt)
            .Take(10) // Process up to 10 at a time
            .ToListAsync(cancellationToken);

        foreach (var execution in pendingExecutions)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                _logger.LogInformation("Processing execution {ExecutionId} of type {ProcessType}",
                    execution.Id, execution.ProcessType);

                // Mark as running
                execution.State = "Running";
                execution.StartedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);

                // Execute the process using existing ProcessEngine logic
                await ExecuteProcessInternal(execution, processEngine, cancellationToken);

                _logger.LogInformation("Completed execution {ExecutionId}", execution.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process execution {ExecutionId}", execution.Id);

                // Mark as failed
                execution.State = "Failed";
                execution.CompletedAt = DateTime.UtcNow;
                execution.ErrorMessage = ex.Message;
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private async Task ExecuteProcessInternal(
        ProcessExecution execution,
        ProcessEngine processEngine,
        CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<VeritheiaDbContext>();

        // Get journey with full context
        var journey = await dbContext.Journeys
            .Include(j => j.User)
            .Include(j => j.Persona)
            .FirstOrDefaultAsync(j => j.UserId == execution.UserId && j.Id == execution.JourneyId, cancellationToken);

        if (journey == null)
        {
            throw new InvalidOperationException($"Journey {execution.JourneyId} not found for user {execution.UserId}");
        }

        // Create process instance
        var processType = processEngine.GetRegisteredProcesses().FirstOrDefault(p => p.ProcessId == execution.ProcessType);
        if (processType == null)
        {
            throw new InvalidOperationException($"Process {execution.ProcessType} not registered");
        }

        var process = processEngine.CreateProcessInstance(execution.ProcessType);
        if (process == null)
        {
            throw new InvalidOperationException($"Failed to create process instance for {execution.ProcessType}");
        }

        // Build process context
        var context = new ProcessContext
        {
            ExecutionId = execution.Id,
            UserId = execution.UserId,
            JourneyId = execution.JourneyId,
            Inputs = execution.Inputs,
            Services = _serviceProvider
        };

        // Validate inputs
        if (!process.ValidateInputs(context))
        {
            throw new InvalidOperationException("Process input validation failed");
        }

        // Execute process
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
            UserId = execution.UserId,
            ExecutionId = execution.Id,
            ProcessType = execution.ProcessType,
            Data = result.Data ?? new Dictionary<string, object>(),
            CreatedAt = DateTime.UtcNow
        };

        dbContext.ProcessResults.Add(processResult);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
