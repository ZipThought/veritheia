using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Veritheia.Data.Entities;
using veritheia.Tests.TestBase;
using Xunit;

namespace veritheia.Tests.Integration.Database;

public class CrudOperationsTests : DatabaseTestBase
{
    public CrudOperationsTests(DatabaseFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Can_Insert_User_Entity()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "test@example.com",
            DisplayName = "Test User",
            LastActiveAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        // Assert
        var savedUser = await Context.Users.FindAsync(user.Id);
        Assert.NotNull(savedUser);
        Assert.Equal("test@example.com", savedUser.Email);
        Assert.Equal("Test User", savedUser.DisplayName);
    }

    [Fact]
    public async Task Can_Insert_Journey_With_User_Relationship()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "researcher@example.com",
            DisplayName = "Researcher",
            LastActiveAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        var persona = new Persona
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            Domain = "Research",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var journey = new Journey
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            PersonaId = persona.Id,
            Purpose = "Test systematic review",
            State = "Active",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        Context.Users.Add(user);
        Context.Personas.Add(persona);
        Context.Journeys.Add(journey);
        await Context.SaveChangesAsync();

        // Assert
        var savedJourney = await Context.Journeys
            .Include(j => j.User)
            .FirstOrDefaultAsync(j => j.Id == journey.Id);

        Assert.NotNull(savedJourney);
        Assert.Equal("Test systematic review", savedJourney.Purpose);
        Assert.Equal("Active", savedJourney.State);
        Assert.NotNull(savedJourney.User);
        Assert.Equal("researcher@example.com", savedJourney.User.Email);
    }

    [Fact]
    public async Task Can_Update_Entity()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "original@example.com",
            DisplayName = "Original Name",
            LastActiveAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        // Act
        user.DisplayName = "Updated Name";
        user.UpdatedAt = DateTime.UtcNow;
        await Context.SaveChangesAsync();

        // Assert
        var updatedUser = await Context.Users.FindAsync(user.Id);
        Assert.NotNull(updatedUser);
        Assert.Equal("Updated Name", updatedUser.DisplayName);
        Assert.NotNull(updatedUser.UpdatedAt);
    }

    [Fact]
    public async Task Can_Delete_Entity()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "todelete@example.com",
            DisplayName = "To Delete",
            LastActiveAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        // Act
        Context.Users.Remove(user);
        await Context.SaveChangesAsync();

        // Assert
        var deletedUser = await Context.Users.FindAsync(user.Id);
        Assert.Null(deletedUser);
    }

    [Fact]
    public async Task UUIDv7_Provides_Temporal_Ordering()
    {
        // Arrange & Act
        var id1 = Guid.CreateVersion7();
        await Task.Delay(10); // Small delay to ensure different timestamps
        var id2 = Guid.CreateVersion7();
        await Task.Delay(10);
        var id3 = Guid.CreateVersion7();

        // Create users with these IDs
        var user1 = new User { Id = id1, Email = "1@test.com", DisplayName = "First", LastActiveAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow };
        var user2 = new User { Id = id2, Email = "2@test.com", DisplayName = "Second", LastActiveAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow };
        var user3 = new User { Id = id3, Email = "3@test.com", DisplayName = "Third", LastActiveAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow };

        Context.Users.AddRange(user1, user2, user3);
        await Context.SaveChangesAsync();

        // Assert - when ordered by ID, should be in creation order
        // Filter out seeded data to only test our created users
        var orderedUsers = await Context.Users
            .Where(u => u.Email.EndsWith("@test.com"))
            .OrderBy(u => u.Id)
            .Select(u => u.DisplayName)
            .ToListAsync();

        Assert.Equal(["First", "Second", "Third"], orderedUsers);
    }
}