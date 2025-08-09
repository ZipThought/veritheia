# Veritheia Implementation

## 1. Overview

This document specifies the technical implementation of Veritheia. The system operates as a local-first epistemic infrastructure with four primary components: PostgreSQL with pgvector for knowledge storage, ASP.NET Core for process orchestration, adapter-based LLM integration for assessments, and Blazor Server for user interfaces. All components enforce user data sovereignty and prevent automated insight generation.


## 2. Technology Stack

### 2.1 Knowledge Database

PostgreSQL 16 with pgvector extension provides unified storage for documents, metadata, and embeddings. The pgvector extension enables efficient similarity search over 1536-dimensional embedding vectors while maintaining ACID guarantees for relational data. Deployment uses containerization through .NET Aspire for consistent development and production environments.

### 2.2 Process Engine

ASP.NET Core 8.0 implements the Process Engine as a RESTful API service. The architecture employs Domain-Driven Design with aggregate boundaries around User, Journey, and Document entities. Data access uses the Repository pattern with Entity Framework Core 8.0, while CQRS separates read and write operations for scalability.

### 2.3 Presentation Tier

Blazor Server provides the web interface, enabling real-time updates through SignalR connections. This architecture choice eliminates JavaScript complexity while maintaining responsive user experiences. Component design follows a strict separation between user-authored content display and system-provided structure.

### 2.4 Cognitive System Integration

The ICognitiveAdapter interface abstracts LLM implementation details, supporting multiple backends: LlamaCppAdapter for local inference, SemanticKernelAdapter for Microsoft Semantic Kernel, and OpenAIAdapter for cloud-based models. Each adapter implements assessment-only operations, preventing insight generation through prompt engineering constraints.

## Data Architecture

### Entity Model

The data layer (`veritheia.Data`) defines these core entities:

- **Document**: Represents raw corpus materials (PDFs, text files)
- **ProcessedContent**: Stores embeddings and extracted text chunks
- **KnowledgeScope**: Defines virtual boundaries for knowledge organization
- **ProcessDefinition**: Metadata describing available processes
- **ProcessExecution**: Tracks process runs and their state
- **ProcessResult**: Stores process outputs with extensible JSON schema
- **User**: Core identity with associated persona and knowledge base
- **Journey**: User's engagement instance with a process
- **Journal**: Narrative record of intellectual development (Research, Method, Decision, Reflection)
- **JournalEntry**: Individual narrative entries within journals

### Primary Key Strategy

All entities use **UUIDv7 (RFC 9562)** as primary keys:

- **Format**: Standard UUID format (36 characters with hyphens in string form, 16 bytes binary)
- **Benefits**: 
  - Time-ordered like ULID (48-bit Unix timestamp in milliseconds)
  - Native PostgreSQL UUID type support (no conversion needed)
  - Built-in .NET 8+ support via `Guid.CreateVersion7()`
  - RFC standardization ensures long-term stability
  - Forward compatible with PostgreSQL 18's native `uuidv7()` function
- **Implementation**: Direct mapping between C# `Guid` and PostgreSQL `uuid` type
- **Example**: `018e3d28-a729-7000-8000-000000000000`

Previous specification (ULID) was reconsidered due to:
- UUIDv7 provides same temporal ordering benefits
- Native database type eliminates conversion overhead
- Industry standard with wider tooling support

### Database Design Patterns

- **Repository Pattern**: Generic `IRepository<T>` with concrete implementations
- **Unit of Work**: Transaction management across repositories
- **Specification Pattern**: Complex queries via `ISpecification<T>`
- **Value Converters**: UTC DateTime, JSONB for PostgreSQL
- **Soft Deletes**: Logical deletion with `DeletedAt` timestamp
- **Auditing**: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy on all entities
- **Query Optimization**: Use `.AsNoTracking()` for all read-only operations
- **Vector Queries**: Use `FromSqlRaw()` for pgvector operations with `<->` operator

### Vector Storage Strategy

- **Multiple Embedding Support**: Dimension-grouped columns to support model evolution
  - `embedding_1536`: OpenAI ada-002, Cohere embed-v3
  - `embedding_768`: E5-large-v2, BGE-large
  - `embedding_384`: Lightweight/mobile models
- **Index Type**: HNSW (Hierarchical Navigable Small World) with filtered indexes
  - Filtered by `processing_model` to reduce index size
  - Separate index per model for optimal performance
  - O(log n) search complexity
  - Default parameters: m=16, ef_construction=64
- **Query Routing**: Service layer selects column based on embedding dimension
- **Model Migration**: Add column for new dimension, background re-embedding, atomic switch

### Database Migrations

The system uses Entity Framework Core migrations with this workflow:
1. Define entity changes in `veritheia.Data`
2. Generate migrations from `veritheia.ApiService` context
3. Apply migrations during startup or deployment

## Service Architecture

### Dependency Injection Structure

The application uses ASP.NET Core's built-in dependency injection with these service lifetimes:

- **Scoped Services**: Database contexts, repositories, process instances
- **Singleton Services**: Configuration, cognitive adapters, caching
- **Transient Services**: Validators, mappers, utilities

### Platform Services

The platform provides guaranteed services that all processes can depend on:

- **Document Encounter Service**: Records HOW and WHY a document entered a journey
- **Conceptual Embedding Service**: Creates embeddings that reflect the author's conceptual framework
- **Personal Metadata Service**: Extracts metadata relevant to the author's inquiry
- **Semantic Chunking Service**: Splits documents based on the author's conceptual boundaries
- **Journey Repository**: All data access filtered through journey context

### Process Registration

Processes are registered through a convention-based pattern that ensures proper dependency injection and discovery. Each process is registered both as itself and as an `IAnalyticalProcess` implementation.

## Process Architecture

### Process Execution Flow

1. **Input Collection**: Dynamic forms generated from process definition
2. **Context Creation**: User journey and inputs packaged into ProcessContext
3. **Process Execution**: Business logic runs with access to platform services
4. **Result Storage**: Outputs saved with full provenance and versioning
5. **Result Rendering**: Process-specific UI components display results

### Reference Process Patterns

#### Systematic Screening Process (Analytical Pattern)
- Implements dual assessment: relevance and contribution
- Uses cognitive system in two distinct modes (librarian vs peer reviewer)
- Produces filterable results with detailed rationales
- Demonstrates journey-specific analysis

Journal Integration:
- **Research Journal**: Records findings about relevant papers
- **Decision Journal**: Documents inclusion/exclusion rationales
- **Method Journal**: Captures evolving search strategies
- **Reflection Journal**: Notes emerging patterns and insights

#### Guided Composition Process (Compositional Pattern)
- Generates constrained content based on source materials
- Creates evaluation rubrics aligned with objectives
- Implements real-time assessment with feedback
- Shows teacher/student role differentiation

Journal Integration:
- **Method Journal**: Teaching approaches and constraint design
- **Decision Journal**: Rubric adjustments and grading overrides
- **Reflection Journal**: Student progress observations
- **Research Journal**: Pedagogical insights from assignments

### Process Context

Every process execution receives a context that includes:
- Current knowledge scope
- User journey with assembled journal context
- Process-specific inputs
- Execution metadata
- Platform service references

Context Assembly:
1. **Journal Selection**: Relevant journals for current task
2. **Entry Extraction**: Recent significant entries
3. **Narrative Compression**: Maintaining coherence within token limits
4. **Persona Integration**: User's conceptual vocabulary and patterns

This context ensures outputs remain personally relevant and meaningful within the specific inquiry.

## Extension Architecture

### Extension Points

The system provides several extension points for adding new capabilities:

1. **Process Extensions**: New analytical workflows via `IAnalyticalProcess`
2. **Data Model Extensions**: Domain-specific entities related to process executions
3. **UI Component Extensions**: Custom Blazor components for process interfaces
4. **Result Renderer Extensions**: Specialized visualization for process outputs

### Extension Integration

Extensions integrate through:
- Service registration in dependency injection container
- Entity Framework migrations for data model changes
- Blazor component registration for UI elements
- Process registry for discovery and metadata

For detailed extension development, see [EXTENSION-GUIDE.md](./EXTENSION-GUIDE.md).

## Development Environment

### Local Development Setup

1. **Prerequisites**: .NET 8 SDK, Docker Desktop, PostgreSQL client tools
2. **Configuration**: Local settings in `appsettings.Development.json`
3. **Startup**: Run via .NET Aspire for orchestrated services
4. **Access Points**:
   - Web UI: https://localhost:5001
   - API: https://localhost:5000
   - Aspire Dashboard: https://localhost:15000

### Testing Approach

- **Unit Tests**: Domain logic and service methods
- **Integration Tests**: API endpoints with test containers
- **E2E Tests**: UI workflows with Playwright
- **Performance Tests**: Vector search and embedding generation

### Debugging Tools

- Aspire Dashboard for distributed tracing
- Structured logging with Serilog
- PostgreSQL query analysis
- Browser developer tools for Blazor

## Security Patterns

### Authentication & Authorization

- ASP.NET Core Identity for user management
- JWT tokens for API authentication
- Process-based authorization (users see only their executions)
- Scope-based data access control

### Data Protection

- Encryption at rest via PostgreSQL
- TLS for all network communication
- No sensitive data in logs
- User journey isolation

## Performance Optimization

### Caching Strategy

- Redis for frequently accessed metadata
- In-memory cache for static data
- Output caching for read-heavy endpoints
- Embedding cache to avoid recomputation

### Scaling Patterns

- Horizontal scaling for API instances
- Read replicas for database queries
- Background workers for embedding generation
- CDN for static assets

## Deployment Considerations

### Container Strategy

- Multi-stage Docker builds for optimization
- .NET Aspire for local orchestration
- Kubernetes manifests for production
- Health checks for all services

### Configuration Management

- Environment-specific settings
- Secret management via platform
- Feature flags for gradual rollout
- Telemetry configuration

### Monitoring & Observability

- Application Insights or OpenTelemetry
- Structured logging with correlation
- Performance counters
- Custom metrics for process execution

## API Design Principles

### RESTful Conventions

- Resource-based URLs
- Proper HTTP verbs
- Consistent response formats
- HATEOAS where appropriate

### Response Patterns

All API responses follow a consistent structure with success indicators, data payloads, and error information. Pagination is implemented for list endpoints.

### Versioning Strategy

- URL-based versioning (v1, v2)
- Backward compatibility commitment
- Deprecation notices in headers
- Migration guides for breaking changes

## Design Patterns

All implementations MUST follow the imperative patterns documented in [DESIGN-PATTERNS.md](./DESIGN-PATTERNS.md).

Key patterns include:
- Domain-Driven Design with aggregate boundaries
- Repository and Specification patterns  
- Result pattern for operation outcomes
- Process Context for execution state
- Adapter pattern for Cognitive System
- Unit of Work for transaction management
- CQRS for command/query separation

See [DESIGN-PATTERNS.md](./DESIGN-PATTERNS.md) for complete implementation details and code examples.