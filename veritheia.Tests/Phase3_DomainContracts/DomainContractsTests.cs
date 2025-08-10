using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Veritheia.Core.Interfaces;
// Core.Models removed - post-DDD
using Veritheia.Data;
using Veritheia.Data.Entities;
using veritheia.Tests.TestBase;
using Xunit;

namespace Veritheia.Tests.Phase3_DomainContracts;

/// <summary>
/// Tests for Phase 3: Domain-shaped contracts and AuthoredResult pattern
/// Tests that we're using EF Core directly, not wrapping in repositories
/// </summary>
[Collection("DatabaseTests")]
public class DomainContractsTests : DatabaseTestBase
{
    public DomainContractsTests(DatabaseFixture fixture) : base(fixture) { }
    
    protected VeritheiaDbContext CreateDbContext() => Fixture.CreateContext();
    // [Fact]
    // AuthoredResult removed in post-DDD refactoring
    // public async Task AuthoredResult_TracksJourneyAttribution()
    // {
    //     // AuthoredResult and FormationNote were removed as over-ceremonial
    //     // Direct entity returns are used instead
    // }
    
    [Fact]
    public async Task DirectEFCore_NoRepositoryWrapper_Works()
    {
        // This test proves we're using EF Core directly, not through repositories
        await using var context = CreateDbContext();
        
        // Create user directly with EF Core
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "direct.ef@example.com",
            DisplayName = "Direct EF User"
        };
        
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        // Query directly with EF Core - no repository abstraction
        var savedUser = await context.Users
            .FirstOrDefaultAsync(u => u.Email == "direct.ef@example.com");
        
        // Assert
        Assert.NotNull(savedUser);
        Assert.Equal("Direct EF User", savedUser.DisplayName);
        
        // This is the correct pattern - EF Core IS the repository
        // No need for IUserRepository wrapping DbSet<User>
    }
    
    [Fact]
    public async Task PostgreSQL_EnforcesConstraints_NotCSharp()
    {
        // This test demonstrates PostgreSQL is the semantic enforcer
        await using var context = CreateDbContext();
        
        // Create user and persona (required by FK constraint)
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "constraint.test@example.com",
            DisplayName = "Constraint Test"
        };
        
        var persona = new Persona
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Users.Add(user);
        context.Personas.Add(persona);
        await context.SaveChangesAsync();
        
        // Try to create journey without valid PersonaId
        var journey = new Journey
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            PersonaId = Guid.CreateVersion7(), // Invalid - doesn't exist
            Purpose = "Test Journey",
            State = "Active",
            CreatedAt = DateTime.UtcNow
        };
        
        context.Journeys.Add(journey);
        
        // PostgreSQL will enforce the FK constraint
        var exception = await Assert.ThrowsAsync<DbUpdateException>(
            async () => await context.SaveChangesAsync());
        
        // The constraint is enforced by PostgreSQL, not C# code
        Assert.Contains("violates foreign key constraint", exception.InnerException?.Message ?? "");
    }
    
    [Fact]
    public async Task DocumentStorage_ExternalResource_NeedsRepository()
    {
        // This demonstrates when we DO need a repository - external resources
        // We're mocking file storage which PostgreSQL cannot handle
        
        var mockStorage = new MockDocumentStorage();
        var content = "Document content"u8.ToArray();
        
        // Act - Store document (external to PostgreSQL)
        var path = await mockStorage.StoreDocumentAsync(
            new MemoryStream(content),
            "test.pdf",
            "application/pdf");
        
        // Assert
        Assert.NotEmpty(path);
        Assert.True(await mockStorage.ExistsAsync(path));
        
        // This IS a valid repository - it abstracts external storage
        // Not PostgreSQL data which EF Core already handles
    }
    
    // Mock implementation for testing
    private class MockDocumentStorage : IDocumentStorageRepository
    {
        private readonly Dictionary<string, byte[]> _storage = new();
        
        public Task<string> StoreDocumentAsync(Stream content, string fileName, string mimeType, System.Threading.CancellationToken cancellationToken = default)
        {
            var path = $"mock://{Guid.NewGuid()}/{fileName}";
            using var ms = new MemoryStream();
            content.CopyTo(ms);
            _storage[path] = ms.ToArray();
            return Task.FromResult(path);
        }
        
        public Task<Stream> GetDocumentContentAsync(string storagePath, System.Threading.CancellationToken cancellationToken = default)
        {
            if (_storage.TryGetValue(storagePath, out var content))
                return Task.FromResult<Stream>(new MemoryStream(content));
            throw new FileNotFoundException();
        }
        
        public Task<bool> ExistsAsync(string storagePath, System.Threading.CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_storage.ContainsKey(storagePath));
        }
        
        public Task DeleteDocumentAsync(string storagePath, System.Threading.CancellationToken cancellationToken = default)
        {
            _storage.Remove(storagePath);
            return Task.CompletedTask;
        }
        
        public Task<DocumentStorageMetadata> GetMetadataAsync(string storagePath, System.Threading.CancellationToken cancellationToken = default)
        {
            if (_storage.TryGetValue(storagePath, out var content))
            {
                return Task.FromResult(new DocumentStorageMetadata(
                    content.Length,
                    DateTime.UtcNow,
                    DateTime.UtcNow,
                    "mock-hash"));
            }
            throw new FileNotFoundException();
        }
    }
}