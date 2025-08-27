using Microsoft.EntityFrameworkCore;
using veritheia.Tests.TestBase;
using Veritheia.Data;
using Veritheia.Data.Entities;
using Xunit;

namespace veritheia.Tests.Integration.Database;

[Trait("Category", "Integration")]
public class DatabaseSchemaValidationTests : DatabaseTestBase
{
    public DatabaseSchemaValidationTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Database_ShouldHaveAllRequiredTables()
    {
        // Verify all DbSets can be queried (confirms tables exist)
        var tableChecks = new Func<Task>[]
        {
            () => Context.Users.Take(0).ToListAsync(),
            () => Context.Personas.Take(0).ToListAsync(),
            () => Context.ProcessCapabilities.Take(0).ToListAsync(),
            () => Context.Journeys.Take(0).ToListAsync(),
            () => Context.JourneyFrameworks.Take(0).ToListAsync(),
            () => Context.Journals.Take(0).ToListAsync(),
            () => Context.JournalEntries.Take(0).ToListAsync(),
            () => Context.Documents.Take(0).ToListAsync(),
            () => Context.DocumentMetadata.Take(0).ToListAsync(),
            () => Context.KnowledgeScopes.Take(0).ToListAsync(),
            () => Context.JourneyDocumentSegments.Take(0).ToListAsync(),
            () => Context.JourneySegmentAssessments.Take(0).ToListAsync(),
            () => Context.JourneyFormations.Take(0).ToListAsync(),
            () => Context.SearchIndexes.Take(0).ToListAsync(),
            () => Context.SearchVectors.Take(0).ToListAsync(),
            () => Context.ProcessDefinitions.Take(0).ToListAsync(),
            () => Context.ProcessExecutions.Take(0).ToListAsync(),
            () => Context.ProcessResults.Take(0).ToListAsync()
        };

        foreach (var check in tableChecks)
        {
            await check(); // Should not throw if table exists with correct schema
        }
    }

    [Fact]
    public async Task ProcessExecutions_ShouldHaveUserIdColumn()
    {
        // This test specifically validates the ProcessExecution entity can be queried
        // with UserId field, which was the source of the schema mismatch
        var query = Context.ProcessExecutions
            .Where(pe => pe.UserId != Guid.Empty)
            .Select(pe => new { pe.UserId, pe.Id, pe.State })
            .Take(0);

        var result = await query.ToListAsync();

        // If we reach here without exception, the schema is correct
        Assert.NotNull(result);
    }

    [Fact]
    public async Task AllUserOwnedEntities_ShouldHaveUserIdColumn()
    {
        // Test all entities that implement IUserOwned have UserId in their queries
        var userOwnedChecks = new Func<Task>[]
        {
            () => Context.Personas.Where(e => e.UserId != Guid.Empty).Take(0).ToListAsync(),
            () => Context.ProcessCapabilities.Where(e => e.UserId != Guid.Empty).Take(0).ToListAsync(),
            () => Context.Journeys.Where(e => e.UserId != Guid.Empty).Take(0).ToListAsync(),
            () => Context.JourneyFrameworks.Where(e => e.UserId != Guid.Empty).Take(0).ToListAsync(),
            () => Context.Journals.Where(e => e.UserId != Guid.Empty).Take(0).ToListAsync(),
            () => Context.JournalEntries.Where(e => e.UserId != Guid.Empty).Take(0).ToListAsync(),
            () => Context.Documents.Where(e => e.UserId != Guid.Empty).Take(0).ToListAsync(),
            () => Context.DocumentMetadata.Where(e => e.UserId != Guid.Empty).Take(0).ToListAsync(),
            () => Context.KnowledgeScopes.Where(e => e.UserId != Guid.Empty).Take(0).ToListAsync(),
            () => Context.JourneyDocumentSegments.Where(e => e.UserId != Guid.Empty).Take(0).ToListAsync(),
            () => Context.JourneySegmentAssessments.Where(e => e.UserId != Guid.Empty).Take(0).ToListAsync(),
            () => Context.JourneyFormations.Where(e => e.UserId != Guid.Empty).Take(0).ToListAsync(),
            () => Context.SearchIndexes.Where(e => e.UserId != Guid.Empty).Take(0).ToListAsync(),
            () => Context.SearchVectors.Where(e => e.UserId != Guid.Empty).Take(0).ToListAsync(),
            () => Context.ProcessDefinitions.Where(e => e.UserId != Guid.Empty).Take(0).ToListAsync(),
            () => Context.ProcessExecutions.Where(e => e.UserId != Guid.Empty).Take(0).ToListAsync(),
            () => Context.ProcessResults.Where(e => e.UserId != Guid.Empty).Take(0).ToListAsync()
        };

        foreach (var check in userOwnedChecks)
        {
            await check(); // Should not throw if UserId column exists
        }
    }

    [Fact]
    public async Task ProcessWorkerService_ShouldBeAbleToQueryPendingExecutions()
    {
        // This replicates the exact query that was failing in ProcessWorkerService
        var pendingExecutions = await Context.ProcessExecutions
            .Where(pe => pe.State == "Pending")
            .OrderBy(pe => pe.CreatedAt)
            .Take(10)
            .ToListAsync();

        // Should not throw - validates the schema matches the query expectations
        Assert.NotNull(pendingExecutions);
    }

    [Fact]
    public async Task VectorTables_ShouldSupportPgvectorTypes()
    {
        // Test that pgvector columns can be queried without errors
        var vectorChecks = new Func<Task>[]
        {
            () => Context.SearchVectors.Take(0).ToListAsync()
        };

        foreach (var check in vectorChecks)
        {
            await check(); // Should not throw if pgvector is properly configured
        }
    }
}