using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Veritheia.Core.Interfaces;
using Veritheia.Core.ValueObjects;
using Veritheia.Data.Processes;
using Veritheia.Data.Services;
using Veritheia.Tests.Helpers;

namespace Veritheia.Tests.Integration.Processes;

/// <summary>
/// Integration tests for SystematicScreeningProcess using mocked LLM
/// These tests run in CI and use mocks to avoid external dependencies
/// </summary>
[Trait("Category", "Integration")]
public class SystematicScreeningProcessTests
{
    private readonly Mock<ICognitiveAdapter> _mockCognitiveAdapter;
    private readonly Mock<ILogger<BasicSystematicScreeningProcess>> _mockLogger;
    private readonly BasicSystematicScreeningProcess _process;

    public SystematicScreeningProcessTests()
    {
        _mockCognitiveAdapter = new Mock<ICognitiveAdapter>();
        _mockLogger = new Mock<ILogger<BasicSystematicScreeningProcess>>();

        // Create a real ServiceCollection for proper dependency injection
        var services = new ServiceCollection();
        services.AddScoped<ICognitiveAdapter>(_ => _mockCognitiveAdapter.Object);
        services.AddScoped<CsvParserService>(sp => new CsvParserService(Mock.Of<ILogger<CsvParserService>>()));
        services.AddScoped<CsvWriterService>(sp => new CsvWriterService(Mock.Of<ILogger<CsvWriterService>>()));
        services.AddScoped<SemanticExtractionService>(sp => new SemanticExtractionService(_mockCognitiveAdapter.Object, Mock.Of<ILogger<SemanticExtractionService>>()));

        var serviceProvider = services.BuildServiceProvider();

        _process = new BasicSystematicScreeningProcess(_mockLogger.Object, serviceProvider);
    }

    [Fact]
    public void GetInputDefinition_ShouldReturnCorrectInputs()
    {
        // Act
        var inputDefinition = _process.GetInputDefinition();

        // Assert
        Assert.NotNull(inputDefinition);
        Assert.Contains(inputDefinition.Fields, f => f.Name == "research_questions");
        Assert.Contains(inputDefinition.Fields, f => f.Name == "csv_content");
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
                ["research_questions"] = TestDataHelper.GetResearchQuestionsText("cybersecurity_llm_rqs.txt"),
                ["csv_content"] = TestDataHelper.GetCsvSample("ieee_sample.csv")
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
    public void ValidateInputs_WithMissingCsvContent_ShouldReturnFalse()
    {
        // Arrange
        var context = new ProcessContext
        {
            ExecutionId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            JourneyId = Guid.NewGuid(),
            Inputs = new Dictionary<string, object>
            {
                ["research_questions"] = TestDataHelper.GetResearchQuestionsText("single_rq.txt")
            },
            Services = null // Not used in validation
        };

        // Act
        var result = _process.ValidateInputs(context);

        // Assert
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
            ["research_questions"] = researchQuestions,
            ["csv_content"] = csvContent
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
            ["research_questions"] = researchQuestions,
            ["csv_content"] = csvContent
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
