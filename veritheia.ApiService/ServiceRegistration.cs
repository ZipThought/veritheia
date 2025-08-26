using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Veritheia.Data;
using Veritheia.Data.Services;
using Veritheia.Core.Interfaces;
using Veritheia.Data.Processes;

namespace Veritheia.ApiService;

/// <summary>
/// Service registration for the ApiService business logic component
/// This component provides the application programming interface for all system operations
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Register all ApiService services with the dependency injection container
    /// </summary>
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment? environment = null)
    {
        // Register database context
        var connectionString = configuration.GetConnectionString("postgres") 
            ?? throw new InvalidOperationException("PostgreSQL connection string not configured");
            
        services.AddDbContext<VeritheiaDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.UseVector();
            }));

        // Register core services
        services.AddScoped<JourneyService>();
        services.AddScoped<PersonaService>();
        services.AddScoped<UserService>();
        services.AddScoped<DocumentService>();
        services.AddScoped<JournalService>();
        services.AddScoped<ProcessEngine>();
        services.AddScoped<ProcessWorkerService>();
        
        // Register cognitive and extraction services
        // CRITICAL: Only use production cognitive adapter in production environments
        RegisterCognitiveAdapter(services, configuration, environment);
        services.AddScoped<SemanticExtractionService>();
        services.AddScoped<TextExtractionService>();
        services.AddScoped<EmbeddingService>();
        services.AddScoped<DocumentIngestionService>();
        
        // Register file handling services
        services.AddScoped<FileStorageService>();
        services.AddScoped<CsvParserService>();
        services.AddScoped<CsvWriterService>();
        
        // Register analytical processes
        services.AddScoped<IAnalyticalProcess, BasicSystematicScreeningProcess>();
        services.AddScoped<IAnalyticalProcess, BasicConstrainedCompositionProcess>();
        
        // Register document storage
        services.AddScoped<IDocumentStorageRepository, FileStorageService>();
        
        return services;
    }
    
    /// <summary>
    /// Register the appropriate cognitive adapter based on environment
    /// CRITICAL: Test adapters must NEVER be registered in production
    /// </summary>
    private static void RegisterCognitiveAdapter(IServiceCollection services, IConfiguration configuration, IHostEnvironment? environment)
    {
        // Check if we're in a test environment
        var isTestEnvironment = IsTestEnvironment(configuration, environment);
        
        if (isTestEnvironment)
        {
            // For test environments, check if a test cognitive adapter should be used
            // This allows CI/dev environments without LLM to still run integration tests
            var useTestAdapter = configuration.GetValue<bool>("Testing:UseTestCognitiveAdapter", false);
            
            if (useTestAdapter)
            {
                // Try to load test adapter from test assembly using reflection
                // This approach prevents test adapters from being accessible in production
                // because test assemblies won't be present in production builds
                try
                {
                    var testAssembly = AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => a.FullName?.Contains("veritheia.Tests") == true);
                    
                    if (testAssembly != null)
                    {
                        var testAdapterType = testAssembly.GetType("Veritheia.Tests.Helpers.TestCognitiveAdapter");
                        if (testAdapterType != null)
                        {
                            // Verify the type implements ICognitiveAdapter
                            if (typeof(ICognitiveAdapter).IsAssignableFrom(testAdapterType))
                            {
                                services.AddSingleton(typeof(ICognitiveAdapter), testAdapterType);
                                Console.WriteLine("INFO: Test cognitive adapter registered for integration testing");
                                return;
                            }
                        }
                    }
                    
                    Console.WriteLine("WARNING: Test cognitive adapter requested but test assembly not found or invalid.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WARNING: Failed to load test cognitive adapter: {ex.Message}");
                }
                
                // Fallback to production adapter if test adapter can't be loaded
                Console.WriteLine("INFO: Using production cognitive adapter as fallback.");
                services.AddHttpClient<OpenAICognitiveAdapter>();
                services.AddScoped<ICognitiveAdapter, OpenAICognitiveAdapter>();
                return;
            }
        }
        
        // Production path: Always use real OpenAI adapter
        // Validate that we're not accidentally using test adapters
        ValidateProductionEnvironment(configuration, environment);
        
        services.AddHttpClient<OpenAICognitiveAdapter>();
        services.AddScoped<ICognitiveAdapter, OpenAICognitiveAdapter>();
    }
    
    /// <summary>
    /// Determine if we're running in a test environment
    /// </summary>
    private static bool IsTestEnvironment(IConfiguration configuration, IHostEnvironment? environment)
    {
        // Check environment name
        if (environment?.EnvironmentName == "Test" || environment?.EnvironmentName == "Testing")
            return true;
            
        // Check for test configuration
        if (configuration.GetValue<bool>("Testing:IsTestEnvironment", false))
            return true;
            
        // Check if we're running under test framework
        var isUnderTest = AppDomain.CurrentDomain.GetAssemblies()
            .Any(assembly => assembly.FullName?.Contains("Microsoft.TestPlatform") == true ||
                           assembly.FullName?.Contains("xunit") == true ||
                           assembly.FullName?.Contains("NUnit") == true);
                           
        return isUnderTest;
    }
    
    /// <summary>
    /// Validate that production environments are not using test configurations
    /// </summary>
    private static void ValidateProductionEnvironment(IConfiguration configuration, IHostEnvironment? environment)
    {
        var isProduction = environment?.IsProduction() == true;
        var useTestAdapter = configuration.GetValue<bool>("Testing:UseTestCognitiveAdapter", false);
        
        if (isProduction && useTestAdapter)
        {
            throw new InvalidOperationException(
                "CRITICAL SECURITY ERROR: Test cognitive adapter configuration detected in production environment. " +
                "This violates data integrity requirements and would corrupt formation processes. " +
                "Remove 'Testing:UseTestCognitiveAdapter' configuration from production settings.");
        }
        
        // Additional validation for test-related configuration in production
        var testConfig = configuration.GetSection("Testing");
        if (isProduction && testConfig.Exists() && testConfig.GetChildren().Any())
        {
            throw new InvalidOperationException(
                "CRITICAL CONFIGURATION ERROR: Testing section found in production configuration. " +
                "All test-related configuration must be removed from production environments to " +
                "ensure data integrity and prevent formation corruption.");
        }
    }
}