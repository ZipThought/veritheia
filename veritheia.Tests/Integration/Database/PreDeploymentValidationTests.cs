using Microsoft.EntityFrameworkCore;
using veritheia.Tests.TestBase;
using Veritheia.Data;
using Xunit;

namespace veritheia.Tests.Integration.Database;

/// <summary>
/// Critical tests that must pass before any deployment.
/// These tests validate that the database schema matches the EF model expectations.
/// </summary>
[Trait("Category", "Integration")]
[Trait("Category", "PreDeployment")]
public class PreDeploymentValidationTests : DatabaseTestBase
{
    public PreDeploymentValidationTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task CriticalDatabaseQueries_ShouldExecuteWithoutErrors()
    {
        // These are the exact queries that caused the production failure
        // They must work for the application to function

        // 1. ProcessWorkerService query that was failing
        var pendingProcesses = await Context.ProcessExecutions
            .Where(pe => pe.State == "Pending")
            .OrderBy(pe => pe.CreatedAt)
            .Take(10)
            .Select(pe => new { pe.UserId, pe.Id, pe.ProcessType, pe.State })
            .ToListAsync();
        Assert.NotNull(pendingProcesses);

        // 2. All user-partitioned queries must work
        var userQueries = new Func<Task>[]
        {
            () => Context.Journeys.Where(j => j.UserId != Guid.Empty).Take(1).ToListAsync(),
            () => Context.ProcessExecutions.Where(pe => pe.UserId != Guid.Empty).Take(1).ToListAsync(),
            () => Context.ProcessResults.Where(pr => pr.UserId != Guid.Empty).Take(1).ToListAsync(),
            () => Context.Personas.Where(p => p.UserId != Guid.Empty).Take(1).ToListAsync()
        };

        foreach (var query in userQueries)
        {
            await query(); // Must not throw
        }

        // 3. Composite key queries must work
        var compositeKeyQueries = new Func<Task>[]
        {
            () => Context.ProcessExecutions.Where(pe => pe.UserId == Guid.NewGuid() && pe.Id == Guid.NewGuid()).ToListAsync(),
            () => Context.Personas.Where(p => p.UserId == Guid.NewGuid() && p.Id == Guid.NewGuid()).ToListAsync(),
            () => Context.ProcessResults.Where(pr => pr.UserId == Guid.NewGuid() && pr.Id == Guid.NewGuid()).ToListAsync()
        };

        foreach (var query in compositeKeyQueries)
        {
            await query(); // Must not throw
        }
    }

    [Fact]
    public void DatabaseModel_ShouldMatchEntityFrameworkExpectations()
    {
        // Validate that the database model can be created without errors
        var model = Context.Model;
        Assert.NotNull(model);

        // Check critical entity types exist in the model
        var criticalEntities = new[]
        {
            typeof(Veritheia.Data.Entities.ProcessExecution),
            typeof(Veritheia.Data.Entities.ProcessResult),
            typeof(Veritheia.Data.Entities.Journey),
            typeof(Veritheia.Data.Entities.User),
            typeof(Veritheia.Data.Entities.Persona)
        };

        foreach (var entityType in criticalEntities)
        {
            var entityTypeModel = model.FindEntityType(entityType);
            Assert.NotNull(entityTypeModel);

            // For user-owned entities, ensure UserId property exists
            if (typeof(Veritheia.Data.Interfaces.IUserOwned).IsAssignableFrom(entityType))
            {
                var userIdProperty = entityTypeModel.FindProperty("UserId");
                Assert.NotNull(userIdProperty);
            }
        }
    }

    [Fact]
    public void MigrationHistory_ShouldBeConsistent()
    {
        // Ensure all migrations have been applied
        var appliedMigrations = Context.Database.GetAppliedMigrations().ToList();
        Assert.NotEmpty(appliedMigrations);

        // Check that the latest migration includes the composite primary keys
        Assert.Contains("20250820070909_CompositePrimaryKeys", appliedMigrations);

        // Ensure no pending migrations
        var pendingMigrations = Context.Database.GetPendingMigrations().ToList();
        Assert.Empty(pendingMigrations);
    }

    [Fact]
    public async Task PgVector_ShouldBeProperlyConfigured()
    {
        // Test that pgvector extension is available and vector columns work
        try
        {
            // This query will fail if pgvector is not properly configured
            await Context.Database.ExecuteSqlRawAsync("SELECT '[1,2,3]'::vector(3)");
        }
        catch (Exception ex)
        {
            Assert.Fail($"pgvector is not properly configured: {ex.Message}");
        }

        // Test vector table queries
        var vectorTables = new Func<Task>[]
        {
            () => Context.SearchVectors1536.Take(0).ToListAsync(),
            () => Context.SearchVectors768.Take(0).ToListAsync(),
            () => Context.SearchVectors384.Take(0).ToListAsync()
        };

        foreach (var query in vectorTables)
        {
            await query(); // Must not throw
        }
    }
}