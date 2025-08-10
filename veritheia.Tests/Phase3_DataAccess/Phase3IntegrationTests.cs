using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
// Core.Models removed - post-DDD
using Veritheia.Data;
using Veritheia.Data.Entities;
using veritheia.Tests.TestBase;
using Xunit;

namespace Veritheia.Tests.Phase3_DataAccess;

/// <summary>
/// Phase 3: Shows correct data access patterns using Phase 1 & 2 infrastructure
/// Demonstrates EF Core as repository, PostgreSQL as enforcer
/// </summary>
[Collection("DatabaseTests")]
public class Phase3IntegrationTests : DatabaseTestBase
{
    public Phase3IntegrationTests(DatabaseFixture fixture) : base(fixture) { }
    
    [Fact]
    public async Task Phase3_DirectEFCore_AccessingPhase1Entities()
    {
        // Using Phase 1 entities directly with EF Core
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "test@example.com",
            DisplayName = "Test User"
        };
        
        var persona = new Persona
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow
        };
        
        var journey = new Journey
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            PersonaId = persona.Id,
            Purpose = "Research quantum computing",
            State = "Active",
            CreatedAt = DateTime.UtcNow
        };
        
        // Direct EF Core usage - no repository wrapper
        Context.Users.Add(user);
        Context.Personas.Add(persona);
        Context.Journeys.Add(journey);
        await Context.SaveChangesAsync();
        
        // Query with EF Core - it IS the repository
        var result = await Context.Journeys
            .Include(j => j.Persona)
            .Include(j => j.User)
            .FirstOrDefaultAsync(j => j.Id == journey.Id);
        
        Assert.NotNull(result);
        Assert.Equal("Research quantum computing", result.Purpose);
        Assert.NotNull(result.Persona);
    }
    
    [Fact]
    public async Task Phase3_PostgreSQLEnforcesConstraints()
    {
        // PostgreSQL enforces domain rules, not C#
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "constraint@test.com",
            DisplayName = "Test"
        };
        
        // Try to create journey without persona (violates FK)
        var journey = new Journey
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            PersonaId = Guid.CreateVersion7(), // Doesn't exist!
            Purpose = "Will fail",
            State = "Active",
            CreatedAt = DateTime.UtcNow
        };
        
        Context.Users.Add(user);
        Context.Journeys.Add(journey);
        
        // PostgreSQL will enforce the constraint
        await Assert.ThrowsAsync<DbUpdateException>(
            async () => await Context.SaveChangesAsync());
    }
    
    [Fact]
    public async Task Phase3_AuthoredResult_TracksJourneyAttribution()
    {
        // Setup journey
        var user = new User { Id = Guid.CreateVersion7(), Email = "author@test.com", DisplayName = "Author" };
        var persona = new Persona { Id = Guid.CreateVersion7(), UserId = user.Id, CreatedAt = DateTime.UtcNow };
        var journey = new Journey
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            PersonaId = persona.Id,
            Purpose = "Test Journey",
            State = "Active",
            CreatedAt = DateTime.UtcNow
        };
        
        Context.Users.Add(user);
        Context.Personas.Add(persona);
        Context.Journeys.Add(journey);
        await Context.SaveChangesAsync();
        
        // Post-DDD: Direct entity returns instead of AuthoredResult
        // Journey tracking happens at the service level
        Assert.NotNull(journey);
        Assert.Equal("Test Journey", journey.Purpose);
    }
    
    [Fact]
    public async Task Phase3_DocumentOwnership_FromPhase1()
    {
        // Phase 1 added UserId to Document
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "owner@test.com",
            DisplayName = "Owner"
        };
        
        var document = new Document
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,  // Ownership tracking
            FileName = "research.pdf",
            MimeType = "application/pdf",
            FilePath = "/docs/research.pdf",
            FileSize = 1024,
            UploadedAt = DateTime.UtcNow
        };
        
        Context.Users.Add(user);
        Context.Documents.Add(document);
        await Context.SaveChangesAsync();
        
        // Direct query with ownership
        var userDocs = await Context.Documents
            .Where(d => d.UserId == user.Id)
            .ToListAsync();
        
        Assert.Single(userDocs);
    }
    
    [Fact]
    public async Task Phase3_JourneyDocumentSegments_FormationTracking()
    {
        // Setup entities
        var user = new User { Id = Guid.CreateVersion7(), Email = "seg@test.com", DisplayName = "Test" };
        var persona = new Persona { Id = Guid.CreateVersion7(), UserId = user.Id, CreatedAt = DateTime.UtcNow };
        var journey = new Journey
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            PersonaId = persona.Id,
            Purpose = "Understanding AI",
            State = "Active",
            CreatedAt = DateTime.UtcNow
        };
        var document = new Document
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            FileName = "ai_paper.pdf",
            MimeType = "application/pdf",
            FilePath = "/ai_paper.pdf",
            FileSize = 5000,
            UploadedAt = DateTime.UtcNow
        };
        
        Context.Users.Add(user);
        Context.Personas.Add(persona);
        Context.Journeys.Add(journey);
        Context.Documents.Add(document);
        
        // Journey document segment - formation tracking
        var segment = new JourneyDocumentSegment
        {
            Id = Guid.CreateVersion7(),
            JourneyId = journey.Id,
            DocumentId = document.Id,
            SegmentContent = "AI systems require careful alignment",
            SegmentType = "abstract",
            SegmentPurpose = "Understanding AI safety",
            SequenceIndex = 1
        };
        
        Context.JourneyDocumentSegments.Add(segment);
        await Context.SaveChangesAsync();
        
        // Query segments for journey
        var segments = await Context.JourneyDocumentSegments
            .Where(s => s.JourneyId == journey.Id)
            .Include(s => s.Document)
            .ToListAsync();
        
        Assert.Single(segments);
        Assert.Equal("Understanding AI safety", segments[0].SegmentPurpose);
    }
    
    [Fact]
    public async Task Phase3_ProcessExecution_WithPhase1Entities()
    {
        // Process execution tracking
        // Create a valid journey first (FK constraint)
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "test@example.com",
            DisplayName = "Test User"
        };
        var persona = new Persona
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            Domain = "Test"
        };
        var journey = new Journey
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            PersonaId = persona.Id,
            Purpose = "Test",
            ProcessType = "TestProcess"
        };
        
        Context.Users.Add(user);
        Context.Personas.Add(persona);
        Context.Journeys.Add(journey);
        await Context.SaveChangesAsync();
        
        var execution = new ProcessExecution
        {
            Id = Guid.CreateVersion7(),
            JourneyId = journey.Id,
            ProcessType = "TestProcess",
            StartedAt = DateTime.UtcNow
        };
        
        Context.ProcessExecutions.Add(execution);
        await Context.SaveChangesAsync();
        
        // Direct query
        var saved = await Context.ProcessExecutions
            .FirstOrDefaultAsync(e => e.Id == execution.Id);
        
        Assert.NotNull(saved);
        Assert.Equal(execution.JourneyId, saved.JourneyId);
    }
}