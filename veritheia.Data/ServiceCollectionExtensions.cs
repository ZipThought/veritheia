using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Pgvector.EntityFrameworkCore;
using Veritheia.Core.Interfaces;
using Veritheia.Data.Processes;
using Veritheia.Data.Services;

namespace Veritheia.Data;

/// <summary>
/// Extension methods for registering Data layer services
/// Encapsulates all database and data service initialization
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all Data layer services and database context
    /// </summary>
    public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // Configure JSON serialization for Npgsql
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        // Register Database
        services.AddDbContext<VeritheiaDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("veritheiadb");
            options.UseNpgsql(connectionString, o => 
            {
                o.UseVector();
            });
        });

        // Register Core Data Services
        services.AddScoped<JourneyService>();
        services.AddScoped<PersonaService>();
        services.AddScoped<UserService>();
        services.AddScoped<DocumentService>();
        services.AddScoped<ProcessEngine>();
        services.AddScoped<JournalService>();

        // Platform Services
        services.AddScoped<DocumentIngestionService>();
        services.AddScoped<TextExtractionService>();
        services.AddScoped<EmbeddingService>();

        // LLAssist Services
        services.AddScoped<CsvParserService>();
        services.AddScoped<CsvWriterService>();
        services.AddScoped<SemanticExtractionService>();

        // Cognitive Adapter - Local LLM implementation
        services.AddHttpClient<LocalLLMAdapter>();
        services.AddSingleton<ICognitiveAdapter, LocalLLMAdapter>();

        // Process Worker Service - Background execution
        services.AddHostedService<ProcessWorkerService>();

        // Document Storage
        services.AddScoped<IDocumentStorageRepository>(sp =>
        {
            var storagePath = Path.Combine(environment.ContentRootPath, "Storage");
            return new FileStorageService(storagePath);
        });

        return services;
    }
}