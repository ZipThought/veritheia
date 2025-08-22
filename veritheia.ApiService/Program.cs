using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Veritheia.Core.Interfaces;
using Veritheia.Data;
using Veritheia.Data.Processes;
using Veritheia.Data.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Register Database
builder.Services.AddDbContext<VeritheiaDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("veritheiadb");
    options.UseNpgsql(connectionString);
});

// Register Services (Post-DDD: Direct service registration)
builder.Services.AddScoped<JourneyService>();
builder.Services.AddScoped<DocumentService>();
builder.Services.AddScoped<ProcessEngine>();

// Platform Services
builder.Services.AddScoped<DocumentIngestionService>();
builder.Services.AddScoped<TextExtractionService>();
builder.Services.AddScoped<EmbeddingService>();

// LLAssist Services
builder.Services.AddScoped<CsvParserService>();
builder.Services.AddScoped<CsvWriterService>();
builder.Services.AddScoped<SemanticExtractionService>();

// User & Journey System (Phase 7)
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<JournalService>();
builder.Services.AddScoped<PersonaService>();

// Cognitive Adapter (Phase 8) - Local LLM implementation
builder.Services.AddHttpClient<LocalLLMAdapter>();
builder.Services.AddSingleton<ICognitiveAdapter, LocalLLMAdapter>();

// Process Worker Service - Background execution
builder.Services.AddHostedService<ProcessWorkerService>();

// Analytical Processes (Phase 9-10) - SKELETON: Basic structure only
// These would be discovered and registered by ProcessEngine in real implementation

builder.Services.AddScoped<IDocumentStorageRepository>(sp =>
{
    var environment = sp.GetRequiredService<IWebHostEnvironment>();
    var storagePath = Path.Combine(environment.ContentRootPath, "Storage");
    return new FileStorageService(storagePath);
});

// Register Controllers
builder.Services.AddControllers();

// Register Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Veritheia API", 
        Version = "v1",
        Description = "API for the Veritheia environment for inquiry",
        Contact = new OpenApiContact
        {
            Name = "Veritheia Team",
            Email = "support@veritheia.local"
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

// Enable routing
app.UseRouting();

// Configure Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Veritheia API V1");
        c.RoutePrefix = "swagger";
    });
}

// Map endpoints
app.MapControllers();
app.MapDefaultEndpoints();

// Redirect root to Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();

// Make Program class accessible for testing
public partial class Program { }
