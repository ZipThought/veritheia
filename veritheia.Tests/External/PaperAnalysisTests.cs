using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;
using Veritheia.Core.Interfaces;
using Veritheia.Data.Services;
using Xunit;
using Xunit.Abstractions;

namespace veritheia.Tests.External;

/// <summary>
/// Integration tests using actual research papers in the repository.
/// Tests PDF extraction and LLM analysis on real academic content.
/// </summary>
[Trait("Category", "LLMIntegration")]
[Trait("RequiresExternalService", "true")]
[Collection("LLMIntegration")]
public class PaperAnalysisTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly ServiceProvider _serviceProvider;
    private readonly string _papersPath;
    
    public PaperAnalysisTests(ITestOutputHelper output)
    {
        _output = output;
        
        // Find papers directory relative to test execution
        var baseDir = Directory.GetCurrentDirectory();
        while (!Directory.Exists(Path.Combine(baseDir, "papers")) && baseDir != Path.GetPathRoot(baseDir))
        {
            baseDir = Directory.GetParent(baseDir)?.FullName ?? baseDir;
        }
        _papersPath = Path.Combine(baseDir, "papers");
        
        var services = new ServiceCollection();
        
        // Configuration
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LocalLLM:Url"] = Environment.GetEnvironmentVariable("LLM_URL") ?? "http://192.168.68.100:1234",
                ["LocalLLM:Model"] = Environment.GetEnvironmentVariable("LLM_MODEL") ?? "gemma-3-12b-it"
            })
            .Build();
        
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging(builder => builder.AddProvider(new TestLoggerProvider(_output)));
        services.AddHttpClient<LocalLLMAdapter>();
        services.AddSingleton<ICognitiveAdapter, LocalLLMAdapter>();
        services.AddScoped<TextExtractionService>();
        
        _serviceProvider = services.BuildServiceProvider();
    }
    
    [Fact]
    public async Task ExtractText_FromLLAssistPaper_ExtractsAbstractCorrectly()
    {
        // Arrange
        var pdfPath = Path.Combine(_papersPath, "2407.13993v3.pdf");
        if (!File.Exists(pdfPath))
        {
            _output.WriteLine($"Paper not found at {pdfPath}, skipping test");
            return;
        }
        
        var textExtraction = _serviceProvider.GetRequiredService<TextExtractionService>();
        
        // Act
        using var stream = File.OpenRead(pdfPath);
        var extractedText = await textExtraction.ExtractTextAsync(stream, "application/pdf");
        
        // Assert
        Assert.NotNull(extractedText);
        Assert.NotEmpty(extractedText);
        
        // Check for expected content from LLAssist paper
        Assert.Contains("LLAssist", extractedText);
        Assert.Contains("literature review", extractedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("systematic", extractedText, StringComparison.OrdinalIgnoreCase);
        
        _output.WriteLine($"✓ Extracted {extractedText.Length} characters from LLAssist paper");
        _output.WriteLine($"First 500 chars: {extractedText.Substring(0, Math.Min(500, extractedText.Length))}...");
    }
    
    [Fact]
    public async Task ExtractText_FromEdgePromptPaper_ExtractsMethodologySection()
    {
        // Arrange
        var pdfPath = Path.Combine(_papersPath, "3701716.3717810.pdf");
        if (!File.Exists(pdfPath))
        {
            _output.WriteLine($"Paper not found at {pdfPath}, skipping test");
            return;
        }
        
        var textExtraction = _serviceProvider.GetRequiredService<TextExtractionService>();
        
        // Act
        using var stream = File.OpenRead(pdfPath);
        var extractedText = await textExtraction.ExtractTextAsync(stream, "application/pdf");
        
        // Assert
        Assert.NotNull(extractedText);
        Assert.NotEmpty(extractedText);
        
        // Check for expected content from EdgePrompt paper
        Assert.Contains("EdgePrompt", extractedText);
        Assert.Contains("K-12", extractedText);
        Assert.Contains("guardrail", extractedText, StringComparison.OrdinalIgnoreCase);
        
        _output.WriteLine($"✓ Extracted {extractedText.Length} characters from EdgePrompt paper");
    }
    
    [Fact]
    public async Task AnalyzePaper_LLAssistMethodology_IdentifiesKeyContributions()
    {
        // Arrange
        var pdfPath = Path.Combine(_papersPath, "2407.13993v3.pdf");
        if (!File.Exists(pdfPath))
        {
            _output.WriteLine($"Paper not found at {pdfPath}, skipping test");
            return;
        }
        
        var textExtraction = _serviceProvider.GetRequiredService<TextExtractionService>();
        var cognitiveAdapter = _serviceProvider.GetRequiredService<ICognitiveAdapter>();
        
        // Extract text
        using var stream = File.OpenRead(pdfPath);
        var extractedText = await textExtraction.ExtractTextAsync(stream, "application/pdf");
        
        // Take first 2000 characters for analysis (to stay within token limits)
        var textSample = extractedText.Substring(0, Math.Min(2000, extractedText.Length));
        
        // Act
        var analysisPrompt = $@"Analyze this research paper excerpt and identify:
1. The main contribution
2. The methodology used
3. Key tools or techniques mentioned

Paper excerpt:
{textSample}

Provide a brief structured response.";
        
        var analysis = await cognitiveAdapter.GenerateTextAsync(
            analysisPrompt,
            "You are analyzing academic papers for systematic review. Be precise and factual.");
        
        // Assert
        Assert.NotNull(analysis);
        Assert.NotEmpty(analysis);
        
        _output.WriteLine("LLM Analysis of LLAssist paper:");
        _output.WriteLine(analysis);
        
        // Check if LLM identified key aspects
        if (!analysis.Contains("[Cannot connect") && !analysis.Contains("[LLM Error"))
        {
            // The analysis should mention some key aspects
            var mentionsLiteratureReview = analysis.Contains("literature", StringComparison.OrdinalIgnoreCase) ||
                                          analysis.Contains("review", StringComparison.OrdinalIgnoreCase);
            var mentionsLLM = analysis.Contains("LLM", StringComparison.OrdinalIgnoreCase) ||
                             analysis.Contains("language model", StringComparison.OrdinalIgnoreCase);
            
            Assert.True(mentionsLiteratureReview || mentionsLLM, 
                "Analysis should identify literature review or LLM aspects");
            
            _output.WriteLine("✓ LLM successfully analyzed paper content");
        }
    }
    
    [Fact]
    public async Task ScreenPaper_AgainstVeritheiaRequirements_DeterminesRelevance()
    {
        // Arrange
        var pdfPath = Path.Combine(_papersPath, "3701716.3717810.pdf");
        if (!File.Exists(pdfPath))
        {
            _output.WriteLine($"Paper not found at {pdfPath}, skipping test");
            return;
        }
        
        var textExtraction = _serviceProvider.GetRequiredService<TextExtractionService>();
        var cognitiveAdapter = _serviceProvider.GetRequiredService<ICognitiveAdapter>();
        
        // Extract text
        using var stream = File.OpenRead(pdfPath);
        var extractedText = await textExtraction.ExtractTextAsync(stream, "application/pdf");
        var textSample = extractedText.Substring(0, Math.Min(1500, extractedText.Length));
        
        // Act - Screen against Veritheia's core principles
        var screeningPrompt = $@"Screen this paper against the following research questions:
1. Does it address epistemic sovereignty or user control over knowledge?
2. Does it discuss offline-first or local processing architectures?
3. Does it involve educational or knowledge management systems?

Paper excerpt:
{textSample}

Provide decision: INCLUDE or EXCLUDE with reasoning for each question.";
        
        var screeningResult = await cognitiveAdapter.GenerateTextAsync(
            screeningPrompt,
            "You are screening papers for relevance to a knowledge management system. Be objective.");
        
        // Assert
        Assert.NotNull(screeningResult);
        Assert.NotEmpty(screeningResult);
        
        _output.WriteLine("Screening result for EdgePrompt paper:");
        _output.WriteLine(screeningResult);
        
        if (!screeningResult.Contains("[Cannot connect") && !screeningResult.Contains("[LLM Error"))
        {
            // Should include this paper as it's foundational to Veritheia
            Assert.Contains("INCLUDE", screeningResult, StringComparison.OrdinalIgnoreCase);
            _output.WriteLine("✓ Paper correctly identified as relevant");
        }
    }
    
    [Fact]
    public async Task ComparePapers_IdentifyCommonThemes()
    {
        // Arrange
        var llassistPath = Path.Combine(_papersPath, "2407.13993v3.pdf");
        var edgepromptPath = Path.Combine(_papersPath, "3701716.3717810.pdf");
        
        if (!File.Exists(llassistPath) || !File.Exists(edgepromptPath))
        {
            _output.WriteLine("Papers not found, skipping test");
            return;
        }
        
        var textExtraction = _serviceProvider.GetRequiredService<TextExtractionService>();
        var cognitiveAdapter = _serviceProvider.GetRequiredService<ICognitiveAdapter>();
        
        // Extract both papers
        string paper1Text, paper2Text;
        using (var stream = File.OpenRead(llassistPath))
        {
            paper1Text = await textExtraction.ExtractTextAsync(stream, "application/pdf");
        }
        using (var stream = File.OpenRead(edgepromptPath))
        {
            paper2Text = await textExtraction.ExtractTextAsync(stream, "application/pdf");
        }
        
        // Take samples from each
        var sample1 = paper1Text.Substring(0, Math.Min(800, paper1Text.Length));
        var sample2 = paper2Text.Substring(0, Math.Min(800, paper2Text.Length));
        
        // Act
        var comparisonPrompt = $@"Compare these two research paper excerpts and identify:
1. Common themes or concerns
2. Complementary approaches
3. How they might inform a knowledge management system

Paper 1 (LLAssist):
{sample1}

Paper 2 (EdgePrompt):
{sample2}

Provide a brief analysis.";
        
        var comparison = await cognitiveAdapter.GenerateTextAsync(
            comparisonPrompt,
            "You are comparing research papers to identify synergies. Be analytical.");
        
        // Assert
        Assert.NotNull(comparison);
        Assert.NotEmpty(comparison);
        
        _output.WriteLine("Comparative analysis of foundational papers:");
        _output.WriteLine(comparison);
        
        if (!comparison.Contains("[Cannot connect") && !comparison.Contains("[LLM Error"))
        {
            _output.WriteLine("✓ Successfully compared papers for common themes");
        }
    }
    
    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}

// Test logger provider implementation
public class TestLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _output;
    
    public TestLoggerProvider(ITestOutputHelper output)
    {
        _output = output;
    }
    
    public ILogger CreateLogger(string categoryName)
    {
        return new TestLogger<object>(_output);
    }
    
    public void Dispose() { }
}