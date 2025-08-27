using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Text.Json;
using Veritheia.Core.Interfaces;
using Veritheia.Core.ValueObjects;
using Veritheia.ApiService.Processes;
using Veritheia.ApiService.Services;
using Veritheia.Data;
using Veritheia.Data.Entities;
using Veritheia.Data.Services;
using Veritheia.Tests.Helpers;
using veritheia.Tests.TestBase;

namespace Veritheia.Tests.Integration.Processes;

/// <summary>
/// Integration tests for SystematicScreeningProcess using mocked LLM
/// Tests the new corpus-based implementation
/// </summary>
[Trait("Category", "Integration")]
public class SystematicScreeningProcessTests : DatabaseTestBase
{
    private readonly Mock<ICognitiveAdapter> _mockCognitiveAdapter;
    private readonly Mock<ILogger<SystematicScreeningProcess>> _mockLogger;
    private readonly SystematicScreeningProcess _process;
    private readonly IServiceProvider _serviceProvider;

    public SystematicScreeningProcessTests(DatabaseFixture fixture) : base(fixture)
    {
        _mockCognitiveAdapter = new Mock<ICognitiveAdapter>();
        _mockLogger = new Mock<ILogger<SystematicScreeningProcess>>();

        // Create a real ServiceCollection for proper dependency injection
        var services = new ServiceCollection();
        
        // Add database context
        services.AddScoped<VeritheiaDbContext>(_ => Context);
        
        // Add ApiService services
        services.AddScoped<DocumentService>();
        services.AddScoped<DocumentMetadataService>();
        services.AddScoped<SemanticExtractionService>();
        
        // Add Data layer services (utilities)
        services.AddScoped<CsvWriterService>();
        
        // Add mocked services
        services.AddScoped<ICognitiveAdapter>(_ => _mockCognitiveAdapter.Object);
        services.AddScoped<IDocumentStorageRepository>(_ => Mock.Of<IDocumentStorageRepository>());
        
        // Add logging
        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();
        _process = new SystematicScreeningProcess(_mockLogger.Object, _serviceProvider);
    }

    [Fact]
    public void GetInputDefinition_ShouldReturnCorrectInputs()
    {
        // Act
        var inputDefinition = _process.GetInputDefinition();

        // Assert
        Assert.NotNull(inputDefinition);
        Assert.Contains(inputDefinition.Fields, f => f.Name == "research_questions");
        Assert.Contains(inputDefinition.Fields, f => f.Name == "document_ids");
        Assert.Contains(inputDefinition.Fields, f => f.Name == "relevance_threshold");
        Assert.Contains(inputDefinition.Fields, f => f.Name == "contribution_threshold");
    }

    [Fact]
    public void ValidateInputs_WithValidInputs_ShouldReturnTrue()
    {
        // Arrange
        var documentIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var context = new ProcessContext
        {
            ExecutionId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            JourneyId = Guid.NewGuid(),
            Inputs = new Dictionary<string, object>
            {
                ["research_questions"] = "How are LLMs being used in cybersecurity?",
                ["document_ids"] = JsonSerializer.Serialize(documentIds)
            }
        };

        // Act
        var result = _process.ValidateInputs(context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateInputs_WithMissingResearchQuestions_ShouldReturnFalse()
    {
        // Arrange
        var documentIds = new List<Guid> { Guid.NewGuid() };
        var context = new ProcessContext
        {
            ExecutionId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            JourneyId = Guid.NewGuid(),
            Inputs = new Dictionary<string, object>
            {
                ["document_ids"] = JsonSerializer.Serialize(documentIds)
            }
        };

        // Act
        var result = _process.ValidateInputs(context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateInputs_WithMissingDocumentIds_ShouldReturnFalse()
    {
        // Arrange
        var context = new ProcessContext
        {
            ExecutionId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            JourneyId = Guid.NewGuid(),
            Inputs = new Dictionary<string, object>
            {
                ["research_questions"] = "Test research question"
            }
        };

        // Act
        var result = _process.ValidateInputs(context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExecuteAsync_WithCorpusDocuments_ShouldProcessSuccessfully()
    {
        // Arrange - Create test user and documents in corpus
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            DisplayName = "Test User"
        };
        Context.Users.Add(user);

        // Create test documents with metadata
        var documentIds = new List<Guid>();
        for (int i = 0; i < 3; i++)
        {
            var docId = Guid.NewGuid();
            documentIds.Add(docId);

            var document = new Document
            {
                Id = docId,
                UserId = userId,
                FileName = $"paper_{i}.pdf",
                MimeType = "application/pdf",
                FilePath = $"test/paper_{i}.pdf",
                FileSize = 1000,
                UploadedAt = DateTime.UtcNow
            };
            Context.Documents.Add(document);

            var metadata = new DocumentMetadata
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DocumentId = docId,
                Title = $"Test Paper {i}: AI in Cybersecurity",
                Authors = new[] { $"Author {i}" },
                Abstract = $"This paper explores the use of large language models in cybersecurity applications. " +
                          $"We investigate threat detection using AI systems. Paper number {i}.",
                Keywords = new[] { "AI", "cybersecurity", "LLM" },
                PublicationDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Publisher = "Test Conference"
            };
            Context.DocumentMetadata.Add(metadata);
        }

        await Context.SaveChangesAsync();

        // Create process context
        var inputs = new Dictionary<string, object>
        {
            ["research_questions"] = "How are LLMs being used in cybersecurity?\nWhat are the challenges?",
            ["document_ids"] = JsonSerializer.Serialize(documentIds),
            ["relevance_threshold"] = 0.7f,
            ["contribution_threshold"] = 0.7f
        };

        var context = new ProcessContext
        {
            ExecutionId = Guid.NewGuid(),
            UserId = userId,
            JourneyId = Guid.NewGuid(),
            Inputs = inputs
        };

        // Mock LLM responses
        // 3 documents × (1 semantic extraction + 2 RQs × 2 assessments) = 15 calls
        // Order: For EACH document: semantic, then 4 assessments (2 RQs × 2 types)
        var mockResponses = new List<string>();

        for (int doc = 0; doc < 3; doc++)
        {
            // Semantic extraction response for this document (JSON)
            mockResponses.Add(@"{
                ""topics"": [""cybersecurity"", ""threat detection"", ""AI systems""],
                ""entities"": [""LLM"", ""neural networks"", ""threat detection""],
                ""keywords"": [""security"", ""artificial intelligence"", ""machine learning""]
            }");
            
            // Assessment responses for this document (2 RQs × 2 assessments = 4)
            for (int i = 0; i < 4; i++)
            {
                mockResponses.Add("Score: 0.8\nReasoning: This paper directly addresses the research question about LLMs in cybersecurity.");
            }
        }

        var setupSequence = _mockCognitiveAdapter.SetupSequence(x => x.GenerateTextAsync(It.IsAny<string>(), It.IsAny<string?>()));
        foreach (var response in mockResponses)
        {
            setupSequence = setupSequence.ReturnsAsync(response);
        }

        // Act
        var result = await _process.ExecuteAsync(context, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success, $"Process failed: {result.ErrorMessage ?? "Unknown error"}");
        Assert.NotNull(result.Data);
        
        // Verify summary data
        Assert.Contains("total_documents", result.Data.Keys);
        Assert.Contains("processed_documents", result.Data.Keys);
        Assert.Contains("must_read_count", result.Data.Keys);
        Assert.Contains("research_questions", result.Data.Keys);
        Assert.Contains("csv_output", result.Data.Keys);
        
        Assert.Equal(3, Convert.ToInt32(result.Data["total_documents"]));
        Assert.Equal(3, Convert.ToInt32(result.Data["processed_documents"]));
        
        // Verify research questions were parsed correctly
        var resultRQs = result.Data["research_questions"] as List<string>;
        Assert.NotNull(resultRQs);
        Assert.Equal(2, resultRQs.Count);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingAbstracts_ShouldSkipDocuments()
    {
        // Arrange - Create documents without abstracts
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test2@example.com",
            DisplayName = "Test User 2"
        };
        Context.Users.Add(user);

        var documentIds = new List<Guid>();
        
        // Document with metadata
        var docId1 = Guid.NewGuid();
        documentIds.Add(docId1);
        Context.Documents.Add(new Document
        {
            Id = docId1,
            UserId = userId,
            FileName = "with_abstract.pdf",
            MimeType = "application/pdf",
            FilePath = "test/with_abstract.pdf",
            FileSize = 1000,
            UploadedAt = DateTime.UtcNow
        });
        Context.DocumentMetadata.Add(new DocumentMetadata
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DocumentId = docId1,
            Title = "Paper with Abstract",
            Abstract = "This paper has an abstract about AI.",
            Authors = new[] { "Author A" }
        });

        // Document without abstract
        var docId2 = Guid.NewGuid();
        documentIds.Add(docId2);
        Context.Documents.Add(new Document
        {
            Id = docId2,
            UserId = userId,
            FileName = "no_abstract.pdf",
            MimeType = "application/pdf",
            FilePath = "test/no_abstract.pdf",
            FileSize = 1000,
            UploadedAt = DateTime.UtcNow
        });
        Context.DocumentMetadata.Add(new DocumentMetadata
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DocumentId = docId2,
            Title = "Paper without Abstract",
            Abstract = null, // No abstract
            Authors = new[] { "Author B" }
        });

        await Context.SaveChangesAsync();

        // Create process context
        var inputs = new Dictionary<string, object>
        {
            ["research_questions"] = "Test question",
            ["document_ids"] = JsonSerializer.Serialize(documentIds)
        };

        var context = new ProcessContext
        {
            ExecutionId = Guid.NewGuid(),
            UserId = userId,
            JourneyId = Guid.NewGuid(),
            Inputs = inputs
        };

        // Mock LLM responses - only 1 document should be processed
        // 1 semantic + 1 RQ × 2 assessments = 3 calls
        _mockCognitiveAdapter.SetupSequence(x => x.GenerateTextAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync(@"{""topics"": [""AI""], ""entities"": [""test""], ""keywords"": [""test""]}")
            .ReturnsAsync("Score: 0.5\nReasoning: Test")
            .ReturnsAsync("Score: 0.5\nReasoning: Test");

        // Act
        var result = await _process.ExecuteAsync(context, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, Convert.ToInt32(result.Data["total_documents"]));
        Assert.Equal(1, Convert.ToInt32(result.Data["processed_documents"])); // Only 1 with abstract
    }

    [Fact]
    public async Task ExecuteAsync_WithThresholds_ShouldDetermineMustRead()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test3@example.com",
            DisplayName = "Test User 3"
        };
        Context.Users.Add(user);

        var docId = Guid.NewGuid();
        Context.Documents.Add(new Document
        {
            Id = docId,
            UserId = userId,
            FileName = "test.pdf",
            MimeType = "application/pdf",
            FilePath = "test/test.pdf",
            FileSize = 1000,
            UploadedAt = DateTime.UtcNow
        });
        Context.DocumentMetadata.Add(new DocumentMetadata
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DocumentId = docId,
            Title = "Test Paper",
            Abstract = "Test abstract",
            Authors = new[] { "Author" }
        });

        await Context.SaveChangesAsync();

        var inputs = new Dictionary<string, object>
        {
            ["research_questions"] = "Test question",
            ["document_ids"] = JsonSerializer.Serialize(new List<Guid> { docId }),
            ["relevance_threshold"] = 0.7f,
            ["contribution_threshold"] = 0.7f
        };

        var context = new ProcessContext
        {
            ExecutionId = Guid.NewGuid(),
            UserId = userId,
            JourneyId = Guid.NewGuid(),
            Inputs = inputs
        };

        // Mock responses with scores above threshold
        _mockCognitiveAdapter.SetupSequence(x => x.GenerateTextAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync(@"{""topics"": [""test""], ""entities"": [""test""], ""keywords"": [""test""]}")
            .ReturnsAsync("Score: 0.8\nReasoning: High relevance") // Above threshold
            .ReturnsAsync("Score: 0.9\nReasoning: Strong contribution"); // Above threshold

        // Act
        var result = await _process.ExecuteAsync(context, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, Convert.ToInt32(result.Data["must_read_count"])); // Should be must-read
        
        var results = result.Data["results"] as List<Dictionary<string, object>>;
        Assert.NotNull(results);
        Assert.Single(results);
    }
}