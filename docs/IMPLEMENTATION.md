# Veritheia Implementation

This document shows how technical choices ensure users remain the authors of their understanding. Every implementation decision supports intellectual sovereignty—from database schemas that preserve journey context to process interfaces that maintain personal relevance.

## Technology Mapping

### I. Knowledge Database → PostgreSQL with pgvector

The Knowledge Database is implemented using **PostgreSQL** with the **pgvector** extension.

- **Rationale**: Unified storage for both relational metadata and high-dimensional vector embeddings
- **Version**: PostgreSQL 16 with pgvector extension
- **Deployment**: Containerized via .NET Aspire orchestration

### II. Process Engine → ASP.NET Core API

The Process Engine is implemented as an **ASP.NET Core 8.0** Web API.

- **Project**: `veritheia.ApiService`
- **Framework**: .NET 8.0 with C# 12
- **Patterns**: Repository pattern for data access, CQRS for command/query separation

### III. Presentation Tier → Blazor Server

The web client is implemented using **Blazor Server**.

- **Project**: `veritheia.Web`
- **Framework**: Blazor on .NET 8.0
- **Rationale**: Unified C# codebase, real-time updates, simplified deployment

### IV. Cognitive System Interface → Adapter Pattern

The Cognitive System is accessed through a C# interface:

```csharp
public interface ICognitiveAdapter
{
    Task<EmbeddingResult> CreateEmbeddingsAsync(string text);
    Task<string> GenerateTextAsync(string prompt, GenerationParameters parameters);
}
```

**Implementations**:
- `LlamaCppAdapter`: Local inference using llama.cpp via P/Invoke
- `SemanticKernelAdapter`: Microsoft Semantic Kernel integration
- `OpenAIAdapter`: OpenAI API (for comparison/testing only)

## Data Layer Implementation

### Entity Framework Core Configuration

**Project**: `veritheia.Data`

Key entities:
- `Document`: Raw corpus storage
- `ProcessedContent`: Embeddings and extracted data
- `KnowledgeScope`: Virtual knowledge boundaries
- `ProcessDefinition`: Metadata for available processes
- `ProcessExecution`: Process run history and state
- `ProcessResult`: Extensible result storage using JSON columns

### Vector Storage

- Embedding dimension: 1536 (OpenAI ada-002 compatible)
- Index type: IVFFlat with cosine distance
- Query optimization: Approximate nearest neighbor search

### Migration Strategy

```bash
cd veritheia.ApiService
dotnet ef migrations add <MigrationName> -p ../veritheia.Data -s . -o Migrations
```

## Service Architecture

### Dependency Injection

Core services registered in `Program.cs`:
- `VeritheiaDbContext` - EF Core database context
- `ICognitiveAdapter` - Cognitive system abstraction
- `IKnowledgeRepository` - Knowledge layer operations
- `IProcessEngine` - Process execution runtime
- `IProcessRegistry` - Process discovery and metadata

Process registration pattern:
```csharp
services.AddScoped<IAnalyticalProcess, DocumentIngestionProcess>();
services.AddScoped<IAnalyticalProcess, SystematicScreeningProcess>();
```

### .NET Aspire Configuration

The `veritheia.AppHost` project orchestrates:
- PostgreSQL container with pgvector
- Redis cache
- API and Web services
- Development dashboard

## Process Implementation

### Process Interface Architecture

All processes implement a common interface:
```csharp
public interface IAnalyticalProcess
{
    ProcessDefinition GetDefinition();
    Task<ProcessResult> ExecuteAsync(ProcessContext context);
    IProcessResultRenderer GetResultRenderer();
}

public class ProcessDefinition
{
    public string ProcessType { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ProcessTriggerType TriggerType { get; set; }
    public InputDefinition Inputs { get; set; }
}
```

### Platform Services (Static Processes)

Core services available to all processes:
```csharp
public interface IPlatformServices
{
    IDocumentProcessor DocumentProcessor { get; }
    ITextExtractor TextExtractor { get; }
    IEmbeddingGenerator EmbeddingGenerator { get; }
    IMetadataExtractor MetadataExtractor { get; }
    IDocumentChunker DocumentChunker { get; }
}
```

Implementation in `veritheia.Core.Processes.Static`:
- `DocumentIngestionProcess` - Automatic pipeline for new documents
- `TextExtractionService` - PDF and text file processing
- `EmbeddingGenerationService` - Vector embedding creation
- `MetadataExtractionService` - Document metadata extraction

### Analytical Processes (Dynamic)

Example implementation pattern:
```csharp
public class SystematicScreeningProcess : IAnalyticalProcess
{
    private readonly IKnowledgeRepository _knowledge;
    private readonly ICognitiveAdapter _cognitive;
    
    public ProcessDefinition GetDefinition() => new()
    {
        ProcessType = "SystematicScreening",
        Name = "Systematic Literature Review",
        TriggerType = ProcessTriggerType.Manual,
        Inputs = new InputDefinition()
            .AddTextArea("researchQuestions", required: true)
            .AddTextArea("inclusionCriteria", required: true)
            .AddScopeSelector("scope", required: false)
    };
    
    public async Task<ProcessResult> ExecuteAsync(ProcessContext context)
    {
        // Every step shaped by user input
        // Results meaningful only within the specific journey
        // Output bears the author's intellectual fingerprint
    }
}
```

## Development Workflow

### Local Development

1. Ensure Docker Desktop is running
2. Run `dotnet run --project veritheia.AppHost`
3. Access services:
   - Web UI: https://localhost:5001
   - API: https://localhost:5000
   - Aspire Dashboard: https://localhost:15000

### Testing Strategy

- **Unit Tests**: Core logic and domain models
- **Integration Tests**: API endpoints with test database
- **E2E Tests**: Blazor components with Playwright

### Debugging

- Use Aspire Dashboard for distributed tracing
- PostgreSQL logs available in Docker container
- Application Insights integration for production

## Security Implementation

### Authentication & Authorization

- ASP.NET Core Identity for user management
- JWT tokens for API authentication
- Role-based access control for knowledge scopes

### Data Protection

- Encryption at rest via PostgreSQL TDE
- TLS for all service communication
- Sensitive data never logged

## Performance Considerations

### Caching Strategy

- Redis for frequently accessed metadata
- In-memory cache for embedding lookups
- Output caching for read-heavy API endpoints

### Scaling Approach

- Horizontal scaling for API instances
- Read replicas for PostgreSQL
- Dedicated embedding generation workers