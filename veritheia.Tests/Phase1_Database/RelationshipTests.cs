using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Veritheia.Data.Entities;
using veritheia.Tests.TestBase;
using Xunit;

namespace veritheia.Tests.Phase1_Database;

public class RelationshipTests : DatabaseTestBase
{
    public RelationshipTests(DatabaseFixture fixture) : base(fixture) { }
    
    [Fact]
    public async Task Can_Create_Journey_With_Journals()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "researcher@test.com",
            DisplayName = "Researcher",
            LastActiveAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        var journey = new Journey
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            Purpose = "Literature review on AI safety",
            State = "Active",
            CreatedAt = DateTime.UtcNow
        };
        
        var researchJournal = new Journal
        {
            Id = Guid.CreateVersion7(),
            JourneyId = journey.Id,
            Type = "Research",
            CreatedAt = DateTime.UtcNow
        };
        
        var methodJournal = new Journal
        {
            Id = Guid.CreateVersion7(),
            JourneyId = journey.Id,
            Type = "Method",
            CreatedAt = DateTime.UtcNow
        };
        
        // Act
        Context.Users.Add(user);
        Context.Journeys.Add(journey);
        Context.Journals.AddRange(researchJournal, methodJournal);
        await Context.SaveChangesAsync();
        
        // Assert
        var savedJourney = await Context.Journeys
            .Include(j => j.Journals)
            .FirstOrDefaultAsync(j => j.Id == journey.Id);
            
        Assert.NotNull(savedJourney);
        Assert.Equal(2, savedJourney.Journals.Count);
        Assert.Contains(savedJourney.Journals, j => j.Type == "Research");
        Assert.Contains(savedJourney.Journals, j => j.Type == "Method");
    }
    
    [Fact]
    public async Task Can_Create_Document_With_Journey_Segments()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "user@test.com",
            DisplayName = "User",
            LastActiveAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        var journey = new Journey
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            Purpose = "Research journey",
            State = "Active",
            CreatedAt = DateTime.UtcNow
        };
        
        var document = new Document
        {
            Id = Guid.CreateVersion7(),
            FileName = "test-paper.pdf",
            FilePath = "/documents/test-paper.pdf",
            MimeType = "application/pdf",
            FileSize = 1024000,
            UploadedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        var segment1 = new JourneyDocumentSegment
        {
            Id = Guid.CreateVersion7(),
            JourneyId = journey.Id,
            DocumentId = document.Id,
            SegmentContent = "This is the abstract of the paper...",
            SegmentType = "abstract",
            SequenceIndex = 0,
            CreatedAt = DateTime.UtcNow
        };
        
        var segment2 = new JourneyDocumentSegment
        {
            Id = Guid.CreateVersion7(),
            JourneyId = journey.Id,
            DocumentId = document.Id,
            SegmentContent = "This is the methodology section...",
            SegmentType = "methodology",
            SequenceIndex = 1,
            CreatedAt = DateTime.UtcNow
        };
        
        // Act
        Context.Users.Add(user);
        Context.Journeys.Add(journey);
        Context.Documents.Add(document);
        Context.JourneyDocumentSegments.AddRange(segment1, segment2);
        await Context.SaveChangesAsync();
        
        // Assert
        var segments = await Context.JourneyDocumentSegments
            .Where(s => s.JourneyId == journey.Id)
            .OrderBy(s => s.SequenceIndex)
            .ToListAsync();
            
        Assert.Equal(2, segments.Count);
        Assert.Equal("abstract", segments[0].SegmentType);
        Assert.Equal("methodology", segments[1].SegmentType);
    }
    
    [Fact]
    public async Task Can_Create_Persona_For_User()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "multi-persona@test.com",
            DisplayName = "Multi Persona User",
            LastActiveAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        var researcherPersona = new Persona
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            Domain = "Researcher",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var studentPersona = new Persona
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            Domain = "Student",
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };
        
        // Act
        Context.Users.Add(user);
        Context.Personas.AddRange(researcherPersona, studentPersona);
        await Context.SaveChangesAsync();
        
        // Assert
        var personas = await Context.Personas
            .Where(p => p.UserId == user.Id)
            .ToListAsync();
            
        Assert.Equal(2, personas.Count);
        Assert.Contains(personas, p => p.Domain == "Researcher" && p.IsActive);
        Assert.Contains(personas, p => p.Domain == "Student" && !p.IsActive);
    }
    
    [Fact]
    public async Task Cascade_Delete_Works_Correctly()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "cascade@test.com",
            DisplayName = "Cascade Test",
            LastActiveAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        var journey = new Journey
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            Purpose = "Test cascade",
            State = "Active",
            CreatedAt = DateTime.UtcNow
        };
        
        var journal = new Journal
        {
            Id = Guid.CreateVersion7(),
            JourneyId = journey.Id,
            Type = "Research",
            CreatedAt = DateTime.UtcNow
        };
        
        Context.Users.Add(user);
        Context.Journeys.Add(journey);
        Context.Journals.Add(journal);
        await Context.SaveChangesAsync();
        
        // Act - Delete the journey
        Context.Journeys.Remove(journey);
        await Context.SaveChangesAsync();
        
        // Assert - Journal should be deleted due to cascade
        var journalExists = await Context.Journals.AnyAsync(j => j.Id == journal.Id);
        Assert.False(journalExists);
        
        // User should still exist
        var userExists = await Context.Users.AnyAsync(u => u.Id == user.Id);
        Assert.True(userExists);
    }
}