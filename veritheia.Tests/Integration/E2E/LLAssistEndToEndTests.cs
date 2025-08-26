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
using Xunit;
using Xunit.Abstractions;

namespace veritheia.Tests.Integration.E2E;

/// <summary>
/// End-to-end integration tests for the complete LLAssist workflow.
/// Uses the proper DatabaseFixture for real PostgreSQL testing with Testcontainers.
/// 
/// To run: dotnet test --filter "FullyQualifiedName~LLAssistEndToEndTests"
/// </summary>
[Collection("DatabaseTests")]
[Trait("Category", "E2E")]
public class LLAssistEndToEndTests : DatabaseTestBase
{
    private readonly ITestOutputHelper _output;
    private readonly ServiceProvider _serviceProvider;
    
    public LLAssistEndToEndTests(DatabaseFixture fixture, ITestOutputHelper output) : base(fixture)
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
        
        // Add services using the actual registration
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
        services.AddScoped<FileStorageService>();
        services.AddScoped<IDocumentStorageRepository, FileStorageService>();
        services.AddScoped<TextExtractionService>();
        services.AddScoped<EmbeddingService>();
        services.AddScoped<DocumentIngestionService>();
        
        // Add configuration
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
    
    [Fact]
    public async Task CompleteSystematicScreeningWorkflow_WithRealLLM_ProducesValidResults()
    {
        // Arrange - Create user and journey
        _output.WriteLine("=== Starting LLAssist End-to-End Test ===");
        
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
                    ["type"] = "Academic researcher"
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
            "ML Security Research Screening", 
            researcherPersona.Id, 
            "systematic-screening");
        Assert.NotNull(journey);
        _output.WriteLine($"✓ Journey created: {journey.Purpose}");
        
        // Step 4: Prepare test data
        _output.WriteLine("Step 4: Preparing test dataset...");
        var csvContent = GenerateTestCsvData();
        var researchQuestions = new List<string>
        {
            "How are large language models being utilized for cybersecurity threat detection?",
            "What machine learning techniques are most effective for identifying advanced threats?"
        };
        
        var inputs = new Dictionary<string, object>
        {
            ["csv_content"] = csvContent,
            ["research_questions"] = string.Join("\n", researchQuestions)
        };
        
        _output.WriteLine($"✓ Dataset prepared with {csvContent.Split('\n').Length - 1} papers");
        _output.WriteLine($"✓ Research questions: {researchQuestions.Count}");
        
        // Step 5: Execute systematic screening process
        _output.WriteLine("Step 5: Executing systematic screening process...");
        _output.WriteLine("⏳ This will make real LLM calls and may take a few minutes...");
        
        var startTime = DateTime.UtcNow;
        var result = await processEngine.ExecuteProcessAsync(
            "systematic-screening", 
            journey.Id, 
            inputs);
        var duration = DateTime.UtcNow - startTime;
        
        // Step 6: Verify results
        _output.WriteLine($"Step 6: Verifying results (completed in {duration.TotalSeconds:F1} seconds)...");
        
        Assert.NotNull(result);
        Assert.True(result.Success, $"Process failed: {result.ErrorMessage}");
        Assert.NotNull(result.Data);
        
        // Check result structure
        Assert.True(result.Data.ContainsKey("total_articles"), "Missing total_articles in results");
        Assert.True(result.Data.ContainsKey("must_read_count"), "Missing must_read_count in results");
        Assert.True(result.Data.ContainsKey("results"), "Missing results array");
        
        var totalArticles = Convert.ToInt32(result.Data["total_articles"]);
        var mustReadCount = Convert.ToInt32(result.Data["must_read_count"]);
        var mustReadPercentage = result.Data.ContainsKey("must_read_percentage") 
            ? Convert.ToDouble(result.Data["must_read_percentage"]) 
            : 0;
        
        _output.WriteLine($"✓ Process completed successfully");
        _output.WriteLine($"  - Total articles processed: {totalArticles}");
        _output.WriteLine($"  - Must-read papers: {mustReadCount} ({mustReadPercentage:F1}%)");
        
        // Verify reasonable results
        Assert.InRange(totalArticles, 1, 10); // We're sending a small test dataset
        Assert.InRange(mustReadCount, 0, totalArticles); // Must-read should be subset
        
        // Step 7: Check for CSV output
        if (result.Data.ContainsKey("csv_output"))
        {
            var csvBase64 = result.Data["csv_output"].ToString();
            Assert.NotEmpty(csvBase64);
            
            var csvBytes = Convert.FromBase64String(csvBase64!);
            var csvOutput = Encoding.UTF8.GetString(csvBytes);
            
            _output.WriteLine($"✓ CSV output generated ({csvBytes.Length} bytes)");
            
            // Verify CSV has expected columns
            var lines = csvOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Assert.True(lines.Length > 1, "CSV should have header and data");
            
            var header = lines[0].ToLower();
            Assert.Contains("title", header);
            Assert.Contains("must-read", header);
            Assert.Contains("relevance", header);
            
            _output.WriteLine($"  - CSV rows: {lines.Length}");
            _output.WriteLine($"  - Header: {lines[0].Substring(0, Math.Min(100, lines[0].Length))}...");
        }
        
        // Step 8: Verify journey state was updated
        _output.WriteLine("Step 8: Verifying journey state...");
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
        _output.WriteLine($"  - Started: {latestExecution.StartedAt}");
        _output.WriteLine($"  - Completed: {latestExecution.CompletedAt}");
        
        _output.WriteLine("\n=== LLAssist End-to-End Test PASSED ===");
    }
    
    private string GenerateTestCsvData()
    {
        // Create a small test dataset with 3 papers
        var csv = new StringBuilder();
        csv.AppendLine("title,abstract,authors,year,venue,doi,link,keywords");
        
        // Paper 1: Highly relevant to LLMs and cybersecurity
        csv.AppendLine("\"Large Language Models for Automated Threat Detection\",\"This paper presents a novel approach using large language models (LLMs) to automatically detect and classify cybersecurity threats in real-time. We demonstrate that transformer-based models can identify zero-day vulnerabilities with 94% accuracy.\",\"Smith, J.; Doe, A.\",2024,\"IEEE Security\",\"10.1109/SEC.2024.001\",\"https://doi.org/10.1109/SEC.2024.001\",\"LLM;cybersecurity;threat detection\"");
        
        // Paper 2: Somewhat relevant - ML but not specifically LLMs
        csv.AppendLine("\"Machine Learning in Network Security\",\"A survey of machine learning techniques applied to network intrusion detection. We review traditional ML approaches including SVM and random forests for anomaly detection.\",\"Brown, K.; Wilson, L.\",2023,\"ACM Computing Surveys\",\"10.1145/CS.2023.002\",\"https://doi.org/10.1145/CS.2023.002\",\"machine learning;network security;anomaly detection\"");
        
        // Paper 3: Not relevant - different domain
        csv.AppendLine("\"Blockchain Applications in Supply Chain\",\"This paper explores the use of blockchain technology for supply chain management and traceability. We present a distributed ledger framework for tracking goods.\",\"Garcia, M.; Lee, H.\",2023,\"Journal of Supply Chain Management\",\"10.1016/j.scm.2023.003\",\"https://doi.org/10.1016/j.scm.2023.003\",\"blockchain;supply chain;distributed ledger\"");
        
        return csv.ToString();
    }
}