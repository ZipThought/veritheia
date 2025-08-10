# Design Patterns

## 1. Overview

This document specifies mandatory design patterns for Veritheia implementation. Each pattern emerges from first principles engineering—the natural consequences of our technology choices rather than imposed abstractions. We explicitly reject Domain-Driven Design (DDD) in favor of patterns that arise organically from the telos and ontology of our tech stack.

## 2. Core Principles

### 2.1 Single Source of Truth

Documentation serves as the authoritative specification. Each requirement exists in exactly one location. Implementation code includes explicit references to documentation sections. Deviations require documented rationale and approval.

### 2.2 Formation Preservation

Patterns preserve user intellectual sovereignty through journey-aware data access and provenance tracking. Performance optimizations cannot compromise formation principles.

### 2.3 First Principles Over Abstractions

We reject ceremonial patterns that abstract what is already abstracted. PostgreSQL with its constraints IS our domain model enforcer. Entity Framework Core IS our repository and unit of work. The patterns documented here emerge from the natural grain of these technologies, not from architectural fashion.

## 3. Why We Reject DDD

### The Philosophical Mismatch

Domain-Driven Design emerged from enterprise software where "business domains" have tangible meaning—orders, invoices, inventory. Veritheia has no business domain. It is epistemic infrastructure for journey-specific understanding formation. The attempt to force DDD patterns onto this reality creates impedance mismatch:

1. **No Universal Domain**: Each journey creates its own projection space with unique meaning
2. **Intelligence Lives Elsewhere**: LLMs hold the intelligence, not domain objects with behavior  
3. **PostgreSQL IS the Model**: Foreign keys, constraints, and types already enforce all rules
4. **EF Core IS the Repository**: DbContext and DbSet already implement these patterns

### What DDD Gets Wrong Here

```csharp
// DDD ANTI-PATTERN: Aggregate root with behavior
public class User : AggregateRoot
{
    private readonly List<Journey> _journeys = new();
    
    public Journey StartJourney(Process process, string purpose)
    {
        // This pretends the C# object enforces rules
        // But PostgreSQL foreign keys are the real enforcer
        var journey = new Journey(this, process, purpose);
        _journeys.Add(journey);
        return journey;
    }
}

// FIRST PRINCIPLES: Honest data structure
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public List<Journey> Journeys { get; set; } // PostgreSQL enforces the relationship
}
```

The DDD version adds ceremony without value. PostgreSQL's foreign key constraint `journeys.user_id REFERENCES users(id)` is the actual aggregate boundary. The C# class is merely an honest projection of database truth.

## 4. First Principles Patterns

### 4.1 Direct Data Access Through EF Core

Entity Framework Core already provides repository and unit of work patterns. We use it directly:

```csharp
public class JourneyService
{
    private readonly VeritheiaDbContext _db;
    
    public async Task<Journey> CreateJourney(Guid userId, string purpose)
    {
        // PostgreSQL enforces user exists via FK
        var journey = new Journey 
        { 
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Purpose = purpose,
            State = "Active",
            CreatedAt = DateTime.UtcNow
        };
        
        _db.Journeys.Add(journey);
        await _db.SaveChangesAsync(); // This IS the unit of work
        return journey;
    }
    
    public async Task<List<Journey>> GetActiveJourneys(Guid userId)
    {
        // Direct LINQ query - no abstraction needed
        return await _db.Journeys
            .Where(j => j.UserId == userId && j.State == "Active")
            .Include(j => j.Persona)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
    }
}
```

### 4.2 Journey-Scoped Queries as Extension Methods

For common query patterns, use extension methods instead of repository abstractions:

```csharp
public static class JourneyQueries
{
    public static IQueryable<JourneyDocumentSegment> ForJourney(
        this IQueryable<JourneyDocumentSegment> segments, 
        Guid journeyId)
    {
        return segments.Where(s => s.JourneyId == journeyId);
    }
    
    public static IQueryable<JourneyDocumentSegment> WithEmbeddings(
        this IQueryable<JourneyDocumentSegment> segments)
    {
        return segments.Include(s => s.SearchIndexes)
                      .ThenInclude(i => i.SearchVectors);
    }
}

// Usage
var segments = await _db.JourneyDocumentSegments
    .ForJourney(journeyId)
    .WithEmbeddings()
    .ToListAsync();
```

### 4.4 Simple Return Values

Services return domain entities directly. Journey tracking happens at the process/controller level:

```csharp
public class DocumentService
{
    private readonly VeritheiaDbContext _db;
    private readonly FileStorageService _files;
    
    public async Task<Document> ProcessDocument(string path, Guid userId)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"File not found: {path}");
            
        // Store file
        using var stream = File.OpenRead(path);
        var storagePath = await _files.SaveDocument(stream, Path.GetFileName(path));
        
        // Create document record
        var document = new Document
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            FileName = Path.GetFileName(path),
            FilePath = storagePath,
            FileSize = new FileInfo(path).Length,
            UploadedAt = DateTime.UtcNow
        };
        
        _db.Documents.Add(document);
        await _db.SaveChangesAsync();
        
        return document; // Simple return, no ceremony
    }
}
```

### 4.5 Process Context for Journey Awareness

Process execution carries journey context naturally:

```csharp
public class ProcessContext
{
    public Guid JourneyId { get; init; }
    public Guid UserId { get; init; }
    public Dictionary<string, object> Parameters { get; init; } = new();
    public CancellationToken CancellationToken { get; init; }
}

// Process uses context with direct database access
public class ScreeningProcess : IAnalyticalProcess
{
    private readonly VeritheiaDbContext _db;
    private readonly ICognitiveAdapter _cognitive;
    
    public async Task<ProcessResult> ExecuteAsync(ProcessContext context)
    {
        // Get journey directly
        var journey = await _db.Journeys
            .Include(j => j.Persona)
            .FirstOrDefaultAsync(j => j.Id == context.JourneyId);
            
        if (journey == null)
            throw new InvalidOperationException($"Journey {context.JourneyId} not found");
            
        // Process documents in journey's projection space
        var segments = await _db.JourneyDocumentSegments
            .Where(s => s.JourneyId == context.JourneyId)
            .ToListAsync();
            
        // ... processing logic
        
        return new ProcessResult { Success = true, Data = results };
    }
}
```

### 4.8 Cognitive Adapter (The One True Abstraction)

The cognitive adapter is a necessary abstraction because it wraps external AI services:

```csharp
public interface ICognitiveAdapter
{
    Task<float[]> CreateEmbedding(string text);
    Task<AssessmentResult> AssessAsync(string content, string criteria);
    int MaxContextTokens { get; }
}

public record AssessmentResult(double Score, string Reasoning);

// Implementation wraps external service
public class SemanticKernelAdapter : ICognitiveAdapter
{
    private readonly Kernel _kernel;
    
    public int MaxContextTokens => 128000;
    
    public async Task<float[]> CreateEmbedding(string text)
    {
        // This abstracts OpenAI/Claude/local models
        var result = await _kernel.InvokeAsync("embedding", text);
        return result.ToArray();
    }
    
    public async Task<AssessmentResult> AssessAsync(string content, string criteria)
    {
        // Measurement within journey's projection space
        var prompt = $"Assess this content: {content}\nAgainst criteria: {criteria}";
        var result = await _kernel.InvokeAsync("assessment", prompt);
        
        return new AssessmentResult(
            Score: ParseScore(result),
            Reasoning: ParseReasoning(result)
        );
    }
}
```

This abstraction has value because:
1. AI services are external resources (like files)
2. Multiple providers exist (OpenAI, Anthropic, local)
3. The interface is stable while implementations vary
```

### 4.3 File Storage Service

The only true "repository" pattern we need - for resources outside PostgreSQL:

```csharp
public class FileStorageService
{
    private readonly string _rootPath;
    
    public async Task<string> SaveDocument(Stream content, string fileName)
    {
        var path = Path.Combine(_rootPath, "documents", fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        
        using var fs = new FileStream(path, FileMode.Create);
        await content.CopyToAsync(fs);
        return path;
    }
    
    public async Task<Stream> GetDocument(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Document not found: {path}");
            
        return new FileStream(path, FileMode.Open, FileAccess.Read);
    }
}
```

This is a repository pattern because it abstracts external storage (filesystem, S3, etc.) that PostgreSQL cannot handle.

### 4.6 Command/Query Separation (When Complexity Warrants)

CQRS only when operations become complex enough to benefit:

```csharp
// Simple operations - just use service methods
public class JourneyService
{
    public async Task<Journey> GetJourney(Guid id) { ... }
    public async Task<Journey> CreateJourney(string purpose) { ... }
}

// Complex operations - CQRS helps organize
public record ComplexSearchQuery(
    string[] SearchTerms,
    Guid[] JourneyIds,
    DateTime? Since,
    int PageNumber = 1,
    int PageSize = 20);

public class ComplexSearchHandler
{
    private readonly VeritheiaDbContext _db;
    
    public async Task<PagedResult> Handle(ComplexSearchQuery query)
    {
        // Complex query logic benefits from separation
        var baseQuery = _db.JourneyDocumentSegments
            .Where(s => query.JourneyIds.Contains(s.JourneyId));
            
        if (query.Since.HasValue)
            baseQuery = baseQuery.Where(s => s.CreatedAt >= query.Since.Value);
            
        // ... more complex filtering
        
        return new PagedResult { ... };
    }
}

// Implementation
public class GetDocumentsBySearchQueryHandler : IQueryHandler<GetDocumentsBySearchQuery, PagedResult<DocumentDto>>
{
    private readonly VeritheiaDbContext _db;
    
    public async Task<PagedResult<DocumentDto>> HandleAsync(
        GetDocumentsBySearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var baseQuery = _db.Documents
            .Where(d => d.FileName.Contains(query.SearchTerm));
            
        var documents = await baseQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(d => new DocumentDto
            {
                Id = d.Id,
                FileName = d.FileName,
                FilePath = d.FilePath
            })
            .ToListAsync(cancellationToken);
            
        var count = await baseQuery.CountAsync(cancellationToken);
        
        return new PagedResult<DocumentDto>(
            documents,
            count,
            query.PageNumber,
            query.PageSize);
    }
}
```

### 4.9 Process Implementation

Processes operate within journey projection spaces, using direct database access:

```csharp
public class SystematicScreeningProcess : IAnalyticalProcess
{
    private readonly VeritheiaDbContext _db;
    private readonly ICognitiveAdapter _cognitive;
    
    public async Task<ProcessResult> ExecuteAsync(ProcessContext context)
    {
        // Get journey-projected segments directly
        var segments = await _db.JourneyDocumentSegments
            .Where(s => s.JourneyId == context.JourneyId)
            .Include(s => s.Document)
            .ToListAsync();
        
        var assessments = new List<object>();
        
        foreach (var segment in segments)
        {
            // AI measures within journey's projection space
            var assessment = await _cognitive.AssessAsync(
                segment.SegmentContent,
                context.Parameters["ResearchQuestions"] as string);
                
            assessments.Add(new
            {
                SegmentId = segment.Id,
                Relevance = assessment.Score,
                Rationale = assessment.Reasoning
            });
        }
        
        // Return simple result - journey tracking at controller level
        return new ProcessResult
        {
            Success = true,
            Data = new Dictionary<string, object>
            {
                ["assessments"] = assessments,
                ["segmentCount"] = segments.Count
            }
        };
    }
}
```

### 4.7 PostgreSQL as Domain Enforcer

The database schema IS the domain model. Constraints enforce all business rules:

```sql
-- PostgreSQL enforces aggregate boundaries
CREATE TABLE journeys (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id),
    persona_id UUID NOT NULL REFERENCES personas(id),
    purpose TEXT NOT NULL,
    state VARCHAR(50) NOT NULL CHECK (state IN ('Active', 'Paused', 'Completed')),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- PostgreSQL enforces value object invariants
CREATE TABLE search_vectors_1536 (
    id UUID PRIMARY KEY,
    embedding vector(1536) NOT NULL,  -- pgvector enforces dimensions
    CHECK (vector_dims(embedding) = 1536)
);

-- PostgreSQL enforces journey projection boundaries
CREATE TABLE journey_document_segments (
    id UUID PRIMARY KEY,
    journey_id UUID NOT NULL REFERENCES journeys(id) ON DELETE CASCADE,
    document_id UUID NOT NULL REFERENCES documents(id),
    -- Segment only exists within journey context
    UNIQUE(journey_id, document_id, sequence_index)
);
```

The C# entities are honest projections of this truth:

```csharp
public class Journey
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PersonaId { get; set; }
    public string Purpose { get; set; }
    public string State { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties let EF Core follow the relationships
    public User User { get; set; }
    public Persona Persona { get; set; }
    public List<JourneyDocumentSegment> Segments { get; set; }
}
```
```

## 5. Cross-Cutting Patterns That Remain Useful

### 5.1 Logging Pattern
```csharp
public class DocumentService
{
    private readonly ILogger<DocumentService> _logger;
    
    public async Task<Result<Document>> ProcessAsync(string path)
    {
        using var _ = _logger.BeginScope(new Dictionary<string, object>
        {
            ["FilePath"] = path,
            ["CorrelationId"] = Guid.NewGuid()
        });
        
        _logger.LogInformation("Starting document processing");
        
        try
        {
            var result = await ProcessDocumentInternalAsync(path);
            _logger.LogInformation("Document processed successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Document processing failed");
            throw;
        }
    }
}
```

### 5.2 Validation (When Truly Complex)
```csharp
public class CreateJourneyCommandValidator : AbstractValidator<CreateJourneyCommand>
{
    public CreateJourneyCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");
            
        RuleFor(x => x.Purpose)
            .NotEmpty()
            .WithMessage("Journey purpose is required")
            .MaximumLength(500)
            .WithMessage("Purpose must not exceed 500 characters");
            
        RuleFor(x => x.ProcessId)
            .NotEmpty()
            .WithMessage("Process ID is required")
            .MustAsync(ProcessExists)
            .WithMessage("Process does not exist");
    }
    
    private async Task<bool> ProcessExists(Guid processId, CancellationToken cancellationToken)
    {
        // Check process registry
        return true;
    }
}
```

### 5.3 Error Handling
```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (NotFoundException ex)
        {
            await HandleNotFoundExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleGenericExceptionAsync(context, ex);
        }
    }
    
    private async Task HandleValidationExceptionAsync(HttpContext context, ValidationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        
        var problemDetails = new ValidationProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Validation error",
            Status = StatusCodes.Status400BadRequest,
            Detail = "One or more validation errors occurred",
            Instance = context.Request.Path
        };
        
        foreach (var error in ex.Errors)
        {
            problemDetails.Errors.Add(error.PropertyName, new[] { error.ErrorMessage });
        }
        
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
```

## 6. The Philosophical Foundation

### Why First Principles Over DDD

The rejection of DDD is not arbitrary—it's recognition of fundamental conflict. DDD patterns require translation layers that fight against the natural patterns of PostgreSQL, EF Core, and C#. This creates impedance mismatch:

1. **DDD wants aggregate roots with behavior** → PostgreSQL already enforces aggregates through foreign keys
2. **DDD wants repositories abstracting persistence** → EF Core DbContext/DbSet already ARE repositories
3. **DDD wants domain objects with logic** → Intelligence lives in LLMs, not C# methods
4. **DDD assumes stable business domain** → Veritheia has journey-specific projection spaces

Rather than build translation layers between incompatible paradigms, we reject DDD and embrace what our stack naturally provides.

### The Stack as Philosophy

Our technology choices embody our philosophy:

- **PostgreSQL**: The immutable ground truth, enforcing relational integrity
- **pgvector**: Semantic space where meaning can be measured but not generated
- **EF Core**: Honest projection of database truth into runtime objects
- **C# records**: Immutable value carriers, not behavior containers
- **Journey projections**: User-authored frameworks that give documents meaning

Each technology was chosen for what it IS, not what it could be abstracted to become.

### Praxis from Ontology

The patterns in this document are not imposed but discovered—they emerge from working WITH the grain of our stack:

- Direct database access because EF Core already provides the abstraction
- Extension methods for queries because LINQ naturally supports composition
- File storage service because filesystem/S3 are external to PostgreSQL
- Cognitive adapter because AI services are external and varied

This is praxis arising from ontology—practical patterns emerging from the essential nature of our tools.

## Implementation Checklist

When implementing any feature, verify:

- [ ] PostgreSQL constraints enforce data integrity (not C# code)
- [ ] All entities use Guid.CreateVersion7() for primary keys
- [ ] Services use VeritheiaDbContext directly (no repository abstraction)
- [ ] Complex queries use LINQ with appropriate Includes
- [ ] Operations return simple types or domain entities
- [ ] Process execution uses ProcessContext for journey awareness
- [ ] Cognitive operations go through ICognitiveAdapter
- [ ] Transactions use DbContext's built-in transaction support
- [ ] Commands and queries are separated only when complexity warrants
- [ ] File operations use FileStorageService
- [ ] Patterns emerge from tech stack, not architectural fashion
- [ ] Every abstraction has clear value beyond what the stack provides

Remember: First principles engineering means understanding what each technology provides and using it for that purpose. DDD patterns are in fundamental conflict with the design patterns inherent in PostgreSQL, EF Core, and C#. Rather than force translation layers between incompatible paradigms, we reject DDD outright and work directly with the natural patterns our stack provides. Additional abstractions must earn their complexity by providing clear value beyond what these tools naturally offer.