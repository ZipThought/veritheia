using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Veritheia.Core.Interfaces;
using Veritheia.Data.Services;
using Xunit;
using Moq;

namespace Veritheia.Tests.Phase6_PlatformServices;

/// <summary>
/// Tests for Phase 6 Platform Services
/// Validates text extraction, embedding generation, and document ingestion
/// </summary>
[Trait("Category", "Integration")]
public class PlatformServicesTests
{
    [Fact]
    public async Task TextExtractionService_ExtractsPlainText()
    {
        // Arrange
        var logger = new Mock<ILogger<TextExtractionService>>();
        var service = new TextExtractionService(logger.Object);
        
        var content = "This is a test document.\nIt has multiple lines.\nAnd some content.";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        
        // Act
        var extracted = await service.ExtractTextAsync(stream, "text/plain");
        
        // Assert
        Assert.Equal(content, extracted);
    }
    
    [Fact]
    public async Task TextExtractionService_ExtractsMarkdown()
    {
        // Arrange
        var logger = new Mock<ILogger<TextExtractionService>>();
        var service = new TextExtractionService(logger.Object);
        
        var content = "# Heading\n\nThis is **bold** and this is *italic*.";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        
        // Act
        var extracted = await service.ExtractTextAsync(stream, "text/markdown");
        
        // Assert
        Assert.Equal(content, extracted);
    }
    
    [Fact]
    public async Task TextExtractionService_ExtractsCsv()
    {
        // Arrange
        var logger = new Mock<ILogger<TextExtractionService>>();
        var service = new TextExtractionService(logger.Object);
        
        var content = "Name,Age,City\nAlice,30,New York\nBob,25,San Francisco";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        
        // Act
        var extracted = await service.ExtractTextAsync(stream, "text/csv");
        
        // Assert
        Assert.Equal(content, extracted);
    }
    
    [Fact]
    public async Task TextExtractionService_RejectsUnsupportedFormat()
    {
        // Arrange
        var logger = new Mock<ILogger<TextExtractionService>>();
        var service = new TextExtractionService(logger.Object);
        
        using var stream = new MemoryStream();
        
        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(async () =>
            await service.ExtractTextAsync(stream, "application/octet-stream"));
    }
    
    [Fact]
    public async Task EmbeddingService_GeneratesEmbeddings()
    {
        // Arrange
        var mockAdapter = new Mock<ICognitiveAdapter>();
        mockAdapter.Setup(a => a.CreateEmbedding(It.IsAny<string>()))
            .ReturnsAsync(new float[384]);
        
        var logger = new Mock<ILogger<EmbeddingService>>();
        var service = new EmbeddingService(mockAdapter.Object, logger.Object);
        
        var chunks = new[]
        {
            new DocumentChunk { Content = "First chunk" },
            new DocumentChunk { Content = "Second chunk" },
            new DocumentChunk { Content = "Third chunk" }
        };
        
        // Act
        var result = await service.GenerateEmbeddingsAsync(chunks);
        
        // Assert
        Assert.Equal(3, result.Count);
        foreach (var chunk in result)
        {
            Assert.NotNull(chunk.Embedding);
            Assert.Equal(384, chunk.Embedding.Length);
        }
        
        mockAdapter.Verify(a => a.CreateEmbedding(It.IsAny<string>()), Times.Exactly(3));
    }
    
    [Fact]
    public async Task EmbeddingService_SkipsWhenNoAdapter()
    {
        // Arrange
        var logger = new Mock<ILogger<EmbeddingService>>();
        var service = new EmbeddingService(null, logger.Object);
        
        var chunks = new[]
        {
            new DocumentChunk { Content = "Test chunk" }
        };
        
        // Act
        var result = await service.GenerateEmbeddingsAsync(chunks);
        
        // Assert
        Assert.Single(result);
        Assert.Null(result[0].Embedding);
    }
    
    [Fact]
    public async Task DocumentIngestionService_ProcessesDocument()
    {
        // Arrange
        var mockStorage = new Mock<IDocumentStorageRepository>();
        mockStorage.Setup(s => s.StoreDocumentAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync("/test/path/document.txt");
        
        var mockExtraction = new Mock<TextExtractionService>(Mock.Of<ILogger<TextExtractionService>>());
        mockExtraction.Setup(e => e.ExtractTextAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync("Extracted text content");
        
        var mockEmbedding = new Mock<EmbeddingService>(
            Mock.Of<ICognitiveAdapter>(), 
            Mock.Of<ILogger<EmbeddingService>>());
        mockEmbedding.Setup(e => e.GenerateEmbeddingsAsync(It.IsAny<DocumentChunk[]>()))
            .ReturnsAsync((DocumentChunk[] chunks) => chunks);
        
        var logger = new Mock<ILogger<DocumentIngestionService>>();
        
        var service = new DocumentIngestionService(
            mockStorage.Object,
            mockExtraction.Object,
            mockEmbedding.Object,
            logger.Object);
        
        var content = "Test document content";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        
        // Act
        var result = await service.IngestDocumentAsync(
            stream,
            "test.txt",
            "text/plain",
            new Dictionary<string, object> { ["author"] = "Test Author" });
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("test.txt", result.OriginalName);
        Assert.Equal("text/plain", result.MimeType);
        Assert.Equal("/test/path/document.txt", result.FilePath);
        Assert.NotNull(result.FileHash);
        Assert.Contains("author", result.Metadata.Keys);
        
        mockStorage.Verify(s => s.StoreDocumentAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Once);
        mockExtraction.Verify(e => e.ExtractTextAsync(It.IsAny<Stream>(), "text/plain"), Times.Once);
    }
    
    [Fact]
    public void DocumentIngestionService_ChunksLongText()
    {
        // Arrange
        var logger = new Mock<ILogger<DocumentIngestionService>>();
        var service = new DocumentIngestionService(
            Mock.Of<IDocumentStorageRepository>(),
            Mock.Of<TextExtractionService>(),
            Mock.Of<EmbeddingService>(),
            logger.Object);
        
        // Create text longer than chunk size (1000 chars)
        var longText = new StringBuilder();
        for (int i = 0; i < 300; i++)
        {
            longText.AppendLine($"This is line {i} of the document with some content.");
        }
        
        // Act
        var chunks = service.ChunkText(longText.ToString(), 1000);
        
        // Assert
        Assert.True(chunks.Length > 1, "Should create multiple chunks");
        
        foreach (var chunk in chunks)
        {
            Assert.True(chunk.Content.Length <= 1200, "Chunk should not exceed max size with overlap");
        }
        
        // Verify chunks have proper metadata
        for (int i = 0; i < chunks.Length; i++)
        {
            Assert.Equal(i, chunks[i].SequenceNumber);
            Assert.NotEmpty(chunks[i].Content);
        }
    }
    
    [Fact]
    public void DocumentIngestionService_PreservesShortText()
    {
        // Arrange
        var logger = new Mock<ILogger<DocumentIngestionService>>();
        var service = new DocumentIngestionService(
            Mock.Of<IDocumentStorageRepository>(),
            Mock.Of<TextExtractionService>(),
            Mock.Of<EmbeddingService>(),
            logger.Object);
        
        var shortText = "This is a short document.";
        
        // Act
        var chunks = service.ChunkText(shortText, 1000);
        
        // Assert
        Assert.Single(chunks);
        Assert.Equal(shortText, chunks[0].Content);
        Assert.Equal(0, chunks[0].SequenceNumber);
    }
    
    [Fact]
    public async Task FileStorageService_StoresAndRetrievesDocument()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), "veritheia_test_" + Guid.NewGuid());
        Directory.CreateDirectory(tempPath);
        
        try
        {
            var service = new FileStorageService(tempPath);
            
            var content = "Test file content";
            using var writeStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            
            // Act - Store
            var storedPath = await service.StoreDocumentAsync(writeStream, "test.txt");
            
            // Assert - Store
            Assert.NotNull(storedPath);
            Assert.True(File.Exists(Path.Combine(tempPath, storedPath)));
            
            // Act - Retrieve
            using var readStream = await service.RetrieveDocumentAsync(storedPath);
            using var reader = new StreamReader(readStream);
            var retrieved = await reader.ReadToEndAsync();
            
            // Assert - Retrieve
            Assert.Equal(content, retrieved);
            
            // Act - Delete
            await service.DeleteDocumentAsync(storedPath);
            
            // Assert - Delete
            Assert.False(File.Exists(Path.Combine(tempPath, storedPath)));
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }
    
    [Fact]
    public async Task FileStorageService_HandlesNonExistentFile()
    {
        // Arrange
        var tempPath = Path.GetTempPath();
        var service = new FileStorageService(tempPath);
        
        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(async () =>
            await service.RetrieveDocumentAsync("nonexistent.txt"));
    }
}

// Test helper class
public class DocumentChunk
{
    public string Content { get; set; } = string.Empty;
    public int SequenceNumber { get; set; }
    public float[]? Embedding { get; set; }
}