using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Veritheia.Data.Entities;
using veritheia.Tests.TestBase;
using Xunit;

namespace veritheia.Tests.Integration.Database;

public class VectorStorageTests : DatabaseTestBase
{
    public VectorStorageTests(DatabaseFixture fixture) : base(fixture) { }
    
    [Fact]
    public async Task Can_Store_SearchIndex_With_Vector1536()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "vector@test.com",
            DisplayName = "Vector Test",
            LastActiveAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        var persona = new Persona
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            Domain = "Tester",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var journey = new Journey
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            PersonaId = persona.Id,
            Purpose = "Vector test journey",
            State = "Active",
            CreatedAt = DateTime.UtcNow
        };
        
        var document = new Document
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,  // Document ownership tracking
            FileName = "test.pdf",
            FilePath = "/test.pdf",
            MimeType = "application/pdf",
            FileSize = 1000,
            UploadedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        var segment = new JourneyDocumentSegment
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,  // Required for composite primary key
            JourneyId = journey.Id,
            DocumentId = document.Id,
            SegmentContent = "Test content for embedding",
            SegmentType = "paragraph",
            SequenceIndex = 0,
            CreatedAt = DateTime.UtcNow
        };
        
        var searchIndex = new SearchIndex
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            SegmentId = segment.Id,
            VectorModel = "openai-ada-002",
            CreatedAt = DateTime.UtcNow
        };
        
        // Create a test embedding (1536 dimensions)
        var embedding = new float[1536];
        for (int i = 0; i < 1536; i++)
        {
            embedding[i] = (float)(Math.Sin(i) * 0.1);
        }
        
        var vector1536 = new SearchVector1536
        {
            UserId = user.Id,  // Required for composite primary key
            IndexId = searchIndex.Id,
            Embedding = new Vector(embedding)
        };
        
        // Act
        Context.Users.Add(user);
        Context.Personas.Add(persona);
        Context.Journeys.Add(journey);
        Context.Documents.Add(document);
        Context.JourneyDocumentSegments.Add(segment);
        Context.SearchIndexes.Add(searchIndex);
        Context.SearchVectors1536.Add(vector1536);
        await Context.SaveChangesAsync();
        
        // Assert
        var savedIndex = await Context.SearchIndexes
            .FirstOrDefaultAsync(si => si.Id == searchIndex.Id);
        Assert.NotNull(savedIndex);
        Assert.Equal("openai-ada-002", savedIndex.VectorModel);
        
        var savedVector = await Context.SearchVectors1536
            .FirstOrDefaultAsync(v => v.IndexId == searchIndex.Id);
        Assert.NotNull(savedVector);
        Assert.NotNull(savedVector.Embedding);
        Assert.Equal(1536, savedVector.Embedding.ToArray().Length);
    }
    
    [Fact]
    public async Task Can_Store_SearchVector768()
    {
        // Arrange - Setup entities
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "e5@test.com",
            DisplayName = "E5 Test",
            LastActiveAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        var persona = new Persona
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            Domain = "E5 Tester",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var journey = new Journey
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            PersonaId = persona.Id,
            Purpose = "E5 model test",
            State = "Active",
            CreatedAt = DateTime.UtcNow
        };
        
        var document = new Document
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,  // Document ownership tracking
            FileName = "e5-test.pdf",
            FilePath = "/e5-test.pdf",
            MimeType = "application/pdf",
            FileSize = 2000,
            UploadedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        var segment = new JourneyDocumentSegment
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,  // Required for composite primary key
            JourneyId = journey.Id,
            DocumentId = document.Id,
            SegmentContent = "Content for E5 embedding",
            SegmentType = "section",
            SequenceIndex = 0,
            CreatedAt = DateTime.UtcNow
        };
        
        var searchIndex = new SearchIndex
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,  // Required for composite primary key
            SegmentId = segment.Id,
            VectorModel = "e5-large-v2",
            CreatedAt = DateTime.UtcNow
        };
        
        // Create 768-dimensional embedding
        var embedding768 = new float[768];
        for (int i = 0; i < 768; i++)
        {
            embedding768[i] = (float)(Math.Cos(i) * 0.1);
        }
        
        var vector768 = new SearchVector768
        {
            UserId = user.Id,  // Required for composite primary key
            IndexId = searchIndex.Id,
            Embedding = new Vector(embedding768)
        };
        
        // Act
        Context.Users.Add(user);
        Context.Personas.Add(persona);
        Context.Journeys.Add(journey);
        Context.Documents.Add(document);
        Context.JourneyDocumentSegments.Add(segment);
        Context.SearchIndexes.Add(searchIndex);
        Context.SearchVectors768.Add(vector768);
        await Context.SaveChangesAsync();
        
        // Assert
        var savedVector = await Context.SearchVectors768
            .FirstOrDefaultAsync(v => v.IndexId == searchIndex.Id);
        Assert.NotNull(savedVector);
        Assert.Equal(768, savedVector.Embedding.ToArray().Length);
    }
    
    [Fact]
    public async Task Can_Store_SearchVector384()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "mini@test.com",
            DisplayName = "Mini Model Test",
            LastActiveAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        var persona = new Persona
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            Domain = "Mini Tester",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var journey = new Journey
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            PersonaId = persona.Id,
            Purpose = "Lightweight model test",
            State = "Active",
            CreatedAt = DateTime.UtcNow
        };
        
        var document = new Document
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,  // Document ownership tracking
            FileName = "mini.pdf",
            FilePath = "/mini.pdf",
            MimeType = "application/pdf",
            FileSize = 500,
            UploadedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        var segment = new JourneyDocumentSegment
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,  // Required for composite primary key
            JourneyId = journey.Id,
            DocumentId = document.Id,
            SegmentContent = "Lightweight content",
            SegmentType = "sentence",
            SequenceIndex = 0,
            CreatedAt = DateTime.UtcNow
        };
        
        var searchIndex = new SearchIndex
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,  // Required for composite primary key
            SegmentId = segment.Id,
            VectorModel = "all-MiniLM-L6-v2",
            CreatedAt = DateTime.UtcNow
        };
        
        // Create 384-dimensional embedding
        var embedding384 = new float[384];
        for (int i = 0; i < 384; i++)
        {
            embedding384[i] = (float)(Math.Tan(i * 0.01) * 0.1);
        }
        
        var vector384 = new SearchVector384
        {
            UserId = user.Id,  // Required for composite primary key
            IndexId = searchIndex.Id,
            Embedding = new Vector(embedding384)
        };
        
        // Act
        Context.Users.Add(user);
        Context.Personas.Add(persona);
        Context.Journeys.Add(journey);
        Context.Documents.Add(document);
        Context.JourneyDocumentSegments.Add(segment);
        Context.SearchIndexes.Add(searchIndex);
        Context.SearchVectors384.Add(vector384);
        await Context.SaveChangesAsync();
        
        // Assert
        var savedVector = await Context.SearchVectors384
            .FirstOrDefaultAsync(v => v.IndexId == searchIndex.Id);
        Assert.NotNull(savedVector);
        Assert.Equal(384, savedVector.Embedding.ToArray().Length);
    }
    
    [Fact]
    public async Task SearchIndex_Enforces_Unique_Segment_Model_Constraint()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "unique@test.com",
            DisplayName = "Unique Test",
            LastActiveAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        var persona = new Persona
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            Domain = "Constraint Tester",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var journey = new Journey
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            PersonaId = persona.Id,
            Purpose = "Unique constraint test",
            State = "Active",
            CreatedAt = DateTime.UtcNow
        };
        
        var document = new Document
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,  // Document ownership tracking
            FileName = "unique.pdf",
            FilePath = "/unique.pdf",
            MimeType = "application/pdf",
            FileSize = 1000,
            UploadedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        var segment = new JourneyDocumentSegment
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,  // Required for composite primary key
            JourneyId = journey.Id,
            DocumentId = document.Id,
            SegmentContent = "Test content",
            SegmentType = "paragraph",
            SequenceIndex = 0,
            CreatedAt = DateTime.UtcNow
        };
        
        var index1 = new SearchIndex
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            SegmentId = segment.Id,
            VectorModel = "openai-ada-002",
            CreatedAt = DateTime.UtcNow
        };
        
        var index2 = new SearchIndex
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            SegmentId = segment.Id,  // Same segment
            VectorModel = "openai-ada-002",  // Same model - should violate unique constraint
            CreatedAt = DateTime.UtcNow
        };
        
        // Act & Assert
        Context.Users.Add(user);
        Context.Personas.Add(persona);
        Context.Journeys.Add(journey);
        Context.Documents.Add(document);
        Context.JourneyDocumentSegments.Add(segment);
        Context.SearchIndexes.Add(index1);
        await Context.SaveChangesAsync();
        
        Context.SearchIndexes.Add(index2);
        await Assert.ThrowsAsync<DbUpdateException>(async () => 
            await Context.SaveChangesAsync());
    }
}