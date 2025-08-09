# Veritheia Implementation

## 1. Overview

This document specifies the technical implementation of Veritheia. The system operates as a local-first epistemic infrastructure with four primary components: PostgreSQL with pgvector for journey-specific projection spaces, ASP.NET Core for process orchestration, adapter-based LLM integration for journey-calibrated assessments, and Blazor Server for user interfaces. All components enforce user data sovereignty and ensure insights emerge from user engagement within their projection spaces.


## 2. Technology Stack

### 2.1 Knowledge Database

**PostgreSQL 17** with pgvector extension provides unified storage for documents, metadata, and embeddings, running in Docker containers orchestrated by .NET Aspire. The pgvector extension enables efficient similarity search over 1536-dimensional embedding vectors while maintaining ACID guarantees for relational data. The system uses PostgreSQL 17 (upgradeable to 18 when released) via the `pgvector/pgvector:17-pg17` Docker image, ensuring consistent environments across development and production.

### 2.2 Process Engine

**ASP.NET Core 9.0** (STS release) implements the Process Engine as a RESTful API service. We use .NET 9 specifically for native UUIDv7 support via `Guid.CreateVersion7()`, providing time-ordered primary keys without custom implementations. The architecture employs Domain-Driven Design with aggregate boundaries around User, Journey, and Document entities. Data access uses the Repository pattern with Entity Framework Core 9.0, while CQRS separates read and write operations for scalability.

**Note on .NET 9**: This is a Standard Term Support (STS) release, not Long Term Support (LTS). We accept this trade-off for the native UUIDv7 implementation which is critical for our temporal ordering requirements.

### 2.3 Presentation Tier

Blazor Server provides the web interface, enabling real-time updates through SignalR connections. This architecture choice eliminates JavaScript complexity while maintaining responsive user experiences. Component design follows a strict separation between user-authored content display and system-provided structure.

### 2.4 Cognitive System Integration

The ICognitiveAdapter interface abstracts LLM implementation details, supporting multiple backends: LlamaCppAdapter for local inference, SemanticKernelAdapter for Microsoft Semantic Kernel, and OpenAIAdapter for cloud-based models. Each adapter performs journey-calibrated assessments using journey-specific prompts and criteria, ensuring assessments occur within the user's projection space rather than generic processing.

## Data Architecture

### Entity Model: Journey Projection Architecture

The data layer (`veritheia.Data`) implements journey-specific projection spaces:

**Core Entities**:
- **User**: Core identity with evolving persona
- **Journey**: User's engagement instance with a process
- **JourneyFramework**: Defines how this journey projects documents

**Projection Entities**:
- **Document**: Raw corpus materials (unchanged originals)
- **JourneyDocumentSegment**: Document projected into journey-specific segments
- **SearchIndex**: Metadata for segment embeddings
- **SearchVector_{dimension}**: Polymorphic vector storage by dimension
- **JourneySegmentAssessment**: Journey-specific relevance/contribution scores
- **JourneyFormation**: Accumulated insights from the journey

**Supporting Entities**:
- **KnowledgeScope**: Organizational boundaries for documents
- **ProcessDefinition**: Available process types
- **ProcessExecution**: Process run tracking
- **ProcessResult**: Process outputs
- **Journal**: Narrative record (Research, Method, Decision, Reflection)
- **JournalEntry**: Individual narrative entries

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

**Implementation Example**:
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

// Entity Framework configuration
modelBuilder.Entity<MyEntity>()
    .Property(e => e.Id)
    .HasColumnType("uuid"); // Direct mapping, no converter needed
```

Previous specification (ULID) was reconsidered through dialectical investigation:
- UUIDv7 provides same temporal ordering benefits with native type support
- Eliminates string conversion overhead (16 bytes binary vs 26 bytes string)
- RFC 9562 standardization ensures long-term stability

### Database Design Patterns

- **Repository Pattern**: Generic `IRepository<T>` with concrete implementations
- **Unit of Work**: Transaction management across repositories
- **Specification Pattern**: Complex queries via `ISpecification<T>`
- **Value Converters**: UTC DateTime, JSONB for PostgreSQL
- **Soft Deletes**: Logical deletion with `DeletedAt` timestamp
- **Auditing**: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy on all entities
- **Query Optimization**: 
  - Use `.AsNoTracking()` for ALL read operations
  - Use change tracking only for domain aggregate updates
  - Compile frequently-used queries for performance
- **Vector Queries**: Use `FromSqlRaw()` for pgvector operations with `<->` operator
- **Journey Scoping**: All queries filtered by journey context
- **Formation Tracking**: Persist accumulated insights per journey

### Vector Storage: Polymorphic Table Architecture

**Design**: Separate tables per vector dimension with metadata in `search_indexes`.

**Structure**:
```csharp
// Metadata tracks all embeddings
public class SearchIndex : BaseEntity
{
    public Guid SegmentId { get; set; }
    public string VectorModel { get; set; } // "openai-ada-002", "e5-large-v2"
    public int VectorDimension { get; set; } // 1536, 768, 384
    public DateTime IndexedAt { get; set; }
    
    public JourneyDocumentSegment Segment { get; set; }
}

// Dimension-specific storage
public class SearchVector1536
{
    public Guid IndexId { get; set; }
    public float[] Embedding { get; set; } // Maps to vector(1536)
    
    public SearchIndex Index { get; set; }
}
```

**Index Strategy**: HNSW (Hierarchical Navigable Small World)
- Better query performance than IVFFlat (O(log n) vs O(√n))
- Can be created on empty tables (no data required)
- Parameters: m=16 (bi-directional links), ef_construction=64 (build quality)

**Query Pattern**:
```csharp
public async Task<IEnumerable<SearchResult>> SearchSimilar(
    Guid journeyId, float[] queryVector, string model, int limit = 10)
{
    var dimension = queryVector.Length;
    var vectorTable = $"search_vectors_{dimension}";
    
    var sql = $@"
        SELECT s.id, s.segment_content, v.embedding <-> @query as distance
        FROM journey_document_segments s
        JOIN search_indexes si ON si.segment_id = s.id
        JOIN {vectorTable} v ON v.index_id = si.id
        WHERE s.journey_id = @journeyId 
          AND si.vector_model = @model
        ORDER BY distance
        LIMIT @limit";
    
    return await _context.Database
        .SqlQueryRaw<SearchResult>(sql, 
            new NpgsqlParameter("@query", queryVector),
            new NpgsqlParameter("@journeyId", journeyId),
            new NpgsqlParameter("@model", model),
            new NpgsqlParameter("@limit", limit))
        .AsNoTracking()
        .ToListAsync();
}
```

### Database Migrations with pgvector

**Workflow**:
1. Define entity changes in `veritheia.Data`
2. Generate migration: `dotnet ef migrations add MigrationName`
3. Add raw SQL for PostgreSQL-specific features
4. Apply: Auto in development, explicit in production

**Initial Migration Example**:
```csharp
public partial class InitialProjectionSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Enable pgvector extension
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS vector;");
        
        // Create journey projection tables
        migrationBuilder.CreateTable(
            name: "journey_document_segments",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                journey_id = table.Column<Guid>(type: "uuid", nullable: false),
                document_id = table.Column<Guid>(type: "uuid", nullable: false),
                segment_content = table.Column<string>(type: "text", nullable: false),
                // ... other columns
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_journey_document_segments", x => x.id);
                // ... foreign keys
            });
        
        // Create vector tables and HNSW indexes
        migrationBuilder.Sql(@"
            CREATE TABLE search_vectors_1536 (
                index_id UUID PRIMARY KEY REFERENCES search_indexes(id),
                embedding vector(1536) NOT NULL
            );
            
            CREATE INDEX idx_vectors_1536_hnsw 
            ON search_vectors_1536 
            USING hnsw (embedding vector_cosine_ops)
            WITH (m = 16, ef_construction = 64);
        ");
    }
}
```

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