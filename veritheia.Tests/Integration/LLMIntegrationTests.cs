using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Veritheia.Data.Services;
using Xunit;
using Xunit.Abstractions;

namespace Veritheia.Tests.Integration;

/// <summary>
/// Integration tests for LLM and embedding services.
/// These tests require a local LLM server to be running.
/// 
/// To run these tests locally:
/// 1. Start LM Studio or similar server at http://192.168.68.100:1234
/// 2. Load models: gemma-3-12b-it and nomic-embed-text
/// 3. Run: dotnet test --filter "Category=LLMIntegration"
/// 
/// These tests are excluded from CI/CD pipelines.
/// </summary>
[Trait("Category", "LLMIntegration")]
[Trait("RequiresExternalService", "true")]
[Collection("LLMIntegration")]
public class LLMIntegrationTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly LocalLLMAdapter _adapter;
    
    public LLMIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        
        // Build configuration
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["LocalLLM:Url"] = Environment.GetEnvironmentVariable("LLM_URL") ?? "http://192.168.68.100:1234",
            ["LocalLLM:Model"] = Environment.GetEnvironmentVariable("LLM_MODEL") ?? "gemma-3-12b-it"
        });
        _configuration = configBuilder.Build();
        
        var logger = new TestLogger<LocalLLMAdapter>(_output);
        _adapter = new LocalLLMAdapter(_httpClient, _configuration, logger);
    }
    
    [Fact]
    public async Task GenerateText_SimplePrompt_ReturnsValidResponse()
    {
        // Arrange
        var prompt = "What is 2+2? Answer with just the number.";
        var systemPrompt = "You are a helpful math assistant.";
        
        // Act
        _output.WriteLine($"Testing text generation with prompt: {prompt}");
        var result = await _adapter.GenerateTextAsync(prompt, systemPrompt);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        _output.WriteLine($"Response: {result}");
        
        // Check if it's an error or valid response
        if (!result.Contains("[Cannot connect") && !result.Contains("[LLM Error"))
        {
            Assert.Contains("4", result);
            _output.WriteLine("✓ Text generation successful");
        }
        else
        {
            _output.WriteLine("⚠ LLM server not available - test skipped");
        }
    }
    
    [Fact]
    public async Task CreateEmbedding_ValidText_ReturnsVectorOfCorrectDimension()
    {
        // Arrange
        var text = "This is a test sentence for embedding generation.";
        
        // Act
        _output.WriteLine($"Testing embedding generation for: {text}");
        var result = await _adapter.CreateEmbedding(text);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        // Check dimensions (common embedding sizes)
        Assert.True(
            result.Length == 768 || result.Length == 1536 || result.Length == 384,
            $"Unexpected embedding dimension: {result.Length}");
        
        // Check value range
        Assert.All(result, v => Assert.InRange(v, -2.0f, 2.0f));
        
        _output.WriteLine($"✓ Embedding generated with dimension: {result.Length}");
        _output.WriteLine($"  Sample values: [{result[0]:F4}, {result[1]:F4}, {result[2]:F4}, ...]");
    }
    
    [Fact]
    public async Task GenerateText_DocumentScreening_ProducesValidDecision()
    {
        // Arrange
        var prompt = @"Screen this document against the following research question:
'What are the effects of climate change on agriculture?'

Document: Research paper about drought impacts on crop yields in temperate regions.

Answer with INCLUDE or EXCLUDE and a brief reason.";
        
        var systemPrompt = "You are a research assistant performing systematic literature screening.";
        
        // Act
        _output.WriteLine("Testing document screening capability...");
        var result = await _adapter.GenerateTextAsync(prompt, systemPrompt);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        _output.WriteLine($"Screening result:\n{result}");
        
        if (!result.Contains("[Cannot connect") && !result.Contains("[LLM Error"))
        {
            var hasDecision = result.Contains("INCLUDE", StringComparison.OrdinalIgnoreCase) ||
                             result.Contains("EXCLUDE", StringComparison.OrdinalIgnoreCase);
            Assert.True(hasDecision, "Response should contain INCLUDE or EXCLUDE decision");
            _output.WriteLine("✓ Document screening successful");
        }
    }
    
    [Fact]
    public async Task GenerateText_ConstrainedComposition_GeneratesStructuredContent()
    {
        // Arrange
        var prompt = @"Generate a brief research abstract (100-150 words) about sustainable agriculture.
Include: problem statement, methodology, key findings, and implications.
Use formal academic language.";
        
        var systemPrompt = "You are an academic writer. Follow the structure and constraints precisely.";
        
        // Act
        _output.WriteLine("Testing constrained composition...");
        var result = await _adapter.GenerateTextAsync(prompt, systemPrompt);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        _output.WriteLine($"Generated abstract:\n{result}");
        
        if (!result.Contains("[Cannot connect") && !result.Contains("[LLM Error"))
        {
            // Check for minimum length (rough approximation)
            var wordCount = result.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            Assert.InRange(wordCount, 50, 500); // Generous range for variability
            _output.WriteLine($"✓ Constrained composition successful ({wordCount} words)");
        }
    }
    
    [Theory]
    [InlineData("What is the capital of France?", "Paris")]
    [InlineData("What is 10 * 10?", "100")]
    [InlineData("Complete: The sky is", "blue")]
    public async Task GenerateText_VariousPrompts_ReturnsExpectedContent(string prompt, string expectedContent)
    {
        // Act
        var result = await _adapter.GenerateTextAsync(prompt, null);
        
        // Assert
        Assert.NotNull(result);
        
        if (!result.Contains("[Cannot connect") && !result.Contains("[LLM Error"))
        {
            Assert.Contains(expectedContent, result, StringComparison.OrdinalIgnoreCase);
            _output.WriteLine($"✓ Prompt '{prompt}' -> Contains '{expectedContent}'");
        }
    }
    
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

// Test logger implementation
public class TestLogger<T> : ILogger<T>
{
    private readonly ITestOutputHelper _output;
    
    public TestLogger(ITestOutputHelper output)
    {
        _output = output;
    }
    
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _output.WriteLine($"[{logLevel}] {formatter(state, exception)}");
        if (exception != null)
        {
            _output.WriteLine($"Exception: {exception}");
        }
    }
}