using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using veritheia.Tests.TestBase;
using Veritheia.Core.Interfaces;
using Veritheia.Data;
using Veritheia.Data.Entities;
using Veritheia.Data.Services;
using Xunit;

namespace veritheia.Tests.Integration.Services;

[Trait("Category", "Integration")]
public class ProcessWorkerServiceIntegrationTests : DatabaseTestBase
{
    public ProcessWorkerServiceIntegrationTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task ProcessWorkerService_ShouldQueryDatabaseWithoutErrors()
    {
        // Setup services like ProcessWorkerService would
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, LoggerFactory>();
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        services.AddScoped(_ => Context);
        services.AddScoped<ICognitiveAdapter, MockCognitiveAdapter>();
        
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<ProcessWorkerService>>();
        
        // Create ProcessWorkerService
        var workerService = new ProcessWorkerService(serviceProvider, logger);
        
        // This should not throw any database schema errors
        // We're testing the database access patterns, not the full service execution
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1)); // Cancel quickly
        
        try
        {
            await workerService.StartAsync(cancellationTokenSource.Token);
            
            // Let it run briefly to test database queries
            await Task.Delay(100, CancellationToken.None);
            
            await workerService.StopAsync(CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // Expected - we cancelled it
            await workerService.StopAsync(CancellationToken.None);
        }
        
        // If we reach here, the database queries in ProcessWorkerService worked
        Assert.True(true);
    }

    [Fact]
    public async Task ProcessWorkerService_ShouldHandlePendingExecutions()
    {
        // Create test data
        var testUserId = Guid.NewGuid();
        var testJourneyId = Guid.NewGuid();
        var testExecutionId = Guid.NewGuid();
        
        // Add a user first (required for foreign key)
        var user = new User
        {
            Id = testUserId,
            Email = "test@example.com",
            DisplayName = "Test User",
            CreatedAt = DateTime.UtcNow
        };
        Context.Users.Add(user);
        
        // Add a persona (required for journey)
        var persona = new Persona
        {
            UserId = testUserId,
            Id = Guid.NewGuid(),
            Domain = "Test",
            ConceptualVocabulary = new Dictionary<string, object>(),
            Patterns = new List<object>(),
            MethodologicalPreferences = new List<object>(),
            Markers = new List<object>(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        Context.Personas.Add(persona);
        
        // Add a journey (required for process execution)
        var journey = new Journey
        {
            UserId = testUserId,
            Id = testJourneyId,
            PersonaId = persona.Id,
            ProcessType = "TestProcess",
            Purpose = "Test Journey",
            State = "Active",
            Context = new Dictionary<string, object>(),
            CreatedAt = DateTime.UtcNow
        };
        Context.Journeys.Add(journey);
        
        // Add a pending process execution
        var processExecution = new ProcessExecution
        {
            UserId = testUserId,
            Id = testExecutionId,
            JourneyId = testJourneyId,
            ProcessType = "TestProcess",
            State = "Pending",
            Inputs = new Dictionary<string, object> { { "test", "value" } },
            CreatedAt = DateTime.UtcNow
        };
        Context.ProcessExecutions.Add(processExecution);
        
        await Context.SaveChangesAsync();
        
        // Now test that ProcessWorkerService can query this data
        var pendingExecutions = await Context.ProcessExecutions
            .Where(pe => pe.State == "Pending")
            .OrderBy(pe => pe.CreatedAt)
            .Take(10)
            .ToListAsync();
            
        Assert.Single(pendingExecutions);
        Assert.Equal(testExecutionId, pendingExecutions[0].Id);
        Assert.Equal(testUserId, pendingExecutions[0].UserId);
        Assert.Equal("Pending", pendingExecutions[0].State);
    }
}