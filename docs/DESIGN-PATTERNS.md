# Veritheia Design Patterns

This document defines the imperative design patterns that MUST be followed in all Veritheia implementations. These patterns ensure consistency, maintainability, and alignment with the system's intellectual sovereignty principles.

## Core Principles

### Single Source of Truth (SSOT)
- Documentation defines implementation, not vice versa
- No duplicate information across documents
- All code must trace back to documented requirements
- Deviations require explicit documentation

### Formation Over Function
- Every technical decision must support user's intellectual formation
- Patterns that preserve journey context take precedence
- No shortcuts that compromise intellectual sovereignty

## Mandatory Design Patterns

### 1. Domain-Driven Design (DDD)

#### Aggregate Boundaries
```csharp
// User is an aggregate root with Journey as part of the aggregate
public class User : BaseEntity
{
    public string Email { get; private set; }
    public Persona Persona { get; private set; }
    public IReadOnlyCollection<Journey> Journeys => _journeys.AsReadOnly();
    
    private readonly List<Journey> _journeys = new();
    
    // All journey creation goes through the aggregate root
    public Journey StartJourney(Process process, string purpose)
    {
        var journey = new Journey(this, process, purpose);
        _journeys.Add(journey);
        return journey;
    }
}
```

#### Value Objects
```csharp
// Immutable value object for embedding vectors
public record EmbeddingVector
{
    public float[] Values { get; }
    public string Model { get; }
    public int Dimensions => Values.Length;
    
    public EmbeddingVector(float[] values, string model)
    {
        Values = values ?? throw new ArgumentNullException(nameof(values));
        Model = model ?? throw new ArgumentNullException(nameof(model));
        
        if (values.Length != 1536)
            throw new ArgumentException("Embeddings must be 1536 dimensions");
    }
}
```

### 2. Repository Pattern

#### Generic Repository Interface
```csharp
public interface IJourneyAwareRepository<T> where T : BaseEntity
{
    // All queries filtered through journey context
    Task<T?> GetByIdForJourneyAsync(Ulid id, Ulid journeyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> ListForJourneyAsync(Ulid journeyId, ISpecification<T> spec, CancellationToken cancellationToken = default);
    Task<T> AddToJourneyAsync(T entity, Ulid journeyId, string encounterContext, CancellationToken cancellationToken = default);
    Task UpdateInJourneyContextAsync(T entity, Ulid journeyId, CancellationToken cancellationToken = default);
    Task<int> CountForJourneyAsync(Ulid journeyId, ISpecification<T> spec, CancellationToken cancellationToken = default);
}
```

#### Specification Pattern
```csharp
public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; }
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
    int Take { get; }
    int Skip { get; }
    bool IsPagingEnabled { get; }
}

// Example usage
public class ActiveJourneysSpecification : BaseSpecification<Journey>
{
    public ActiveJourneysSpecification(Ulid userId)
    {
        AddCriteria(j => j.UserId == userId && j.CompletedAt == null);
        AddInclude(j => j.JournalEntries);
        ApplyOrderByDescending(j => j.CreatedAt);
    }
}
```

### 3. Result Pattern

#### Result Type for Operation Outcomes
```csharp
public class AuthoredResult<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string Error { get; }
    public Ulid JourneyId { get; } // Every result tied to journey
    public DateTime AuthoredAt { get; } // When this understanding emerged
    public JournalEntry[] GeneratedNarratives { get; } // New insights to record
    
    protected AuthoredResult(bool isSuccess, T? value, string error, Ulid journeyId)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        JourneyId = journeyId;
        AuthoredAt = DateTime.UtcNow;
        GeneratedNarratives = Array.Empty<JournalEntry>();
    }
    
    public static AuthoredResult<T> Success(T value, Ulid journeyId, params JournalEntry[] narratives) 
        => new(true, value, string.Empty, journeyId) { GeneratedNarratives = narratives };
    public static AuthoredResult<T> Failure(string error, Ulid journeyId) 
        => new(false, default, error, journeyId);
}

// Usage in services
public async Task<Result<Document>> ProcessDocumentAsync(string path)
{
    if (!File.Exists(path))
        return Result<Document>.Failure("File not found");
        
    try
    {
        var document = await ExtractDocumentAsync(path);
        return Result<Document>.Success(document);
    }
    catch (Exception ex)
    {
        return Result<Document>.Failure($"Processing failed: {ex.Message}");
    }
}
```

### 4. Process Context Pattern

#### Context for Process Execution
```csharp
public class ProcessContext
{
    public User Author { get; } // Not "User" - they are authors
    public Journey IntellectualJourney { get; } // Not just "Journey"
    public IReadOnlyList<JournalEntry> FormativeNarratives { get; } // Not just "entries"
    public ConceptualFramework AuthorsFramework { get; } // Not generic "scope"
    public IReadOnlyDictionary<string, object> AuthoredInputs { get; } // User's specific framing
    public CancellationToken CancellationToken { get; }
    
    public ProcessContext(
        User user,
        Journey journey,
        IEnumerable<JournalEntry> relevantEntries,
        KnowledgeScope scope,
        Dictionary<string, object> inputs,
        CancellationToken cancellationToken = default)
    {
        User = user ?? throw new ArgumentNullException(nameof(user));
        Journey = journey ?? throw new ArgumentNullException(nameof(journey));
        RelevantEntries = relevantEntries?.ToList() ?? new List<JournalEntry>();
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        Inputs = new ReadOnlyDictionary<string, object>(inputs ?? new Dictionary<string, object>());
        CancellationToken = cancellationToken;
    }
}
```

### 5. Adapter Pattern for Cognitive System

#### Cognitive System Interface
```csharp
public interface ICognitiveAdapter
{
    Task<EmbeddingVector> CreateEmbeddingsAsync(
        string text,
        CancellationToken cancellationToken = default);
        
    Task<FormationAssistance> AssistWithFormationAsync(
        string prompt,
        ProcessContext context,
        AssistantRole role, // Librarian, PeerReviewer, etc.
        CancellationToken cancellationToken = default);
        
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
    
    int MaxContextTokens { get; }
    string ModelIdentifier { get; }
}

public enum AssistantRole
{
    Librarian,      // For relevance assessment
    PeerReviewer,   // For contribution assessment
    Instructor,     // For content generation with constraints
    Evaluator       // For formative assessment
}

public class FormationAssistance
{
    public string Content { get; set; }
    public AssistantRole Role { get; set; }
    public Dictionary<string, object> Metadata { get; set; } // Scores, rationales, etc.
    public Ulid JourneyId { get; set; }
    public DateTime AssistedAt { get; set; }
}

// Example implementation
public class SemanticKernelAdapter : ICognitiveAdapter
{
    private readonly Kernel _kernel;
    private readonly ITextEmbeddingGenerationService _embeddingService;
    
    public int MaxContextTokens => 128000; // Claude 3 context window
    public string ModelIdentifier => "claude-3-opus-20240229";
    
    public async Task<EmbeddingVector> CreateEmbeddingsAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        var embeddings = await _embeddingService.GenerateEmbeddingAsync(
            text,
            cancellationToken: cancellationToken);
            
        return new EmbeddingVector(
            embeddings.ToArray(),
            "text-embedding-3-small");
    }
}
```

### 6. Unit of Work Pattern

#### Transaction Management
```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Document> Documents { get; }
    IRepository<Journey> Journeys { get; }
    IRepository<ProcessExecution> ProcessExecutions { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

// Usage in services
public async Task<Result<Journey>> CreateJourneyAsync(CreateJourneyCommand command)
{
    await _unitOfWork.BeginTransactionAsync();
    
    try
    {
        var user = await _unitOfWork.Users.GetByIdAsync(command.UserId);
        if (user == null)
            return Result<Journey>.Failure("User not found");
            
        var journey = user.StartJourney(command.Process, command.Purpose);
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitTransactionAsync();
        
        return Result<Journey>.Success(journey);
    }
    catch
    {
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

### 7. CQRS Pattern

#### Command/Query Separation
```csharp
// Commands modify state
public record CreateDocumentCommand(
    string FilePath,
    string Title,
    Ulid ScopeId,
    Ulid UserId);

public interface ICommandHandler<TCommand>
{
    Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

// Queries read state
public record GetDocumentsBySearchQuery(
    string SearchTerm,
    Ulid ScopeId,
    int PageNumber = 1,
    int PageSize = 20);

public interface IQueryHandler<TQuery, TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}

// Implementation
public class GetDocumentsBySearchQueryHandler : IQueryHandler<GetDocumentsBySearchQuery, PagedResult<DocumentDto>>
{
    private readonly IRepository<Document> _repository;
    
    public async Task<PagedResult<DocumentDto>> HandleAsync(
        GetDocumentsBySearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var spec = new DocumentSearchSpecification(query.SearchTerm, query.ScopeId)
            .WithPaging(query.PageNumber, query.PageSize);
            
        var documents = await _repository.ListAsync(spec, cancellationToken);
        var count = await _repository.CountAsync(spec, cancellationToken);
        
        return new PagedResult<DocumentDto>(
            documents.Select(d => d.ToDto()),
            count,
            query.PageNumber,
            query.PageSize);
    }
}
```

### 8. Process Implementation Pattern

#### Example: Systematic Screening Process
```csharp
public class SystematicScreeningProcess : IFormationProcess
{
    private readonly ICognitiveAdapter _cognitiveAdapter;
    private readonly IJourneyAwareRepository<Document> _documentRepo;
    
    public async Task<AuthoredResult<ScreeningResults>> ExecuteAsync(
        ProcessContext context,
        CancellationToken cancellationToken)
    {
        var results = new List<DocumentAssessment>();
        var documents = await _documentRepo.ListForJourneyAsync(
            context.IntellectualJourney.Id, 
            new AllDocumentsSpec());
        
        foreach (var doc in documents)
        {
            // AI assists as librarian for relevance
            var relevanceAssist = await _cognitiveAdapter.AssistWithFormationAsync(
                BuildRelevancePrompt(doc, context.AuthoredInputs["ResearchQuestions"]),
                context,
                AssistantRole.Librarian,
                cancellationToken);
            
            // AI assists as peer reviewer for contribution
            var contributionAssist = await _cognitiveAdapter.AssistWithFormationAsync(
                BuildContributionPrompt(doc, context.AuthoredInputs["ResearchQuestions"]),
                context,
                AssistantRole.PeerReviewer,
                cancellationToken);
            
            results.Add(new DocumentAssessment
            {
                Document = doc,
                RelevanceScore = (double)relevanceAssist.Metadata["score"],
                RelevanceRationale = relevanceAssist.Content,
                ContributionScore = (double)contributionAssist.Metadata["score"],
                ContributionRationale = contributionAssist.Content
            });
        }
        
        // User will triage these results to form their understanding
        var journalEntry = new JournalEntry
        {
            Type = JournalType.Method,
            Narrative = $"Screened {documents.Count} documents. AI assistance helped identify patterns in relevance vs contribution..."
        };
        
        return AuthoredResult<ScreeningResults>.Success(
            new ScreeningResults(results),
            context.IntellectualJourney.Id,
            journalEntry);
    }
}
```

### 9. Builder Pattern for Complex Objects

#### Process Definition Builder
```csharp
public class ProcessDefinitionBuilder
{
    private string _name = string.Empty;
    private string _description = string.Empty;
    private readonly List<ProcessInput> _inputs = new();
    private readonly List<ProcessOutput> _outputs = new();
    private ProcessCategory _category = ProcessCategory.Analytical;
    
    public ProcessDefinitionBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public ProcessDefinitionBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }
    
    public ProcessDefinitionBuilder WithInput(string name, Type type, bool required = true)
    {
        _inputs.Add(new ProcessInput(name, type, required));
        return this;
    }
    
    public ProcessDefinitionBuilder WithOutput(string name, Type type)
    {
        _outputs.Add(new ProcessOutput(name, type));
        return this;
    }
    
    public ProcessDefinitionBuilder InCategory(ProcessCategory category)
    {
        _category = category;
        return this;
    }
    
    public ProcessDefinition Build()
    {
        if (string.IsNullOrWhiteSpace(_name))
            throw new InvalidOperationException("Process name is required");
            
        return new ProcessDefinition(
            _name,
            _description,
            _inputs,
            _outputs,
            _category);
    }
}

// Usage
var definition = new ProcessDefinitionBuilder()
    .WithName("Systematic Screening")
    .WithDescription("Dual assessment of document relevance and contribution")
    .WithInput("researchQuestions", typeof(string[]))
    .WithInput("definitions", typeof(Dictionary<string, string>), required: false)
    .WithOutput("assessments", typeof(IEnumerable<DocumentAssessment>))
    .InCategory(ProcessCategory.Analytical)
    .Build();
```

## Cross-Cutting Patterns

### Logging Pattern
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

### Validation Pattern
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
    
    private async Task<bool> ProcessExists(Ulid processId, CancellationToken cancellationToken)
    {
        // Check process registry
        return true;
    }
}
```

### Error Handling Pattern
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

## Implementation Checklist

When implementing any feature, verify:

- [ ] Aggregate boundaries are respected
- [ ] All entities inherit from BaseEntity with ULID primary keys
- [ ] Repositories use the generic interface
- [ ] Complex queries use specifications
- [ ] Operations return Result<T> types
- [ ] Process execution uses ProcessContext
- [ ] Cognitive operations go through ICognitiveAdapter
- [ ] Transactions managed by Unit of Work
- [ ] Commands and queries are separated
- [ ] Complex object creation uses builders
- [ ] Cross-cutting concerns follow established patterns
- [ ] All patterns align with documented architecture

Remember: These patterns are not suggestions—they are requirements. Deviations must be explicitly documented and justified in ARCHITECTURE.md.