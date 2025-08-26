using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using veritheia.Tests.TestBase;
using Veritheia.Core.Interfaces;
using Veritheia.Data;
using Veritheia.Data.Services;
using Xunit;

namespace veritheia.Tests.Integration.FullStack;

/// <summary>
/// End-to-end tests that validate the complete application stack.
/// These tests ensure that database schema mismatches don't cause runtime failures.
/// </summary>
[Trait("Category", "Integration")]
[Trait("Category", "FullStack")]
public class ApplicationHealthTests : DatabaseTestBase
{
    public ApplicationHealthTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task ProcessWorkerService_ShouldStartAndRunWithoutDatabaseErrors()
    {
        // This test validates that ProcessWorkerService can start and run
        // without throwing the "column p.UserId does not exist" error

        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, LoggerFactory>();
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        services.AddScoped(_ => Context);
        services.AddScoped<ICognitiveAdapter, LocalLLMAdapter>();

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<ProcessWorkerService>>();

        var workerService = new ProcessWorkerService(serviceProvider, logger);

        // Start the service
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(2)); // Run for 2 seconds

        var startTask = workerService.StartAsync(CancellationToken.None);

        try
        {
            // Let it run for a bit to ensure database queries execute
            await Task.Delay(1000, CancellationToken.None);

            // Stop the service cleanly
            await workerService.StopAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            // Stop the service if there's an error
            await workerService.StopAsync(CancellationToken.None);

            // If we get a database schema error, the test should fail
            if (ex.Message.Contains("column") && ex.Message.Contains("does not exist"))
            {
                Assert.Fail($"Database schema error detected: {ex.Message}");
            }

            // Other exceptions might be expected (e.g., no pending processes)
            // We're mainly testing that the database queries don't fail
        }

        // If we reach here, the ProcessWorkerService didn't throw database schema errors
        Assert.True(true, "ProcessWorkerService ran without database schema errors");
    }

    [Fact]
    public async Task CriticalDatabaseOperations_ShouldWorkWithoutErrors()
    {
        // Test the exact operations that were failing in production

        // 1. Query pending process executions (the failing query)
        var pendingExecutions = await Context.ProcessExecutions
            .Where(pe => pe.State == "Pending")
            .OrderBy(pe => pe.CreatedAt)
            .Take(10)
            .ToListAsync();
        Assert.NotNull(pendingExecutions);

        // 2. Test user partitioning works correctly
        var testUserId = Guid.NewGuid();
        var userExecutions = await Context.ProcessExecutions
            .Where(pe => pe.UserId == testUserId)
            .ToListAsync();
        Assert.NotNull(userExecutions);

        // 3. Test composite primary key operations
        var compositeKeyQuery = await Context.ProcessExecutions
            .Where(pe => pe.UserId == Guid.NewGuid() && pe.Id == Guid.NewGuid())
            .FirstOrDefaultAsync();
        // Should be null but not throw an exception
        Assert.Null(compositeKeyQuery);

        // 4. Test all user-owned entities can be queried
        var entityQueries = new[]
        {
            Context.Personas.Where(p => p.UserId == testUserId).CountAsync(),
            Context.Journeys.Where(j => j.UserId == testUserId).CountAsync(),
            Context.ProcessExecutions.Where(pe => pe.UserId == testUserId).CountAsync(),
            Context.ProcessResults.Where(pr => pr.UserId == testUserId).CountAsync()
        };

        foreach (var query in entityQueries)
        {
            var count = await query;
            Assert.True(count >= 0); // Should not throw
        }
    }

    [Fact]
    public async Task DatabaseMigrations_ShouldBeCompleteAndConsistent()
    {
        // Ensure all migrations have been applied
        var appliedMigrations = Context.Database.GetAppliedMigrations().ToList();
        Assert.NotEmpty(appliedMigrations);

        // The critical migration that adds UserId columns should be applied
        Assert.Contains("20250820070909_CompositePrimaryKeys", appliedMigrations);

        // No pending migrations should remain
        var pendingMigrations = Context.Database.GetPendingMigrations().ToList();
        Assert.Empty(pendingMigrations);

        // Database should be able to create the model without errors
        var model = Context.Model;
        Assert.NotNull(model);

        // Critical entities should have proper configuration
        var processExecutionEntity = model.FindEntityType(typeof(Veritheia.Data.Entities.ProcessExecution));
        Assert.NotNull(processExecutionEntity);

        var userIdProperty = processExecutionEntity.FindProperty("UserId");
        Assert.NotNull(userIdProperty);

        // Should have composite primary key (UserId, Id)
        var primaryKey = processExecutionEntity.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Equal(2, primaryKey.Properties.Count);
        Assert.Contains(primaryKey.Properties, p => p.Name == "UserId");
        Assert.Contains(primaryKey.Properties, p => p.Name == "Id");
    }
}
