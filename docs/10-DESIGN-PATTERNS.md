# Veritheia Design Patterns

## I. Philosophical Foundation

This document specifies the design patterns that emerge from Veritheia's architectural commitments. These are not patterns imposed from industry fashion but natural consequences of our technology choices and philosophical stance. The patterns include explicit extension points because institutions, organizations, and research teams extend from this open source foundation for their specific collaborative and distributed needs.

We embrace Domain-Driven Design's core principle—that software should model the problem domain with precision—and its goal of maintaining model integrity through explicit boundaries. However, we reject DDD's implementation practices entirely. No repositories hiding the database. No aggregate roots pretending to enforce rules that PostgreSQL already enforces. No value objects when C# records suffice. The schema IS the domain model. The constraints ARE the business rules. Entity Framework Core provides direct projection of database truth into runtime, nothing more.

Every pattern documented here serves the architectural principles established in 03-ARCHITECTURE.md: user sovereignty through partition boundaries, intellectual formation through projection spaces, anti-surveillance through structural design, and testing through real execution rather than mocked abstractions.

> **Formation Note:** These patterns emerge from the recognition that PostgreSQL with pgvector IS our domain model. We don't abstract what is already abstracted. We partition by user, project by journey, and measure within conceptual spaces because these patterns preserve the possibility of genuine user understanding.

## II. Core Invariants

### User Partition Boundaries

Every significant query begins with user_id. This is not a convention but an invariant. The database partitions naturally along user boundaries, and every query must respect this partitioning.

```csharp
// CORRECT: Query scoped to user partition
var documents = await _db.Documents
    .Where(d => d.UserId == userId)
    .Where(d => d.UploadedAt > since)
    .ToListAsync();

// VIOLATION: Cross-partition query without explicit authorization
var allDocuments = await _db.Documents
    .Where(d => d.UploadedAt > since)
    .ToListAsync(); // This violates partition boundaries
```

Cross-partition operations require explicit bridges with audit trails. They are never accidental.

### Journey Projection Scope

Within a user's partition, most operations scope further to a specific journey. Documents gain meaning only through journey projection. Segments exist only within journey context. Assessments measure only against journey criteria.

```csharp
// CORRECT: Journey-scoped query
var segments = await _db.JourneyDocumentSegments
    .Where(s => s.JourneyId == journeyId)
    .Include(s => s.Document)
    .ToListAsync();

// VIOLATION: Accessing segments without journey context
var segments = await _db.JourneyDocumentSegments
    .Where(s => s.Document.UserId == userId)
    .ToListAsync(); // Segments without journey context are meaningless
```

### Direct Schema Projection

Entity Framework Core maps directly to the database schema. No repository abstractions. No unit of work wrappers. The DbContext IS the unit of work. The DbSet IS the repository.

```csharp
// CORRECT: Direct use of DbContext
public class JourneyService
{
    private readonly VeritheiaDbContext _db;
    
    public async Task<Journey> CreateJourney(Guid userId, Guid personaId, string purpose)
    {
        var journey = new Journey
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            PersonaId = personaId,
            Purpose = purpose,
            State = "Active",
            CreatedAt = DateTime.UtcNow
        };
        
        _db.Journeys.Add(journey);
        await _db.SaveChangesAsync(); // Direct transaction
        return journey;
    }
}

// VIOLATION: Repository abstraction over DbContext
public interface IJourneyRepository
{
    Task<Journey> CreateAsync(Journey journey);
}
// This adds no value - DbContext already provides this
```

## III. Data Access Patterns

### Query Extension Methods

Common query patterns become extension methods, not repository methods. This preserves composability while avoiding abstraction.

```csharp
public static class QueryExtensions
{
    // User partition enforcement
    public static IQueryable<T> ForUser<T>(
        this IQueryable<T> query, 
        Guid userId) where T : IUserOwned
    {
        return query.Where(e => e.UserId == userId);
    }
    
    // Journey scope enforcement
    public static IQueryable<JourneyDocumentSegment> ForJourney(
        this IQueryable<JourneyDocumentSegment> segments,
        Guid journeyId)
    {
        return segments.Where(s => s.JourneyId == journeyId);
    }
    
    // Projection space navigation
    public static IQueryable<JourneyDocumentSegment> WithAssessments(
        this IQueryable<JourneyDocumentSegment> segments)
    {
        return segments.Include(s => s.Assessments);
    }
}

// Usage composes naturally
var relevantSegments = await _db.JourneyDocumentSegments
    .ForJourney(journeyId)
    .WithAssessments()
    .Where(s => s.Assessments.Any(a => a.RelevanceScore > 0.7))
    .ToListAsync();
```

### Service Layer Pattern

Services orchestrate operations within partition and journey boundaries. They use DbContext directly, return domain entities, and let PostgreSQL enforce constraints.

```csharp
public class DocumentService
{
    private readonly VeritheiaDbContext _db;
    private readonly IFileStorage _files;
    private readonly ILogger<DocumentService> _logger;
    
    public async Task<Document> IngestDocument(
        Guid userId,
        Stream content,
        string fileName,
        Guid? scopeId = null)
    {
        // Validate user exists (FK will enforce)
        var userExists = await _db.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
            throw new InvalidOperationException($"User {userId} not found");
        
        // Store file content
        var storagePath = await _files.StoreAsync(content, fileName);
        
        // Create document in user's partition
        var document = new Document
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,  // Partition key
            ScopeId = scopeId,
            FileName = fileName,
            FilePath = storagePath,
            FileSize = content.Length,
            UploadedAt = DateTime.UtcNow
        };
        
        _db.Documents.Add(document);
        await _db.SaveChangesAsync(); // PostgreSQL enforces all constraints
        
        _logger.LogInformation(
            "Document {DocumentId} ingested for user {UserId}",
            document.Id, userId);
        
        return document;
    }
}
```

### Process Context Pattern

Processes operate within journey projection spaces. The ProcessContext carries all necessary scope information.

```csharp
public class ProcessContext
{
    public Guid UserId { get; init; }
    public Guid JourneyId { get; init; }
    public Guid PersonaId { get; init; }
    public Dictionary<string, object> Parameters { get; init; }
    public string JournalContext { get; init; } // Assembled narrative
    public CancellationToken CancellationToken { get; init; }
}

public interface IAnalyticalProcess
{
    Task<ProcessResult> ExecuteAsync(ProcessContext context);
}

public class SystematicScreeningProcess : IAnalyticalProcess
{
    private readonly VeritheiaDbContext _db;
    private readonly ICognitiveAdapter _cognitive;
    
    public async Task<ProcessResult> ExecuteAsync(ProcessContext context)
    {
        // All queries naturally scoped to journey
        var segments = await _db.JourneyDocumentSegments
            .Where(s => s.JourneyId == context.JourneyId)
            .Include(s => s.Document)
            .ToListAsync(context.CancellationToken);
        
        // Process within projection space
        foreach (var segment in segments)
        {
            // AI measures within journey's conceptual framework
            var assessment = await _cognitive.AssessAsync(
                segment.SegmentContent,
                context.Parameters["ResearchQuestions"].ToString(),
                context.JournalContext);
            
            // Store assessment in journey context
            var record = new JourneySegmentAssessment
            {
                Id = Guid.CreateVersion7(),
                SegmentId = segment.Id,
                AssessmentType = "Relevance",
                Score = assessment.Score,
                Reasoning = assessment.Reasoning,
                AssessedAt = DateTime.UtcNow
            };
            
            _db.JourneySegmentAssessments.Add(record);
        }
        
        await _db.SaveChangesAsync();
        
        return new ProcessResult
        {
            Success = true,
            Data = new { SegmentCount = segments.Count }
        };
    }
}
```

## IV. External Service Patterns

### File Storage Abstraction

File storage is external to PostgreSQL, therefore it warrants abstraction.

```csharp
public interface IFileStorage
{
    Task<string> StoreAsync(Stream content, string fileName);
    Task<Stream> RetrieveAsync(string path);
    Task DeleteAsync(string path);
}

public class LocalFileStorage : IFileStorage
{
    private readonly string _basePath;
    
    public async Task<string> StoreAsync(Stream content, string fileName)
    {
        var path = Path.Combine(_basePath, Guid.NewGuid().ToString(), fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        
        using var file = File.Create(path);
        await content.CopyToAsync(file);
        
        return path; // Return path for database storage
    }
}
```

### Cognitive Adapter Pattern

AI services are external and varied, requiring abstraction.

```csharp
public interface ICognitiveAdapter
{
    int MaxContextTokens { get; }
    Task<float[]> CreateEmbeddingAsync(string text, string context);
    Task<AssessmentResult> AssessAsync(string content, string criteria, string context);
}

public record AssessmentResult(double Score, string Reasoning);

public class OpenAIAdapter : ICognitiveAdapter
{
    public int MaxContextTokens => 128000;
    
    public async Task<float[]> CreateEmbeddingAsync(string text, string context)
    {
        // Embedding includes journey context
        var contextualText = $"{context}\n\n{text}";
        return await CallOpenAIEmbeddingAPI(contextualText);
    }
    
    public async Task<AssessmentResult> AssessAsync(
        string content, 
        string criteria, 
        string context)
    {
        // Assessment within journey's projection space
        var prompt = BuildAssessmentPrompt(content, criteria, context);
        var response = await CallOpenAICompletionAPI(prompt);
        return ParseAssessmentResult(response);
    }
}
```

## V. Query Optimization Patterns

### Direct Service Methods for Most Operations

Services expose methods that map naturally to operations. Database updates are inherently commands. Queries return projections of the normalized model.

```csharp
public class PersonaService
{
    private readonly VeritheiaDbContext _db;
    
    public async Task<Persona> GetActivePersona(Guid userId, string domain)
    {
        return await _db.Personas
            .Where(p => p.UserId == userId)
            .Where(p => p.Domain == domain)
            .Where(p => p.IsActive)
            .FirstOrDefaultAsync();
    }
    
    public async Task<Persona> CreatePersona(Guid userId, string domain)
    {
        var persona = new Persona
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Domain = domain,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        _db.Personas.Add(persona);
        await _db.SaveChangesAsync();
        
        return persona;
    }
}
```

### Query Objects for Complex Optimization

When performance requires selective denormalization or complex query optimization, encapsulate in query objects. This is CQRS as optimization technique, not architectural pattern.

```csharp
// Complex search with many parameters
public record DocumentSearchQuery(
    Guid UserId,
    Guid? JourneyId,
    string[] SearchTerms,
    DateTime? Since,
    DateTime? Until,
    int PageNumber,
    int PageSize);

public class DocumentSearchHandler
{
    private readonly VeritheiaDbContext _db;
    
    public async Task<PagedResult<Document>> HandleAsync(DocumentSearchQuery query)
    {
        var baseQuery = _db.Documents
            .Where(d => d.UserId == query.UserId); // Partition boundary
        
        if (query.JourneyId.HasValue)
        {
            // Further scope to journey
            var documentIds = await _db.JourneyDocumentSegments
                .Where(s => s.JourneyId == query.JourneyId.Value)
                .Select(s => s.DocumentId)
                .Distinct()
                .ToListAsync();
            
            baseQuery = baseQuery.Where(d => documentIds.Contains(d.Id));
        }
        
        if (query.Since.HasValue)
            baseQuery = baseQuery.Where(d => d.UploadedAt >= query.Since.Value);
            
        if (query.Until.HasValue)
            baseQuery = baseQuery.Where(d => d.UploadedAt <= query.Until.Value);
        
        // Complex term matching
        if (query.SearchTerms?.Any() == true)
        {
            foreach (var term in query.SearchTerms)
                baseQuery = baseQuery.Where(d => d.FileName.Contains(term));
        }
        
        var totalCount = await baseQuery.CountAsync();
        
        var documents = await baseQuery
            .OrderByDescending(d => d.UploadedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();
        
        return new PagedResult<Document>(
            documents, totalCount, query.PageNumber, query.PageSize);
    }
}
```

## VI. Testing Patterns

### No Internal Mocking

Tests run against real PostgreSQL instances. Respawn resets state between tests.

```csharp
public class JourneyServiceTests : IAsyncLifetime
{
    private VeritheiaDbContext _db;
    private Respawner _respawner;
    
    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<VeritheiaDbContext>()
            .UseNpgsql("Host=localhost;Database=veritheia_test;Username=test;Password=test")
            .Options;
            
        _db = new VeritheiaDbContext(options);
        await _db.Database.EnsureCreatedAsync();
        
        _respawner = await Respawner.CreateAsync(
            _db.Database.GetConnectionString(),
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = new[] { "public" }
            });
    }
    
    public async Task DisposeAsync()
    {
        await _respawner.ResetAsync(_db.Database.GetConnectionString());
        await _db.DisposeAsync();
    }
    
    [Fact]
    public async Task CreateJourney_EnforcesPersonaExists()
    {
        // Arrange
        var service = new JourneyService(_db);
        var userId = Guid.CreateVersion7();
        var personaId = Guid.CreateVersion7(); // Non-existent
        
        // Act & Assert
        // PostgreSQL foreign key constraint prevents this
        await Assert.ThrowsAsync<DbUpdateException>(
            () => service.CreateJourney(userId, personaId, "Test Purpose"));
    }
}
```

### Mock Only External Services

External services (AI, file storage) can be mocked.

```csharp
public class ScreeningProcessTests
{
    [Fact]
    public async Task Process_MeasuresWithinJourneyProjection()
    {
        // Arrange
        var mockCognitive = new Mock<ICognitiveAdapter>();
        mockCognitive.Setup(c => c.AssessAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new AssessmentResult(0.8, "Highly relevant"));
        
        var process = new SystematicScreeningProcess(_db, mockCognitive.Object);
        
        // Act
        var result = await process.ExecuteAsync(context);
        
        // Assert
        mockCognitive.Verify(c => c.AssessAsync(
            It.IsAny<string>(),
            It.Is<string>(s => s.Contains("research question")),
            It.Is<string>(s => s.Contains("journey context"))),
            Times.AtLeastOnce());
    }
}
```

## VII. Primary Key Strategy

All entities use UUIDv7 for temporal ordering without sequence management.

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
```

## VIII. Anti-Patterns to Avoid

### Repository Over DbContext
```csharp
// ANTI-PATTERN: Unnecessary abstraction
public interface IRepository<T>
{
    Task<T> GetByIdAsync(Guid id);
    Task<T> AddAsync(T entity);
}

// DbContext already provides this
```

### Business Logic in Entities
```csharp
// ANTI-PATTERN: Entity with behavior
public class Journey
{
    public void Complete()
    {
        if (State != "Active")
            throw new InvalidOperationException();
        State = "Completed";
    }
}

// PostgreSQL check constraint already enforces valid state transitions
```

### Generic Queries Without Partition Scope
```csharp
// ANTI-PATTERN: Ignoring partition boundaries
public async Task<List<Document>> SearchDocuments(string term)
{
    return await _db.Documents
        .Where(d => d.FileName.Contains(term))
        .ToListAsync();
}

// Must scope to user partition
```

### Mocking Internal Services
```csharp
// ANTI-PATTERN: Mocking DbContext
var mockDb = new Mock<VeritheiaDbContext>();

// Test against real database with Respawn
```

## IX. Implementation Checklist

When implementing any feature:

- [ ] All queries begin with user_id partition scope
- [ ] Journey operations include journey_id in WHERE clause
- [ ] Services use VeritheiaDbContext directly, no repository abstraction
- [ ] External services (AI, files) use interface abstractions
- [ ] Primary keys use Guid.CreateVersion7()
- [ ] Tests run against real PostgreSQL with Respawn
- [ ] Only external services are mocked in tests
- [ ] Cross-partition operations have explicit authorization checks
- [ ] Process execution uses ProcessContext for scope
- [ ] Complex queries use extension methods, not repositories
- [ ] PostgreSQL constraints enforce business rules, not C# code
- [ ] Web components use RenderContext, never call services directly
- [ ] Components declare data needs before render cycle
- [ ] Single database operation per render cycle

## X. Web Layer Patterns

### Demand-Driven Context Pattern

Component-based user interfaces face a fundamental architectural problem: when multiple components independently fetch data during the same render cycle, they create concurrent database operations that violate single-threaded database context assumptions. The traditional solution—having parent components fetch all data and pass it down—violates component independence by forcing parents to know what children need. The demand-driven context pattern resolves this tension by separating demand declaration from data fetching.

The pattern recognizes that component trees have natural traversal points where the framework visits each component in sequence. During this traversal, components declare what data they need without fetching it. After all components have declared their demands, a single bulk operation fetches all required data. Components then consume this pre-loaded data synchronously during render.

```csharp
public class RenderContext
{
    private readonly IServiceProvider _services;
    private readonly HashSet<string> _demands = new();
    private Dictionary<string, object> _data = new();
    private bool _initialized = false;
    
    public void Require(string demand) => _demands.Add(demand);
    
    public T Get<T>(string key) => (T)_data[key];
    
    public async Task InitializeAsync()
    {
        if (_initialized) return;
        
        // Single database operation fetches all demanded data
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DbContext>();
        
        // Bulk load based on accumulated demands
        if (_demands.Contains("user"))
            _data["user"] = await db.Users.FindAsync(userId);
            
        if (_demands.Contains("journeys"))
            _data["journeys"] = await db.Journeys
                .Where(j => j.UserId == userId)
                .ToListAsync();
                
        _initialized = true;
    }
}
```

This context accumulates demands during component initialization, executes a single database operation after all demands are registered, then provides synchronous access to loaded data during render. The pattern works because component initialization happens before data fetching, which happens before rendering.

The anti-patterns this avoids are instructive. Direct service calls from components create the concurrent database access problem. Cascading values from parents violate component independence. Per-component caching creates chaos when the same data is cached differently in different components. The demand-driven context avoids all three by centralizing data fetching while preserving component independence through demand declaration.

## XI. Closing Principle

These patterns emerge from the recognition that PostgreSQL with pgvector IS our domain model, Entity Framework Core IS our data access layer, and the schema IS the source of truth. We do not abstract what is already abstracted. We do not mock what must be real. We partition by user, project by journey, and measure within conceptual spaces.

> **Formation Note:** Every pattern documented here exists to ensure that when users engage with Veritheia, they are authoring their own understanding, not consuming system-generated intelligence. The architectural patterns are the mechanical enforcement of this philosophical commitment.

Every pattern serves the architectural commitment: intellectual sovereignty through structural design, not policy decoration.