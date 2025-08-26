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
using Npgsql;
using Pgvector.EntityFrameworkCore;
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
/// End-to-end test for formation through authorship using systematic screening process.
/// Validates that users can author their intellectual frameworks in natural language,
/// which become the symbolic systems governing document processing.
/// 
/// This test validates the neurosymbolic transcendence - where natural language
/// authorship becomes the symbolic rules, not hard-coded prompts.
/// 
/// Uses real Scopus/IEEE test data to demonstrate formation through engagement.
/// 
/// Prerequisites for local testing:
/// - Local LLM running (e.g., LM Studio on port 1234)
/// - Or set environment variables: LLM_URL and LLM_MODEL
/// </summary>
[Collection("DatabaseTests")]
[Trait("Category", "FormationValidation")]
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

        // Add configuration for LLM
        services.AddSingleton<IConfiguration>(provider =>
        {
            var isCI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")) ||
                      !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"));

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: false);

            // Only load CI config if actually running in CI
            if (isCI)
            {
                configBuilder.AddJsonFile("appsettings.CI.json", optional: true, reloadOnChange: false);
            }

            configBuilder.AddEnvironmentVariables();

            var config = configBuilder.Build();

            // Log the configuration being used
            var llmUrl = config["LLM:Url"] ?? "http://localhost:1234/v1";
            var llmModel = config["LLM:Model"] ?? "llama-3.2-3b-instruct";
            var useTestAdapter = config.GetValue<bool>("Testing:UseTestCognitiveAdapter", false);
            var isCIEnvironment = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")) ||
                      !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"));

            _output.WriteLine($"LLM Configuration: {llmUrl} with model {llmModel}");
            _output.WriteLine($"UseTestCognitiveAdapter: {useTestAdapter}");
            _output.WriteLine($"CI Environment: {isCIEnvironment}");

            return config;
        });

        // Use a singleton factory that creates contexts from the fixture
        // This ensures all contexts use the same database connection
        services.AddScoped<VeritheiaDbContext>(provider => Fixture.CreateContext());

        // Build configuration first to pass to test registration
        var config = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

        // Register cognitive adapter for neural semantic understanding
        TestServiceRegistration.RegisterTestCognitiveAdapter(services, config);
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

        // FileStorageService needs a storage path
        services.AddScoped<FileStorageService>(provider =>
            new FileStorageService(Path.Combine(Path.GetTempPath(), "veritheia_test_storage")));
        services.AddScoped<IDocumentStorageRepository>(provider =>
            provider.GetRequiredService<FileStorageService>());
        services.AddScoped<TextExtractionService>();
        services.AddScoped<EmbeddingService>();
        services.AddScoped<DocumentIngestionService>();

        _serviceProvider = services.BuildServiceProvider();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _output.WriteLine("Database ready for formation journey");

        // Register processes with the ProcessEngine
        var processEngine = _serviceProvider.GetRequiredService<ProcessEngine>();
        processEngine.RegisterProcess<BasicSystematicScreeningProcess>();
        _output.WriteLine("Formation processes registered");
    }

    public override async Task DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();
        await base.DisposeAsync();
    }

    [Fact]
    public async Task FormationJourney_WithUserAuthoredFramework_EnablesUnderstandingDevelopment()
    {
        // Arrange - Create user for formation journey
        _output.WriteLine("=== Starting Formation Journey Test ===");
        _output.WriteLine("Testing neurosymbolic transcendence: Natural language becomes symbolic system");

        // Don't dispose the scope until the entire test completes
        await using var scope = _serviceProvider.CreateAsyncScope();
        var userService = scope.ServiceProvider.GetRequiredService<UserService>();
        var journeyService = scope.ServiceProvider.GetRequiredService<JourneyService>();
        var personaService = scope.ServiceProvider.GetRequiredService<PersonaService>();
        var processEngine = scope.ServiceProvider.GetRequiredService<ProcessEngine>();

        // Register the formation process
        processEngine.RegisterProcess<BasicSystematicScreeningProcess>();

        // Step 1: User begins their formation journey
        _output.WriteLine("\nStep 1: User begins formation journey...");
        var user = await userService.CreateOrGetUserAsync("researcher@university.edu", "Dr. Sarah Chen");
        Assert.NotNull(user);
        _output.WriteLine($"✓ User ready for formation: {user.DisplayName}");

        // Step 2: User establishes their intellectual context (Persona)
        _output.WriteLine("\nStep 2: User establishes intellectual context...");
        var personas = await personaService.GetUserPersonasAsync(user.Id);

        if (personas == null || !personas.Any())
        {
            await Context.Personas.AddAsync(new Persona
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Domain = "Researcher",
                ConceptualVocabulary = new Dictionary<string, object>
                {
                    ["perspective"] = "Cybersecurity researcher focusing on practical AI applications",
                    ["theoretical_stance"] = "Empirical, evidence-based approach",
                    ["methodological_preference"] = "Systematic literature review with dual assessment"
                },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await Context.SaveChangesAsync();
            personas = await personaService.GetUserPersonasAsync(user.Id);
        }

        Assert.NotEmpty(personas);
        var researcherPersona = personas.First(p => p.Domain == "Researcher");
        _output.WriteLine($"✓ Intellectual context established: {researcherPersona.Domain}");

        // Step 3: User creates journey for formation
        _output.WriteLine("\nStep 3: User creates journey for intellectual development...");
        var journey = await journeyService.CreateJourneyAsync(
            user.Id,
            "AI Applications in Cybersecurity - Formation Journey",
            researcherPersona.Id,
            "systematic-screening");
        Assert.NotNull(journey);
        _output.WriteLine($"✓ Formation journey initiated: {journey.Purpose}");

        // Step 4: USER AUTHORS THEIR INTELLECTUAL FRAMEWORK IN NATURAL LANGUAGE
        // This is the key difference - the user's natural language IS the symbolic system
        _output.WriteLine("\nStep 4: User authors their intellectual framework in natural language...");
        _output.WriteLine("This natural language authorship BECOMES the symbolic system governing all processing");

        var intellectualFramework = @"
I am investigating how large language models (LLMs) are being practically deployed 
in cybersecurity threat detection systems. My focus is on real-world implementations 
that have been tested in production environments, not theoretical proposals.

By 'practical deployment' I mean systems that are actually running and processing 
real security events, not proof-of-concepts or research prototypes. I'm particularly 
interested in how these systems handle the challenges of false positives, adversarial 
inputs, and integration with existing security infrastructure.

Papers are relevant if they discuss actual implementations, deployment challenges, 
or empirical evaluations of LLM-based threat detection. Papers that only propose 
theoretical architectures without implementation are not relevant to my research.

Strong contributions are papers that provide performance metrics from production 
deployments, discuss lessons learned from real implementations, or present 
empirical comparisons with traditional threat detection methods.
";

        var researchQuestions = new[]
        {
            "How are LLMs being integrated into production threat detection pipelines?",
            "What are the empirical false positive rates of LLM-based threat detection compared to traditional methods?",
            "How do production systems handle adversarial attacks against LLM-based threat detection?",
            "What are the resource requirements and scalability challenges of deployed LLM threat detection systems?"
        };

        var definitions = @"
Production system: A system processing real security events in a live environment
Empirical evaluation: Performance measured on real-world data, not synthetic datasets
Threat detection: Identification of malicious activity in network traffic, logs, or system behavior
Adversarial attack: Deliberate attempts to deceive or bypass the detection system
False positive rate: Percentage of benign events incorrectly classified as threats
";

        var assessmentCriteria = @"
Papers must provide concrete evidence from actual deployments.
Theoretical proposals without implementation should score low on contribution.
Case studies from production environments should score high on relevance.
Performance comparisons must use real-world data to be considered empirical.
";

        _output.WriteLine("✓ User has authored their complete intellectual framework");
        _output.WriteLine($"  - Intellectual stance: Defined");
        _output.WriteLine($"  - Research questions: {researchQuestions.Length} authored");
        _output.WriteLine($"  - Key definitions: Provided");
        _output.WriteLine($"  - Assessment criteria: Specified");

        // Step 5: Load test corpus for projection through user's framework
        _output.WriteLine("\nStep 5: Loading document corpus for projection...");
        var csvContent = TestDataHelper.GetCsvSample("scopus_sample.csv");
        Assert.NotEmpty(csvContent);

        var csvLines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var documentCount = csvLines.Length - 1; // Subtract header
        _output.WriteLine($"✓ Corpus ready: {documentCount} documents to project through user's framework");

        // Prepare inputs with user's authored framework
        var inputs = new Dictionary<string, object>
        {
            ["intellectual_framework"] = intellectualFramework,
            ["research_questions"] = string.Join("\n", researchQuestions),
            ["definitions"] = definitions,
            ["assessment_criteria"] = assessmentCriteria,
            ["csv_upload"] = csvContent
        };

        // Step 6: MECHANICAL ORCHESTRATION applies user's framework to ALL documents
        _output.WriteLine("\nStep 6: Projecting documents through user-authored framework...");
        _output.WriteLine("⚙️ Mechanical orchestration ensures EVERY document gets identical treatment");
        _output.WriteLine("🧠 Neural semantic understanding interprets user's natural language as symbolic rules");
        _output.WriteLine($"📊 Processing {documentCount} documents through {researchQuestions.Length} research questions");

        var startTime = DateTime.UtcNow;
        ProcessExecutionResult result;
        try
        {
            result = await processEngine.ExecuteProcessAsync(
                "systematic-screening",
                journey.Id,
                inputs);
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Process execution threw exception: {ex.GetType().Name}");
            _output.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
            {
                _output.WriteLine($"Inner exception: {ex.InnerException.Message}");
                if (ex.InnerException.InnerException != null)
                {
                    _output.WriteLine($"Inner inner exception: {ex.InnerException.InnerException.Message}");
                }
            }
            throw;
        }
        var duration = DateTime.UtcNow - startTime;

        // Step 7: Validate formation enablement (not just processing completion)
        _output.WriteLine($"\nStep 7: Validating formation enablement (completed in {duration.TotalSeconds:F1} seconds)...");

        Assert.NotNull(result);

        // Log the actual error if the process failed
        if (!result.Success)
        {
            _output.WriteLine($"Process failed with error: {result.ErrorMessage}");
            if (result.Data != null)
            {
                foreach (var kvp in result.Data)
                {
                    _output.WriteLine($"  {kvp.Key}: {kvp.Value}");
                }
            }
        }

        Assert.True(result.Success, $"Formation journey failed: {result.ErrorMessage}");
        Assert.NotNull(result.Data);

        // Verify framework was captured and applied
        Assert.True(result.Data.ContainsKey("journey_framework"), "User's framework should be preserved");
        var framework = result.Data["journey_framework"] as dynamic;
        Assert.NotNull(framework);
        _output.WriteLine("✓ User's intellectual framework preserved for formation");

        // Verify mechanical orchestration transparency
        Assert.True(result.Data.ContainsKey("processing_summary"), "Processing transparency required");
        var summaryObj = result.Data["processing_summary"];
        Assert.NotNull(summaryObj);

        // Use reflection to access anonymous type properties
        var summaryType = summaryObj.GetType();
        var totalDocuments = (int)summaryType.GetProperty("total_documents").GetValue(summaryObj);
        var successfulProjections = (int)summaryType.GetProperty("successful_projections").GetValue(summaryObj);
        var failedProjections = (int)summaryType.GetProperty("failed_projections").GetValue(summaryObj);

        _output.WriteLine($"✓ Mechanical orchestration complete:");
        _output.WriteLine($"  - Documents attempted: {totalDocuments}");
        _output.WriteLine($"  - Successfully projected: {successfulProjections}");
        _output.WriteLine($"  - Failed projections: {failedProjections}");

        // Verify formation indicators (not just scores)
        Assert.True(result.Data.ContainsKey("formation_indicators"), "Formation indicators required");
        var indicatorsObj = result.Data["formation_indicators"];
        Assert.NotNull(indicatorsObj);

        // Use reflection to access anonymous type properties
        var indicatorsType = indicatorsObj.GetType();
        var requiresEngagement = (int)indicatorsType.GetProperty("documents_requiring_engagement").GetValue(indicatorsObj);
        var engagementPercentage = (double)indicatorsType.GetProperty("engagement_percentage").GetValue(indicatorsObj);

        _output.WriteLine($"✓ Formation indicators generated:");
        _output.WriteLine($"  - Documents requiring engagement: {requiresEngagement}");
        _output.WriteLine($"  - Engagement percentage: {engagementPercentage:F1}%");

        // Verify document projections include framework application
        Assert.True(result.Data.ContainsKey("document_projections"), "Document projections required");
        var projections = result.Data["document_projections"] as IEnumerable<dynamic>;
        Assert.NotNull(projections);

        var projectionsList = projections.ToList();
        Assert.NotEmpty(projectionsList);

        // Step 8: Validate neurosymbolic transcendence
        _output.WriteLine("\nStep 8: Validating neurosymbolic transcendence...");

        // Check that assessments show framework application (not generic scoring)
        var firstProjection = projectionsList.First();
        var projType = firstProjection.GetType();
        var assessmentsProp = projType.GetProperty("assessments");
        Assert.NotNull(assessmentsProp);

        var assessments = assessmentsProp.GetValue(firstProjection) as IEnumerable<object>;
        Assert.NotNull(assessments);
        var assessmentsList = assessments.ToList();
        Assert.NotEmpty(assessmentsList);

        var firstAssessment = assessmentsList.First();
        var assessmentType = firstAssessment.GetType();
        var frameworkAppProp = assessmentType.GetProperty("framework_application");
        Assert.NotNull(frameworkAppProp);

        _output.WriteLine("✓ Framework application verified:");
        var titleProp = projType.GetProperty("title");
        var requiresEngagementProp = projType.GetProperty("requires_engagement");
        var topicsProp = projType.GetProperty("framework_topics");

        _output.WriteLine($"  - Document: {titleProp?.GetValue(firstProjection)}");
        _output.WriteLine($"  - Requires engagement: {requiresEngagementProp?.GetValue(firstProjection)}");
        if (topicsProp != null)
        {
            var topics = topicsProp.GetValue(firstProjection) as IEnumerable<string>;
            if (topics != null)
                _output.WriteLine($"  - Framework-specific topics: {string.Join(", ", topics)}");
        }

        // Step 9: Verify formation prompts for engagement
        Assert.True(result.Data.ContainsKey("formation_prompts"), "Formation prompts required");
        var prompts = result.Data["formation_prompts"] as List<string>;
        if (prompts != null && prompts.Any())
        {
            _output.WriteLine("\n✓ Formation prompts generated for user engagement:");
            foreach (var prompt in prompts.Take(3))
            {
                _output.WriteLine($"  - {prompt}");
            }
        }

        // Step 10: Validate journey state for continued formation
        _output.WriteLine("\nStep 10: Validating journey ready for continued formation...");
        var updatedJourney = await journeyService.GetJourneyAsync(user.Id, journey.Id);
        Assert.NotNull(updatedJourney);

        // Check process execution was recorded for formation tracking
        var executions = await Context.ProcessExecutions
            .Where(pe => pe.JourneyId == journey.Id)
            .ToListAsync();
        Assert.NotEmpty(executions);

        var latestExecution = executions.OrderByDescending(e => e.CreatedAt).First();
        Assert.Equal("Completed", latestExecution.State);

        _output.WriteLine("✓ Journey ready for continued formation");
        _output.WriteLine($"  - Execution state: {latestExecution.State}");
        _output.WriteLine($"  - Formation can continue through iterative refinement");

        // Final validation: This enables formation, not just processing
        _output.WriteLine("\n=== Formation Journey Test Complete ===");
        _output.WriteLine("✅ User authored their framework in natural language");
        _output.WriteLine("✅ Framework became the symbolic system governing processing");
        _output.WriteLine("✅ Documents were projected through user's intellectual lens");
        _output.WriteLine("✅ Results enable engagement for understanding development");
        _output.WriteLine("✅ Formation through authorship validated");

        _output.WriteLine("\nKey Achievement: User didn't need to code - their natural language IS the system");
    }
}