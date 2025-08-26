using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Veritheia.Core.Interfaces;
using Veritheia.Core.ValueObjects;
using Veritheia.Data;
using Veritheia.Data.Entities;
using Veritheia.Data.Processes;
using Veritheia.Data.Services;
using veritheia.Tests.TestBase;
using Veritheia.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace veritheia.Tests.Integration.E2E;

/// <summary>
/// End-to-end integration test for the complete LLAssist workflow with REAL LLM.
/// Uses actual Scopus/IEEE CSV test data to validate real-world compatibility.
/// 
/// This test is EXCLUDED from CI because it requires a real LLM service.
/// To run locally: dotnet test --filter "FullyQualifiedName~LLAssistRealLLMTest"
/// 
/// Prerequisites:
/// - Local LLM running (e.g., LM Studio on port 1234)
/// - Or set environment variables: LLM_URL and LLM_MODEL
/// </summary>
[Collection("DatabaseTests")]
[Trait("Category", "LLMIntegration")]
[Trait("RequiresLLM", "true")]
public class LLAssistRealLLMTest : DatabaseTestBase
{
    private readonly ITestOutputHelper _output;
    private readonly ServiceProvider _serviceProvider;
    
    public LLAssistRealLLMTest(DatabaseFixture fixture, ITestOutputHelper output) : base(fixture)
    {
        _output = output;
        
        // Build service provider with all required services
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder => 
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        
        // Use the context from the fixture
        services.AddScoped<VeritheiaDbContext>(_ => Context);
        
        // Add services using the actual registration - REAL LLM, not mocked
        services.AddHttpClient<OpenAICognitiveAdapter>();
        services.AddScoped<ICognitiveAdapter, OpenAICognitiveAdapter>();
        services.AddScoped<SemanticExtractionService>();
        services.AddScoped<CsvParserService>();
        services.AddScoped<CsvWriterService>();
        services.AddScoped<IAnalyticalProcess, BasicSystematicScreeningProcess>();
        services.AddScoped<ProcessEngine>();
        services.AddScoped<ProcessWorkerService>();
        services.AddScoped<JourneyService>();
        services.AddScoped<PersonaService>();
        services.AddScoped<UserService>();
        services.AddScoped<DocumentService>();
        services.AddScoped<FileStorageService>();
        services.AddScoped<IDocumentStorageRepository, FileStorageService>();
        services.AddScoped<TextExtractionService>();
        services.AddScoped<EmbeddingService>();
        services.AddScoped<DocumentIngestionService>();
        
        // Add configuration for LLM
        services.AddSingleton<IConfiguration>(provider =>
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LLM:Url"] = Environment.GetEnvironmentVariable("LLM_URL") ?? "http://localhost:1234/v1",
                ["LLM:Model"] = Environment.GetEnvironmentVariable("LLM_MODEL") ?? "llama-3.2-3b-instruct"
            });
            return configBuilder.Build();
        });
        
        _serviceProvider = services.BuildServiceProvider();
    }
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _output.WriteLine("Database ready from fixture");
        
        // Register processes with the ProcessEngine
        var processEngine = _serviceProvider.GetRequiredService<ProcessEngine>();
        processEngine.RegisterProcess<BasicSystematicScreeningProcess>();
        _output.WriteLine("Processes registered with ProcessEngine");
    }
    
    public override async Task DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();
        await base.DisposeAsync();
    }
    
    [Fact(Skip = "Requires real LLM service - run manually with: dotnet test --filter LLAssistRealLLMTest")]
    public async Task CompleteSystematicScreeningWorkflow_WithRealScopusData_ProducesValidResults()
    {
        // Arrange - Create user and journey
        _output.WriteLine("=== Starting LLAssist Real LLM Test with Scopus Data ===");
        
        // Use a scope to ensure proper DI lifetime
        using var scope = _serviceProvider.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<UserService>();
        var journeyService = scope.ServiceProvider.GetRequiredService<JourneyService>();
        var personaService = scope.ServiceProvider.GetRequiredService<PersonaService>();
        var processEngine = scope.ServiceProvider.GetRequiredService<ProcessEngine>();
        
        // Register the process
        processEngine.RegisterProcess<BasicSystematicScreeningProcess>();
        
        // Step 1: Create demo user
        _output.WriteLine("Step 1: Creating demo user...");
        var user = await userService.CreateOrGetUserAsync("test@example.com", "Test User");
        Assert.NotNull(user);
        _output.WriteLine($"✓ User created: {user.Email}");
        
        // Step 2: Create default personas
        _output.WriteLine("Step 2: Creating personas...");
        var personas = await personaService.GetUserPersonasAsync(user.Id);
        
        // If no personas exist, create a researcher persona
        if (personas == null || !personas.Any())
        {
            await Context.Personas.AddAsync(new Persona
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Domain = "Researcher",
                ConceptualVocabulary = new Dictionary<string, object>
                {
                    ["type"] = "Academic researcher",
                    ["focus"] = "Cybersecurity and AI"
                },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await Context.SaveChangesAsync();
            personas = await personaService.GetUserPersonasAsync(user.Id);
        }
        Assert.NotEmpty(personas);
        var researcherPersona = personas.First(p => p.Domain == "Researcher");
        _output.WriteLine($"✓ Personas created: {string.Join(", ", personas.Select(p => p.Domain))}");
        
        // Step 3: Create journey with systematic screening process
        _output.WriteLine("Step 3: Creating journey...");
        var journey = await journeyService.CreateJourneyAsync(
            user.Id, 
            "Cybersecurity AI Research - Scopus Dataset", 
            researcherPersona.Id, 
            "systematic-screening");
        Assert.NotNull(journey);
        _output.WriteLine($"✓ Journey created: {journey.Purpose}");
        
        // Step 4: Use REAL Scopus test data
        _output.WriteLine("Step 4: Loading REAL Scopus test dataset...");
        var csvContent = TestDataHelper.GetCsvSample("scopus_sample.csv");
        Assert.NotEmpty(csvContent);
        
        // Count actual articles in the CSV
        var csvLines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var articleCount = csvLines.Length - 1; // Subtract header
        _output.WriteLine($"✓ Loaded Scopus CSV with {articleCount} real research papers");
        
        // Use research questions from the test data
        var researchQuestionsText = TestDataHelper.GetResearchQuestionsText("cybersecurity_llm_rqs.txt");
        var researchQuestions = researchQuestionsText.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(q => q.Trim())
            .Where(q => !string.IsNullOrWhiteSpace(q))
            .ToList();
        
        _output.WriteLine($"✓ Loaded {researchQuestions.Count} research questions:");
        foreach (var rq in researchQuestions)
        {
            _output.WriteLine($"  - {rq}");
        }
        
        var inputs = new Dictionary<string, object>
        {
            ["csv_content"] = csvContent,
            ["research_questions"] = string.Join("\n", researchQuestions)
        };
        
        // Step 5: Execute systematic screening process with REAL LLM
        _output.WriteLine("\nStep 5: Executing systematic screening with REAL LLM...");
        _output.WriteLine("⏳ This will make real LLM calls for each paper and may take several minutes...");
        _output.WriteLine($"   Processing {articleCount} papers × {researchQuestions.Count} questions = {articleCount * researchQuestions.Count * 2} assessments");
        
        var startTime = DateTime.UtcNow;
        var result = await processEngine.ExecuteProcessAsync(
            "systematic-screening", 
            journey.Id, 
            inputs);
        var duration = DateTime.UtcNow - startTime;
        
        // Step 6: Verify results
        _output.WriteLine($"\nStep 6: Verifying results (completed in {duration.TotalSeconds:F1} seconds)...");
        
        Assert.NotNull(result);
        Assert.True(result.Success, $"Process failed: {result.ErrorMessage}");
        Assert.NotNull(result.Data);
        
        // Check result structure
        Assert.True(result.Data.ContainsKey("total_articles_attempted"), "Missing total_articles_attempted");
        Assert.True(result.Data.ContainsKey("processing_success_count"), "Missing processing_success_count");
        Assert.True(result.Data.ContainsKey("processing_failure_count"), "Missing processing_failure_count");
        Assert.True(result.Data.ContainsKey("must_read_count"), "Missing must_read_count");
        Assert.True(result.Data.ContainsKey("results"), "Missing results array");
        
        var totalAttempted = Convert.ToInt32(result.Data["total_articles_attempted"]);
        var successCount = Convert.ToInt32(result.Data["processing_success_count"]);
        var failureCount = Convert.ToInt32(result.Data["processing_failure_count"]);
        var mustReadCount = Convert.ToInt32(result.Data["must_read_count"]);
        var mustReadPercentage = result.Data.ContainsKey("must_read_percentage") 
            ? Convert.ToDouble(result.Data["must_read_percentage"]) 
            : 0;
        
        _output.WriteLine($"✓ Process completed successfully");
        _output.WriteLine($"  - Total articles attempted: {totalAttempted}");
        _output.WriteLine($"  - Successfully processed: {successCount}");
        _output.WriteLine($"  - Failed to process: {failureCount}");
        _output.WriteLine($"  - Must-read papers: {mustReadCount} ({mustReadPercentage:F1}%)");
        
        // With real LLM and real data, we expect:
        Assert.Equal(articleCount, totalAttempted); // Should attempt all papers
        Assert.True(successCount > 0, "Should successfully process at least some papers");
        Assert.InRange(mustReadCount, 0, successCount); // Must-read should be subset of successful
        
        // Check for failure transparency
        if (failureCount > 0)
        {
            Assert.True(result.Data.ContainsKey("detailed_failures"), "Should include failure details");
            _output.WriteLine($"⚠️ {failureCount} papers failed processing (this is expected with real LLM)");
        }
        
        // Step 7: Validate CSV output format
        if (result.Data.ContainsKey("csv_output"))
        {
            var csvBase64 = result.Data["csv_output"].ToString();
            Assert.NotEmpty(csvBase64);
            
            var csvBytes = Convert.FromBase64String(csvBase64!);
            var csvOutput = Encoding.UTF8.GetString(csvBytes);
            
            _output.WriteLine($"\n✓ CSV output generated ({csvBytes.Length} bytes)");
            
            // Verify CSV has expected structure for Scopus data
            var outputLines = csvOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Assert.True(outputLines.Length > 1, "CSV should have header and data");
            
            var header = outputLines[0].ToLower();
            Assert.Contains("title", header);
            Assert.Contains("authors", header);
            Assert.Contains("venue", header);
            Assert.Contains("must-read", header);
            
            // Should have columns for each research question
            foreach (var rq in researchQuestions.Take(1)) // Check at least first RQ
            {
                Assert.Contains("rq1-relevance", header);
                Assert.Contains("rq1-contribution", header);
            }
            
            _output.WriteLine($"  - CSV rows: {outputLines.Length}");
            _output.WriteLine($"  - CSV validates Scopus format compatibility ✓");
        }
        
        // Step 8: Verify specific paper processing (spot check)
        if (result.Data.ContainsKey("results") && result.Data["results"] is IEnumerable<dynamic> results)
        {
            var resultsList = results.ToList();
            if (resultsList.Any())
            {
                _output.WriteLine($"\n✓ Spot-checking first processed paper:");
                dynamic firstResult = resultsList.First();
                
                Assert.NotNull(firstResult.title);
                Assert.NotNull(firstResult.must_read);
                Assert.NotNull(firstResult.topics);
                Assert.NotNull(firstResult.assessments);
                
                _output.WriteLine($"  - Title: {firstResult.title}");
                _output.WriteLine($"  - Must Read: {firstResult.must_read}");
                _output.WriteLine($"  - Topics extracted: {string.Join(", ", firstResult.topics)}");
            }
        }
        
        // Step 9: Verify journey state was updated
        _output.WriteLine("\nStep 9: Verifying journey state...");
        var updatedJourney = await journeyService.GetJourneyAsync(user.Id, journey.Id);
        Assert.NotNull(updatedJourney);
        
        // Check process execution record
        var executions = await Context.ProcessExecutions
            .Where(pe => pe.JourneyId == journey.Id)
            .ToListAsync();
        Assert.NotEmpty(executions);
        
        var latestExecution = executions.OrderByDescending(e => e.CreatedAt).First();
        Assert.Equal("Completed", latestExecution.State);
        
        _output.WriteLine($"✓ Journey updated with execution record");
        _output.WriteLine($"  - Execution state: {latestExecution.State}");
        _output.WriteLine($"  - Processing time: {duration.TotalSeconds:F1} seconds");
        _output.WriteLine($"  - Average per paper: {duration.TotalSeconds / articleCount:F1} seconds");
        
        _output.WriteLine("\n=== LLAssist Real LLM Test with Scopus Data PASSED ===");
        _output.WriteLine("✓ Successfully processed real Scopus CSV format");
        _output.WriteLine("✓ Validated compatibility with academic database exports");
        _output.WriteLine("✓ Confirmed LLAssist algorithm works with real-world data");
    }
}