# Veritheia Implementation

This document maps the conceptual architecture defined in ARCHITECTURE.md to concrete technical implementations.

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
- `Entity` & `Relationship`: Knowledge graph components

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
- `IProcessRegistry` - Dynamic process management

### .NET Aspire Configuration

The `veritheia.AppHost` project orchestrates:
- PostgreSQL container with pgvector
- Redis cache
- API and Web services
- Development dashboard

## Process Implementation

### Static Processes

Built-in processes in `veritheia.Core.Processes.Static`:
- `DocumentIngestionProcess`
- `TextExtractionProcess`
- `EmbeddingGenerationProcess`
- `MetadataExtractionProcess`

### Dynamic Process Framework

Dynamic processes implement `IAnalyticalProcess`:
```csharp
public interface IAnalyticalProcess
{
    Task<ProcessResult> ExecuteAsync(ProcessContext context, KnowledgeScope scope);
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