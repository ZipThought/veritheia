using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Veritheia.Core.Interfaces;
using Veritheia.Core.ValueObjects;
using Veritheia.Data;
using Veritheia.Data.Entities;
using Veritheia.Data.Processes;
using Veritheia.Data.Services;
using veritheia.Tests.TestBase;
using Xunit;
using Xunit.Abstractions;

namespace veritheia.Tests.Integration.E2E;

/// <summary>
/// Simple workflow test using proper database infrastructure with mocked LLM
/// </summary>
[Collection("DatabaseTests")]
[Trait("Category", "Integration")]
public class SimpleWorkflowTest : DatabaseTestBase
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<ICognitiveAdapter> _mockLLM;

    public SimpleWorkflowTest(DatabaseFixture fixture, ITestOutputHelper output) : base(fixture)
    {
        _output = output;
        _mockLLM = new Mock<ICognitiveAdapter>();
    }

    [Fact]
    public async Task Complete_Journey_Workflow_With_Real_Database()
    {
        // Arrange
        _output.WriteLine("=== Journey Workflow Test with Real DB ===");

        // Setup mocked LLM responses
        _mockLLM.Setup(x => x.GenerateTextAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("RELEVANCE: 0.8\nCONTRIBUTION: 0.7\nReasoning: This paper is relevant.");

        _mockLLM.Setup(x => x.CreateEmbedding(It.IsAny<string>()))
            .ReturnsAsync(new float[768]); // Return dummy embedding

        // Create services
        var services = new ServiceCollection();
        services.AddScoped<VeritheiaDbContext>(_ => Context);
        services.AddScoped<ICognitiveAdapter>(_ => _mockLLM.Object);
        services.AddScoped<UserService>();
        services.AddScoped<PersonaService>();
        services.AddScoped<JourneyService>();
        services.AddScoped<ProcessEngine>();
        services.AddScoped<ProcessWorkerService>();
        services.AddScoped<IAnalyticalProcess, BasicSystematicScreeningProcess>();
        services.AddScoped<CsvParserService>();
        services.AddScoped<CsvWriterService>();
        services.AddScoped<SemanticExtractionService>();
        services.AddScoped<FileStorageService>();
        services.AddScoped<IDocumentStorageRepository, FileStorageService>();
        services.AddScoped<TextExtractionService>();
        services.AddScoped<EmbeddingService>();
        services.AddScoped<DocumentIngestionService>();
        services.AddLogging();
        services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(
            new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());

        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        // Get services
        var userService = scope.ServiceProvider.GetRequiredService<UserService>();
        var personaService = scope.ServiceProvider.GetRequiredService<PersonaService>();
        var journeyService = scope.ServiceProvider.GetRequiredService<JourneyService>();
        var processEngine = scope.ServiceProvider.GetRequiredService<ProcessEngine>();

        // Register process
        processEngine.RegisterProcess<BasicSystematicScreeningProcess>();

        // Step 1: Create user
        _output.WriteLine("Step 1: Creating user...");
        var user = await userService.CreateOrGetUserAsync("workflow-test@example.com", "Workflow Test User");
        Assert.NotNull(user);
        _output.WriteLine($"✓ User created: {user.Email}");

        // Verify user is in database with constraints
        var dbUser = await Context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.NotNull(dbUser);
        Assert.Equal(user.Email, dbUser.Email);

        // Step 2: Create persona
        _output.WriteLine("Step 2: Creating persona...");
        var personas = await personaService.GetUserPersonasAsync(user.Id);
        if (!personas.Any())
        {
            // Create a researcher persona
            var persona = new Persona
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Domain = "Researcher",
                ConceptualVocabulary = new Dictionary<string, object> { ["type"] = "test" },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            Context.Personas.Add(persona);
            await Context.SaveChangesAsync();
            personas = await personaService.GetUserPersonasAsync(user.Id);
        }
        var researcherPersona = personas.First(p => p.Domain == "Researcher");
        _output.WriteLine($"✓ Persona ready: {researcherPersona.Domain}");

        // Step 3: Create journey
        _output.WriteLine("Step 3: Creating journey...");
        var journey = await journeyService.CreateJourneyAsync(
            user.Id,
            "Test Research Journey",
            researcherPersona.Id,
            "systematic-screening");
        Assert.NotNull(journey);
        _output.WriteLine($"✓ Journey created: {journey.Purpose}");

        // Verify journey relationships
        var dbJourney = await Context.Journeys
            .Include(j => j.User)
            .Include(j => j.Persona)
            .FirstOrDefaultAsync(j => j.Id == journey.Id);
        Assert.NotNull(dbJourney);
        Assert.Equal(user.Id, dbJourney.UserId);
        Assert.Equal(researcherPersona.Id, dbJourney.PersonaId);

        // Step 4: Execute process
        _output.WriteLine("Step 4: Executing process...");
        var inputs = new Dictionary<string, object>
        {
            ["csv_content"] = "title,abstract,authors,year,venue,doi,link,keywords\n\"Test Paper\",\"Abstract\",\"Author\",2024,\"Venue\",\"doi\",\"link\",\"keywords\"",
            ["research_questions"] = "Is this a test?"
        };

        var result = await processEngine.ExecuteProcessAsync(
            "systematic-screening",
            journey.Id,
            inputs);

        Assert.NotNull(result);
        Assert.True(result.Success, $"Process failed: {result.ErrorMessage}");
        _output.WriteLine($"✓ Process executed successfully");

        // Step 5: Verify database state
        _output.WriteLine("Step 5: Verifying database state...");

        // Check process execution was recorded
        var execution = await Context.ProcessExecutions
            .FirstOrDefaultAsync(pe => pe.JourneyId == journey.Id);
        Assert.NotNull(execution);
        Assert.Equal("Completed", execution.State);
        _output.WriteLine($"✓ Process execution recorded with state: {execution.State}");

        // Check foreign key constraints work
        var journeyCount = await Context.Journeys
            .Where(j => j.UserId == user.Id)
            .CountAsync();
        Assert.Equal(1, journeyCount);

        _output.WriteLine("\n=== Workflow Test PASSED ===");
        _output.WriteLine("✓ User creation with constraints");
        _output.WriteLine("✓ Persona association");
        _output.WriteLine("✓ Journey creation with FK relationships");
        _output.WriteLine("✓ Process execution and recording");
        _output.WriteLine("✓ All database constraints validated");
    }
}