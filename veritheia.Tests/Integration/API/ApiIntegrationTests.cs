using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Veritheia.ApiService;
using Veritheia.Data.Services;
using Veritheia.Data.DTOs;
using Veritheia.Data.Entities;
using veritheia.Tests.TestBase;
using Xunit;

namespace veritheia.Tests.Integration.API;

/// <summary>
/// API integration tests - validates the complete API layer functionality
/// 
/// This test suite validates:
/// - Service layer integration with database
/// - Journey CRUD operations (Create, Read, Update, Archive)
/// - User management (demo user retrieval)
/// - Persona management (active personas retrieval)
/// - Data persistence and retrieval
/// - Business logic validation
/// 
/// These tests ensure the API layer works correctly with real database operations
/// and validates the core functionality that the UI depends on.
/// </summary>
[Trait("Category", "Integration")]
public class ApiIntegrationTests : DatabaseTestBase
{
    public ApiIntegrationTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    private async Task<User> GetDemoUser()
    {
        // The demo user is created by database seeding
        var user = await Context.Users.FirstOrDefaultAsync(u => u.Email == "demo@veritheia.local");
        if (user == null)
        {
            throw new InvalidOperationException("Demo user not found. Database seeding may have failed.");
        }
        return user;
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
    public async Task GetJourneys_ShouldReturnEmptyList_WhenNoJourneysExist()
    {
        // Arrange
        var (userService, personaService, journeyService) = CreateServices();
        var user = await GetDemoUser();

        // Act
        var journeys = await journeyService.GetUserJourneysAsync(user.Id);

        // Assert
        Assert.NotNull(journeys);
        Assert.Empty(journeys);
    }

    [Fact]
    public async Task CreateJourney_ShouldReturnCreatedJourney()
    {
        // Arrange
        var (userService, personaService, journeyService) = CreateServices();
        var user = await GetDemoUser();
        var personas = await personaService.GetActivePersonasAsync(user.Id);
        var researcherPersona = personas.First(p => p.Domain == "Researcher");

        // Act
        var journey = await journeyService.CreateJourneyAsync(
            user.Id,
            "Test Journey",
            researcherPersona.Id,
            "systematic-screening");

        // Assert
        Assert.NotNull(journey);
        Assert.Equal("Test Journey", journey.Purpose);
        Assert.Equal(researcherPersona.Id, journey.PersonaId);
        Assert.Equal("systematic-screening", journey.ProcessType);
        Assert.Equal("Active", journey.State);
    }

    [Fact]
    public async Task GetJourney_ShouldReturnJourney_WhenExists()
    {
        // Arrange - Create a journey first
        var (userService, personaService, journeyService) = CreateServices();
        var user = await GetDemoUser();
        var personas = await personaService.GetActivePersonasAsync(user.Id);
        var researcherPersona = personas.First(p => p.Domain == "Researcher");

        var createdJourney = await journeyService.CreateJourneyAsync(
            user.Id,
            "Test Journey for Get",
            researcherPersona.Id,
            "systematic-screening");

        // Act
        var journey = await journeyService.GetJourneyAsync(user.Id, createdJourney.Id);

        // Assert
        Assert.NotNull(journey);
        Assert.Equal(createdJourney.Id, journey.Id);
        Assert.Equal("Test Journey for Get", journey.Purpose);
    }

    [Fact]
    public async Task GetJourney_ShouldReturnNull_WhenDoesNotExist()
    {
        // Arrange
        var (userService, personaService, journeyService) = CreateServices();
        var user = await GetDemoUser();

        // Act
        var journey = await journeyService.GetJourneyAsync(user.Id, Guid.NewGuid());

        // Assert
        Assert.Null(journey);
    }

    [Fact]
    public async Task UpdateJourney_ShouldUpdateJourney()
    {
        // Arrange - Create a journey first
        var (userService, personaService, journeyService) = CreateServices();
        var user = await GetDemoUser();
        var personas = await personaService.GetActivePersonasAsync(user.Id);
        var researcherPersona = personas.First(p => p.Domain == "Researcher");

        var createdJourney = await journeyService.CreateJourneyAsync(
            user.Id,
            "Original Purpose",
            researcherPersona.Id,
            "systematic-screening");

        var updateContext = new Dictionary<string, object> { { "key", "value" } };

        // Act
        var updatedJourney = await journeyService.UpdateJourneyAsync(
            user.Id,
            createdJourney.Id,
            "Completed",
            updateContext);

        // Assert
        Assert.NotNull(updatedJourney);
        Assert.Equal("Completed", updatedJourney.State);
        Assert.Equal(createdJourney.Id, updatedJourney.Id);
        Assert.Equal("value", updatedJourney.Context["key"]);
    }

    [Fact]
    public async Task ArchiveJourney_ShouldArchiveSuccessfully()
    {
        // Arrange - Create a journey first
        var (userService, personaService, journeyService) = CreateServices();
        var user = await GetDemoUser();
        var personas = await personaService.GetActivePersonasAsync(user.Id);
        var researcherPersona = personas.First(p => p.Domain == "Researcher");

        var createdJourney = await journeyService.CreateJourneyAsync(
            user.Id,
            "Journey to Archive",
            researcherPersona.Id,
            "systematic-screening");

        // Act
        await journeyService.ArchiveJourneyAsync(user.Id, createdJourney.Id);

        // Assert - Journey should be archived (state changed to Abandoned)
        var archivedJourney = await journeyService.GetJourneyAsync(user.Id, createdJourney.Id);
        Assert.NotNull(archivedJourney);
        Assert.Equal("Abandoned", archivedJourney.State);
    }

    [Fact]
    public async Task GetJourneyStatistics_ShouldReturnStatistics()
    {
        // Arrange
        var (userService, personaService, journeyService) = CreateServices();
        var user = await GetDemoUser();

        // Act
        var statistics = await journeyService.GetJourneyStatisticsAsync(user.Id);

        // Assert
        Assert.NotNull(statistics);
        Assert.Equal(0, statistics.TotalJourneys);
        Assert.Equal(0, statistics.ActiveJourneys);
        Assert.Equal(0, statistics.CompletedJourneys);
    }

    [Fact]
    public async Task GetPersonas_ShouldReturnDefaultPersonas()
    {
        // Arrange
        var (userService, personaService, journeyService) = CreateServices();
        var user = await GetDemoUser();

        // Act
        var personas = await personaService.GetActivePersonasAsync(user.Id);

        // Assert
        Assert.NotNull(personas);
        Assert.NotEmpty(personas);
        Assert.Contains(personas, p => p.Domain == "Researcher");
        Assert.Contains(personas, p => p.Domain == "Student");
        Assert.Contains(personas, p => p.Domain == "Entrepreneur");
    }

    [Fact]
    public async Task GetUser_ShouldReturnDemoUser()
    {
        // Arrange
        var (userService, personaService, journeyService) = CreateServices();

        // Act - Get demo user from database
        var user = await GetDemoUser();

        // Assert
        Assert.NotNull(user);
        Assert.Equal("demo@veritheia.local", user.Email);
        Assert.Equal("Dr. Sarah Chen", user.DisplayName);
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
