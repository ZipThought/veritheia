using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Veritheia.Data.Entities;
using Veritheia.Data.Services;
using veritheia.Tests.TestBase;
using Xunit;

namespace Veritheia.Tests.Phase7_UserJourneySystem;

/// <summary>
/// Tests for Phase 7 User & Journey System
/// Validates user management, journey lifecycle, personas, and journaling
/// </summary>
[Collection("DatabaseTests")]
[Trait("Category", "Integration")]
public class UserJourneySystemTests : DatabaseTestBase
{
    public UserJourneySystemTests(DatabaseFixture fixture) : base(fixture)
    {
    }
    [Fact]
    public async Task UserService_CreatesDefaultUser()
    {
        // Arrange
        var logger = new Mock<ILogger<UserService>>();
        var service = new UserService(Context, logger.Object);
        
        // Act
        var user = await service.GetOrCreateDefaultUserAsync();
        
        // Assert
        Assert.NotNull(user);
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("default@veritheia.local", user.Email);
        Assert.Equal("Default User", user.DisplayName);
        
        // Verify persisted
        var dbUser = await Context.Users.FindAsync(user.Id);
        Assert.NotNull(dbUser);
    }
    
    [Fact]
    public async Task UserService_ReturnsExistingDefaultUser()
    {
        // Arrange
        var logger = new Mock<ILogger<UserService>>();
        var service = new UserService(Context, logger.Object);
        
        // Act
        var user1 = await service.GetOrCreateDefaultUserAsync();
        var user2 = await service.GetOrCreateDefaultUserAsync();
        
        // Assert
        Assert.Equal(user1.Id, user2.Id);
        
        // Verify only one user in database
        var userCount = await Context.Users.CountAsync();
        Assert.Equal(1, userCount);
    }
    
    [Fact]
    public async Task UserService_GetsUserWithContext()
    {
        // Arrange
        var logger = new Mock<ILogger<UserService>>();
        var service = new UserService(Context, logger.Object);
        
        var user = await service.GetOrCreateDefaultUserAsync();
        
        // Create some journeys and personas
        var persona = new Persona
        {
            Id = Guid.NewGuid(),
            Name = "Test Persona",
            Perspective = "Testing",
            CreatedAt = DateTime.UtcNow
        };
        
        var journey = new Journey
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            PersonaId = persona.Id,
            Purpose = "Test Journey",
            State = "Active",
            StartedAt = DateTime.UtcNow
        };
        
        Context.Personas.Add(persona);
        Context.Journeys.Add(journey);
        await Context.SaveChangesAsync();
        
        // Act
        var userWithContext = await service.GetUserWithContextAsync(user.Id);
        
        // Assert
        Assert.NotNull(userWithContext);
        Assert.NotEmpty(userWithContext.Journeys);
        Assert.Equal(journey.Id, userWithContext.Journeys.First().Id);
    }
    
    [Fact]
    public async Task JourneyService_CreatesJourney()
    {
        // Arrange
        var userLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(Context, userLogger.Object);
        var user = await userService.GetOrCreateDefaultUserAsync();
        
        var persona = new Persona
        {
            Id = Guid.NewGuid(),
            Name = "Research Persona",
            Perspective = "Academic research",
            CreatedAt = DateTime.UtcNow
        };
        Context.Personas.Add(persona);
        await Context.SaveChangesAsync();
        
        var journeyLogger = new Mock<ILogger<JourneyService>>();
        var journeyService = new JourneyService(Context, journeyLogger.Object);
        
        // Act
        var journey = await journeyService.CreateJourneyAsync(
            user.Id,
            persona.Id,
            "Literature review on AI in education");
        
        // Assert
        Assert.NotNull(journey);
        Assert.NotEqual(Guid.Empty, journey.Id);
        Assert.Equal("Literature review on AI in education", journey.Purpose);
        Assert.Equal("Active", journey.State);
        Assert.NotNull(journey.StartedAt);
        
        // Verify persisted with relationships
        var dbJourney = await Context.Journeys
            .Include(j => j.User)
            .Include(j => j.Persona)
            .FirstAsync(j => j.Id == journey.Id);
        
        Assert.NotNull(dbJourney.User);
        Assert.NotNull(dbJourney.Persona);
    }
    
    [Fact]
    public async Task JourneyService_ResumesJourney()
    {
        // Arrange
        var journeyLogger = new Mock<ILogger<JourneyService>>();
        var journeyService = new JourneyService(Context, journeyLogger.Object);
        
        var journey = await CreateTestJourney();
        journey.State = "Paused";
        journey.PausedAt = DateTime.UtcNow.AddHours(-1);
        await Context.SaveChangesAsync();
        
        // Act
        var resumed = await journeyService.ResumeJourneyAsync(journey.Id);
        
        // Assert
        Assert.NotNull(resumed);
        Assert.Equal("Active", resumed.State);
        Assert.NotNull(resumed.ResumedAt);
        Assert.True(resumed.ResumedAt > resumed.PausedAt);
    }
    
    [Fact]
    public async Task JourneyService_ArchivesJourney()
    {
        // Arrange
        var journeyLogger = new Mock<ILogger<JourneyService>>();
        var journeyService = new JourneyService(Context, journeyLogger.Object);
        
        var journey = await CreateTestJourney();
        
        // Act
        await journeyService.ArchiveJourneyAsync(journey.Id);
        
        // Assert
        var archived = await Context.Journeys.FindAsync(journey.Id);
        Assert.Equal("Archived", archived.State);
        Assert.NotNull(archived.ArchivedAt);
    }
    
    [Fact]
    public async Task JourneyService_GetsActiveJourneys()
    {
        // Arrange
        var journeyLogger = new Mock<ILogger<JourneyService>>();
        var journeyService = new JourneyService(Context, journeyLogger.Object);
        
        var user = await CreateTestUser();
        var persona = await CreateTestPersona();
        
        // Create multiple journeys with different states
        var activeJourney1 = await CreateJourney(user.Id, persona.Id, "Active Journey 1", "Active");
        var activeJourney2 = await CreateJourney(user.Id, persona.Id, "Active Journey 2", "Active");
        var pausedJourney = await CreateJourney(user.Id, persona.Id, "Paused Journey", "Paused");
        var archivedJourney = await CreateJourney(user.Id, persona.Id, "Archived Journey", "Archived");
        
        // Act
        var activeJourneys = await journeyService.GetActiveJourneysAsync(user.Id);
        
        // Assert
        Assert.Equal(2, activeJourneys.Count);
        Assert.All(activeJourneys, j => Assert.Equal("Active", j.State));
        Assert.Contains(activeJourneys, j => j.Purpose == "Active Journey 1");
        Assert.Contains(activeJourneys, j => j.Purpose == "Active Journey 2");
    }
    
    [Fact]
    public async Task PersonaService_TracksVocabularyEvolution()
    {
        // Arrange
        var personaLogger = new Mock<ILogger<PersonaService>>();
        var personaService = new PersonaService(Context, personaLogger.Object);
        
        var persona = await CreateTestPersona();
        
        // Act
        await personaService.UpdateVocabularyAsync(
            persona.Id,
            new[] { "epistemic", "ontological", "phenomenological" });
        
        // Assert
        var updated = await Context.Personas.FindAsync(persona.Id);
        Assert.NotNull(updated.Vocabulary);
        Assert.Contains("epistemic", updated.Vocabulary);
        Assert.Contains("phenomenological", updated.Vocabulary);
    }
    
    [Fact]
    public async Task PersonaService_RecordsPatternRecognition()
    {
        // Arrange
        var personaLogger = new Mock<ILogger<PersonaService>>();
        var personaService = new PersonaService(Context, personaLogger.Object);
        
        var persona = await CreateTestPersona();
        
        // Act
        await personaService.RecordPatternAsync(
            persona.Id,
            "evidence-based reasoning",
            0.85f);
        
        // Assert
        var updated = await Context.Personas.FindAsync(persona.Id);
        Assert.NotNull(updated.Patterns);
        Assert.Contains("evidence-based reasoning", updated.Patterns);
    }
    
    [Fact]
    public async Task JournalService_CreatesJournalEntries()
    {
        // Arrange
        var journalLogger = new Mock<ILogger<JournalService>>();
        var journalService = new JournalService(Context, journalLogger.Object);
        
        var journey = await CreateTestJourney();
        
        // Act
        var entry = await journalService.CreateEntryAsync(
            journey.Id,
            "Research",
            "Initial investigation into AI applications",
            new { sources = 5, papers_reviewed = 3 });
        
        // Assert
        Assert.NotNull(entry);
        Assert.Equal(journey.Id, entry.JourneyId);
        Assert.Equal("Research", entry.EntryType);
        Assert.Contains("Initial investigation", entry.Content);
        Assert.NotNull(entry.Metadata);
        Assert.Equal(5, (int)entry.Metadata["sources"]);
    }
    
    [Fact]
    public async Task JournalService_RetrievesJourneyEntries()
    {
        // Arrange
        var journalLogger = new Mock<ILogger<JournalService>>();
        var journalService = new JournalService(Context, journalLogger.Object);
        
        var journey = await CreateTestJourney();
        
        // Create multiple entries
        await journalService.CreateEntryAsync(journey.Id, "Research", "First entry", null);
        await journalService.CreateEntryAsync(journey.Id, "Method", "Second entry", null);
        await journalService.CreateEntryAsync(journey.Id, "Decision", "Third entry", null);
        
        // Act
        var entries = await journalService.GetJourneyEntriesAsync(journey.Id);
        
        // Assert
        Assert.Equal(3, entries.Count);
        Assert.Contains(entries, e => e.EntryType == "Research");
        Assert.Contains(entries, e => e.EntryType == "Method");
        Assert.Contains(entries, e => e.EntryType == "Decision");
        
        // Verify chronological order
        Assert.True(entries[0].CreatedAt <= entries[1].CreatedAt);
        Assert.True(entries[1].CreatedAt <= entries[2].CreatedAt);
    }
    
    [Fact]
    public async Task JournalService_FiltersEntriesByType()
    {
        // Arrange
        var journalLogger = new Mock<ILogger<JournalService>>();
        var journalService = new JournalService(Context, journalLogger.Object);
        
        var journey = await CreateTestJourney();
        
        // Create entries of different types
        await journalService.CreateEntryAsync(journey.Id, "Research", "Research 1", null);
        await journalService.CreateEntryAsync(journey.Id, "Research", "Research 2", null);
        await journalService.CreateEntryAsync(journey.Id, "Method", "Method entry", null);
        await journalService.CreateEntryAsync(journey.Id, "Decision", "Decision entry", null);
        
        // Act
        var researchEntries = await journalService.GetEntriesByTypeAsync(journey.Id, "Research");
        
        // Assert
        Assert.Equal(2, researchEntries.Count);
        Assert.All(researchEntries, e => Assert.Equal("Research", e.EntryType));
    }
    
    // Helper methods
    private async Task<User> CreateTestUser()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"test-{Guid.NewGuid()}@example.com",
            DisplayName = "Test User",
            CreatedAt = DateTime.UtcNow
        };
        Context.Users.Add(user);
        await Context.SaveChangesAsync();
        return user;
    }
    
    private async Task<Persona> CreateTestPersona()
    {
        var persona = new Persona
        {
            Id = Guid.NewGuid(),
            Name = "Test Persona",
            Perspective = "Testing perspective",
            Vocabulary = new[] { "test", "validate" },
            Patterns = new[] { "systematic" },
            CreatedAt = DateTime.UtcNow
        };
        Context.Personas.Add(persona);
        await Context.SaveChangesAsync();
        return persona;
    }
    
    private async Task<Journey> CreateTestJourney()
    {
        var user = await CreateTestUser();
        var persona = await CreateTestPersona();
        
        var journey = new Journey
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            PersonaId = persona.Id,
            Purpose = "Test Journey",
            State = "Active",
            StartedAt = DateTime.UtcNow
        };
        Context.Journeys.Add(journey);
        await Context.SaveChangesAsync();
        return journey;
    }
    
    private async Task<Journey> CreateJourney(Guid userId, Guid personaId, string purpose, string state)
    {
        var journey = new Journey
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PersonaId = personaId,
            Purpose = purpose,
            State = state,
            StartedAt = DateTime.UtcNow
        };
        
        if (state == "Archived")
        {
            journey.ArchivedAt = DateTime.UtcNow;
        }
        
        Context.Journeys.Add(journey);
        await Context.SaveChangesAsync();
        return journey;
    }
}