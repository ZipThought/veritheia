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
    /// Register the cognitive adapter for production use
    /// </summary>
    private static void RegisterCognitiveAdapter(IServiceCollection services, IConfiguration configuration, IHostEnvironment? environment)
    {
        // PRODUCTION CODE: Always use real OpenAI adapter
        // No test logic should exist in production code
        services.AddHttpClient<OpenAICognitiveAdapter>();
        services.AddScoped<ICognitiveAdapter, OpenAICognitiveAdapter>();
    }

}