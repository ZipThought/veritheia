using Microsoft.Extensions.DependencyInjection;
using Veritheia.ApiService.Services;
using Veritheia.ApiService.Processes;
using Veritheia.Core.Interfaces;

namespace Veritheia.ApiService;

/// <summary>
/// Extension methods for registering ApiService layer services
/// Encapsulates all business logic service initialization
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all ApiService layer business logic services
    /// </summary>
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        // Core Business Services
        services.AddScoped<UserApiService>();
        services.AddScoped<JourneyApiService>();
        services.AddScoped<PersonaApiService>();
        
        // Document Management Services
        services.AddScoped<DocumentService>();
        services.AddScoped<DocumentIngestionService>();
        services.AddScoped<DocumentMetadataService>();
        services.AddScoped<CorpusImportService>();
        
        // Analysis Services
        services.AddScoped<SemanticExtractionService>();
        
        // Process Implementations
        services.AddScoped<IAnalyticalProcess, SystematicScreeningProcess>();

        return services;
    }
}