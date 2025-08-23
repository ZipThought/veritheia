using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Veritheia.Data.Services;
using Veritheia.Data.Entities;
using veritheia.Tests.TestBase;
using Xunit;

namespace veritheia.Tests.Integration.UI;

/// <summary>
/// Integration tests for Journey UI workflow - testing the complete user experience
/// This validates the "formation through authorship" specification requirement
/// </summary>
[Trait("Category", "Integration")]
public class JourneyUIIntegrationTests : DatabaseTestBase
{
    public JourneyUIIntegrationTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    private (UserService userService, PersonaService personaService, JourneyService journeyService) CreateServices()
    {
        // Create services with the initialized test database context
        var serviceProvider = new ServiceCollection()
            .AddSingleton<ILogger<UserService>>(new TestLogger<UserService>())
            .AddSingleton<ILogger<PersonaService>>(new TestLogger<PersonaService>())
            .AddSingleton<ILogger<JourneyService>>(new TestLogger<JourneyService>())
            .BuildServiceProvider();

        var personaService = new PersonaService(Context, serviceProvider.GetRequiredService<ILogger<PersonaService>>());
        var userService = new UserService(Context, personaService, serviceProvider.GetRequiredService<ILogger<UserService>>());
        var journeyService = new JourneyService(Context, serviceProvider.GetRequiredService<ILogger<JourneyService>>());

        return (userService, personaService, journeyService);
    }

    [Fact]
    public async Task EndToEnd_CreateUserAndJourney_ShouldWork()
    {
        // Arrange - This simulates the user experience from Dashboard -> Create Journey
        var (userService, personaService, journeyService) = CreateServices();
        var userEmail = "test-integration@veritheia.local";
        var userName = "Integration Test User";

        // Act 1: User visits dashboard (creates demo user)
        var user = await userService.CreateOrGetUserAsync(userEmail, userName);
        
        // Assert: User should be created with default personas
        Assert.NotNull(user);
        Assert.Equal(userEmail, user.Email);
        Assert.Equal(userName, user.DisplayName);

        var personas = await personaService.GetActivePersonasAsync(user.Id);
        Assert.NotEmpty(personas);
        Assert.Contains(personas, p => p.Domain == "Researcher");
        Assert.Contains(personas, p => p.Domain == "Student");
        Assert.Contains(personas, p => p.Domain == "Entrepreneur");

        // Act 2: User creates a journey (simulates CreateJourney form submission)
        var researcherPersona = personas.First(p => p.Domain == "Researcher");
        var journey = await journeyService.CreateJourneyAsync(
            user.Id,
            "AI Security Literature Review",
            researcherPersona.Id,
            "systematic-screening");

        // Assert: Journey should be created correctly
        Assert.NotNull(journey);
        Assert.Equal(user.Id, journey.UserId);
        Assert.Equal(researcherPersona.Id, journey.PersonaId);
        Assert.Equal("AI Security Literature Review", journey.Purpose);
        Assert.Equal("systematic-screening", journey.ProcessType);
        Assert.Equal("Active", journey.State);

        // Act 3: Load dashboard data (simulates Dashboard.razor loading)
        var journeys = await journeyService.GetUserJourneysAsync(user.Id);
        var statistics = await journeyService.GetJourneyStatisticsAsync(user.Id);

        // Assert: Dashboard should show the created journey
        Assert.Single(journeys);
        Assert.Equal(journey.Id, journeys[0].Id);
        Assert.NotNull(journeys[0].Persona);
        Assert.Equal("Researcher", journeys[0].Persona.Domain);

        Assert.Equal(1, statistics.TotalJourneys);
        Assert.Equal(1, statistics.ActiveJourneys);
        Assert.Equal(0, statistics.CompletedJourneys);
    }

    [Fact]
    public async Task JourneyLifecycle_CreateUpdateArchive_ShouldWork()
    {
        // Arrange
        var (userService, personaService, journeyService) = CreateServices();
        var user = await userService.GetDemoUserAsync();
        var personas = await personaService.GetActivePersonasAsync(user.Id);
        var studentPersona = personas.First(p => p.Domain == "Student");

        // Act 1: Create journey
        var journey = await journeyService.CreateJourneyAsync(
            user.Id,
            "Research Methods Course",
            studentPersona.Id,
            "basic-constrained-composition");

        // Act 2: Update journey context (simulates process execution)
        var updatedContext = new Dictionary<string, object>
        {
            ["learning_objectives"] = new[] { "Understand research design", "Master data analysis" },
            ["current_phase"] = "literature_review",
            ["progress_percentage"] = 45
        };

        var updatedJourney = await journeyService.UpdateJourneyAsync(
            user.Id, 
            journey.Id, 
            "Active", 
            updatedContext);

        // Assert: Journey should be updated
        Assert.Equal(updatedContext["current_phase"], updatedJourney.Context["current_phase"]);
        Assert.Equal(45, updatedJourney.Context["progress_percentage"]);

        // Act 3: Archive journey (simulates Dashboard archive action)
        await journeyService.ArchiveJourneyAsync(user.Id, journey.Id);

        // Act 4: Check statistics after archiving
        var statisticsAfterArchive = await journeyService.GetJourneyStatisticsAsync(user.Id);

        // Assert: Journey should be archived
        Assert.Equal(1, statisticsAfterArchive.TotalJourneys);
        Assert.Equal(0, statisticsAfterArchive.ActiveJourneys); // Should be 0 after archiving
    }

    [Fact]
    public async Task MultipleJourneys_DifferentPersonas_ShouldWork()
    {
        // This test validates the "journey projection spaces" specification requirement
        // Same user, different personas should create different intellectual contexts

        // Arrange
        var (userService, personaService, journeyService) = CreateServices();
        var user = await userService.GetDemoUserAsync();
        var personas = await personaService.GetActivePersonasAsync(user.Id);

        var researcherPersona = personas.First(p => p.Domain == "Researcher");
        var entrepreneurPersona = personas.First(p => p.Domain == "Entrepreneur");
        var studentPersona = personas.First(p => p.Domain == "Student");

        // Act: Create multiple journeys with different personas
        var researchJourney = await journeyService.CreateJourneyAsync(
            user.Id, "Cybersecurity AI Research", researcherPersona.Id, "systematic-screening");

        var businessJourney = await journeyService.CreateJourneyAsync(
            user.Id, "Market Analysis for AI Startups", entrepreneurPersona.Id, "systematic-screening");

        var learningJourney = await journeyService.CreateJourneyAsync(
            user.Id, "Learning Data Science Fundamentals", studentPersona.Id, "basic-constrained-composition");

        // Assert: Each journey should have different intellectual contexts
        var allJourneys = await journeyService.GetUserJourneysAsync(user.Id);
        Assert.Equal(3, allJourneys.Count);

        // Verify each journey has the correct persona association
        var researchJourneyLoaded = allJourneys.First(j => j.Id == researchJourney.Id);
        Assert.Equal("Researcher", researchJourneyLoaded.Persona?.Domain);
        Assert.Equal("systematic-screening", researchJourneyLoaded.ProcessType);

        var businessJourneyLoaded = allJourneys.First(j => j.Id == businessJourney.Id);
        Assert.Equal("Entrepreneur", businessJourneyLoaded.Persona?.Domain);
        Assert.Equal("systematic-screening", businessJourneyLoaded.ProcessType);

        var learningJourneyLoaded = allJourneys.First(j => j.Id == learningJourney.Id);
        Assert.Equal("Student", learningJourneyLoaded.Persona?.Domain);
        Assert.Equal("basic-constrained-composition", learningJourneyLoaded.ProcessType);

        // Verify statistics
        var stats = await journeyService.GetJourneyStatisticsAsync(user.Id);
        Assert.Equal(3, stats.TotalJourneys);
        Assert.Equal(3, stats.ActiveJourneys);
    }

    [Fact]
    public async Task PersonaIntellectualFrameworks_ShouldHaveDistinctCharacteristics()
    {
        // This validates the "neurosymbolic transcendence" specification requirement
        // Each persona should have distinct conceptual vocabulary and patterns

        // Arrange & Act
        var (userService, personaService, journeyService) = CreateServices();
        var user = await userService.GetDemoUserAsync();
        var personas = await personaService.GetActivePersonasAsync(user.Id);

        // Assert: Each persona should have distinct intellectual frameworks
        var researcher = personas.First(p => p.Domain == "Researcher");
        var student = personas.First(p => p.Domain == "Student");
        var entrepreneur = personas.First(p => p.Domain == "Entrepreneur");

        // Researcher should have research-focused vocabulary
        Assert.Contains("methodological_terms", researcher.ConceptualVocabulary.Keys);
        Assert.Contains("assessment_criteria", researcher.ConceptualVocabulary.Keys);

        // Student should have learning-focused vocabulary
        Assert.Contains("learning_terms", student.ConceptualVocabulary.Keys);
        Assert.Contains("assessment_focus", student.ConceptualVocabulary.Keys);

        // Entrepreneur should have business-focused vocabulary
        Assert.Contains("business_terms", entrepreneur.ConceptualVocabulary.Keys);
        Assert.Contains("assessment_focus", entrepreneur.ConceptualVocabulary.Keys);

        // Each should have distinct methodological preferences
        Assert.NotEmpty(researcher.MethodologicalPreferences);
        Assert.NotEmpty(student.MethodologicalPreferences);
        Assert.NotEmpty(entrepreneur.MethodologicalPreferences);
    }
}

/// <summary>
/// Test logger implementation for integration tests
/// </summary>
public class TestLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        // For integration tests, we can output to console or ignore
        // Console.WriteLine($"[{logLevel}] {formatter(state, exception)}");
    }
}
