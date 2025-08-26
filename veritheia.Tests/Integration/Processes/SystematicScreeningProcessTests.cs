using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Veritheia.Core.Interfaces;
using Veritheia.Core.ValueObjects;
using Veritheia.Data;
using Veritheia.Data.Entities;
using Veritheia.Data.Processes;
using Veritheia.Data.Services;
using Veritheia.Tests.Helpers;
using veritheia.Tests.TestBase;
using System.IO;

namespace Veritheia.Tests.Integration.Processes;

/// <summary>
/// Integration tests for SystematicScreeningProcess using REAL DATABASE with Testcontainers
/// These tests use PostgreSQL with pgvector to test actual formation through authorship
/// </summary>
[Collection("DatabaseTests")]
[Trait("Category", "Integration")]
public class SystematicScreeningProcessTests : DatabaseTestBase
{
    private readonly Mock<ICognitiveAdapter> _mockCognitiveAdapter;
    private readonly Mock<ILogger<BasicSystematicScreeningProcess>> _mockLogger;
    private readonly BasicSystematicScreeningProcess _process;
    private readonly IServiceProvider _serviceProvider;

    public SystematicScreeningProcessTests(DatabaseFixture fixture) : base(fixture)
    {
        _mockCognitiveAdapter = new Mock<ICognitiveAdapter>();
        _mockLogger = new Mock<ILogger<BasicSystematicScreeningProcess>>();

        // Create a real ServiceCollection with REAL DATABASE from Testcontainers
        var services = new ServiceCollection();
        
        // Use the REAL database context from fixture
        services.AddScoped<VeritheiaDbContext>(_ => Context);
        
        // Add real services
        services.AddScoped<ICognitiveAdapter>(_ => _mockCognitiveAdapter.Object);
        services.AddScoped<CsvParserService>(sp => new CsvParserService(Mock.Of<ILogger<CsvParserService>>()));
        services.AddScoped<CsvWriterService>(sp => new CsvWriterService(Mock.Of<ILogger<CsvWriterService>>()));
        services.AddScoped<SemanticExtractionService>(sp => new SemanticExtractionService(_mockCognitiveAdapter.Object, Mock.Of<ILogger<SemanticExtractionService>>()));
        
        // Real DocumentService with real storage
        services.AddScoped<IDocumentStorageRepository, FileStorageService>(sp => 
            new FileStorageService(Path.Combine(Path.GetTempPath(), "veritheia_test_storage")));
        services.AddScoped<DocumentService>();

        _serviceProvider = services.BuildServiceProvider();
        _process = new BasicSystematicScreeningProcess(_mockLogger.Object, _serviceProvider);
    }

    [Fact]
    public void GetInputDefinition_ShouldReturnCorrectInputs()
    {
        // Act
        var inputDefinition = _process.GetInputDefinition();

        // Assert
        Assert.NotNull(inputDefinition);
        Assert.Contains(inputDefinition.Fields, f => f.Name == "research_questions");
        Assert.Contains(inputDefinition.Fields, f => f.Name == "csv_upload");
        Assert.Contains(inputDefinition.Fields, f => f.Name == "intellectual_framework");
    }

    [Fact]
    public void ValidateInputs_WithValidInputs_ShouldReturnTrue()
    {
        // Arrange
        var context = new ProcessContext
        {
            ExecutionId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            JourneyId = Guid.NewGuid(),
            Inputs = new Dictionary<string, object>
            {
                ["intellectual_framework"] = "My research focuses on AI in cybersecurity",
                ["research_questions"] = TestDataHelper.GetResearchQuestionsText("cybersecurity_llm_rqs.txt")
            },
            Services = null // Not used in validation
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
        var context = new ProcessContext
        {
            ExecutionId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            JourneyId = Guid.NewGuid(),
            Inputs = new Dictionary<string, object>
            {
                ["csv_content"] = TestDataHelper.GetCsvSample("ieee_sample.csv")
            },
            Services = null // Not used in validation
        };

        // Act
        var result = _process.ValidateInputs(context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateInputs_WithMissingBothFrameworkAndQuestions_ShouldReturnFalse()
    {
        // Arrange - missing both intellectual_framework AND research_questions
        var context = new ProcessContext
        {
            ExecutionId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            JourneyId = Guid.NewGuid(),
            Inputs = new Dictionary<string, object>
            {
                ["csv_upload"] = "some_csv_content"  // Only has CSV, no framework
            },
            Services = null // Not used in validation
        };

        // Act
        var result = _process.ValidateInputs(context);

        // Assert - Should be false since needs either framework OR questions
        Assert.False(result);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidInputs_ShouldProcessArticles()
    {
        // Arrange
        var researchQuestions = TestDataHelper.GetResearchQuestionsText("single_rq.txt");
        var csvContent = TestDataHelper.GetCsvSample("ieee_sample.csv");

        var inputs = new Dictionary<string, object>
        {
            ["intellectual_framework"] = "Testing formation through authorship in CI environment",
            ["research_questions"] = researchQuestions,
            ["csv_upload"] = csvContent
        };

        var context = new ProcessContext
        {
            ExecutionId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            JourneyId = Guid.NewGuid(),
            Inputs = inputs,
            Services = null // Not used since we pass the real service provider to the process constructor
        };

        // Mock LLM responses for 5 articles × 1 RQ × (1 semantic + 2 assessments) = 15 total calls
        var mockResponses = new List<string>();

        // Add semantic extraction responses (5 articles)
        for (int i = 0; i < 5; i++)
        {
            mockResponses.Add(@"{""topics"": [""cybersecurity"", ""AI""], ""entities"": [""LLM""], ""keywords"": [""security""]}");
        }

        // Add assessment responses (5 articles × 1 RQ × 2 assessments = 10 responses)
        for (int i = 0; i < 10; i++)
        {
            mockResponses.Add("Score: 0.7\nReasoning: Test assessment response for CI.");
        }

        var setupSequence = _mockCognitiveAdapter.SetupSequence(x => x.GenerateTextAsync(It.IsAny<string>(), It.IsAny<string>()));
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
        Assert.Contains("total_articles", result.Data.Keys);
        Assert.Contains("must_read_count", result.Data.Keys);
        Assert.Contains("research_questions", result.Data.Keys);
        Assert.Contains("csv_output", result.Data.Keys);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleResearchQuestions_ShouldProcessAll()
    {
        // Arrange
        var researchQuestions = TestDataHelper.GetResearchQuestionsText("cybersecurity_llm_rqs.txt");
        var csvContent = TestDataHelper.GetCsvSample("scopus_sample.csv");

        var inputs = new Dictionary<string, object>
        {
            ["intellectual_framework"] = "Testing formation through authorship in CI environment",
            ["research_questions"] = researchQuestions,
            ["csv_upload"] = csvContent
        };

        var context = new ProcessContext
        {
            ExecutionId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            JourneyId = Guid.NewGuid(),
            Inputs = inputs,
            Services = null // Not used since we pass the real service provider to the process constructor
        };

        // Mock LLM responses - simplified approach for CI testing
        // We need: 6 articles × (1 semantic + 4 RQs × 2 assessments each) = 6 + 48 = 54 responses
        var mockResponses = new List<string>();

        // Add semantic extraction responses (6 articles)
        for (int i = 0; i < 6; i++)
        {
            mockResponses.Add(@"{""topics"": [""cybersecurity"", ""AI""], ""entities"": [""LLM""], ""keywords"": [""security""]}");
        }

        // Add assessment responses (6 articles × 4 RQs × 2 assessments = 48 responses)
        for (int i = 0; i < 48; i++)
        {
            mockResponses.Add("Score: 0.7\nReasoning: Test assessment response for CI.");
        }

        var setupSequence = _mockCognitiveAdapter.SetupSequence(x => x.GenerateTextAsync(It.IsAny<string>(), It.IsAny<string>()));
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

        // Verify that all 4 research questions were processed
        var researchQuestionsArray = TestDataHelper.GetResearchQuestions("cybersecurity_llm_rqs.txt");
        var resultResearchQuestions = (List<string>)result.Data["research_questions"];
        Assert.Equal(researchQuestionsArray.Length, resultResearchQuestions.Count);

        // Verify articles were processed (6 in scopus sample)
        Assert.Equal(6, (int)result.Data["total_articles"]);

        // Verify CSV output was generated
        Assert.Contains("csv_output", result.Data.Keys);
        var csvOutput = result.Data["csv_output"] as string;
        Assert.NotNull(csvOutput);
        Assert.NotEmpty(csvOutput);
    }
}
