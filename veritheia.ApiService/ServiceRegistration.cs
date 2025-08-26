using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
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
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
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
        services.AddHttpClient<OpenAICognitiveAdapter>();
        services.AddScoped<ICognitiveAdapter, OpenAICognitiveAdapter>();
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
}