# Entity Scoping Analysis for Repository Pattern

## Scoping Tiers

### Tier 1: System-Level Entities (No Scoping)

These entities exist independently and are accessed without user or journey context:

| Entity | Repository Interface | Rationale |
|--------|---------------------|-----------|
| User | IUserRepository : IRepository<User> | Root entity, no parent |
| ProcessDefinition | IProcessDefinitionRepository : IRepository<ProcessDefinition> | System configuration |
| KnowledgeScope | IKnowledgeScopeRepository : IRepository<KnowledgeScope> | Organizational structure |

### Tier 2: User-Scoped Entities

These entities belong to users but not to journeys:

| Entity | Repository Interface | Rationale |
|--------|---------------------|-----------|
| Journey | IJourneyRepository : IRepository<Journey>, IUserScopedRepository<Journey> | User owns journeys |
| Persona | IPersonaRepository : IRepository<Persona>, IUserScopedRepository<Persona> | User has personas |
| ProcessCapability | IProcessCapabilityRepository : IUserScopedRepository<ProcessCapability> | User capabilities |
| ProcessExecution | IProcessExecutionRepository : IUserScopedRepository<ProcessExecution> | User initiates executions |

### Tier 3: Journey-Scoped Entities

These entities only exist within journey contexts:

| Entity | Repository Interface | Rationale |
|--------|---------------------|-----------|
| JourneyFramework | IJourneyFrameworkRepository : IJourneyScopedRepository<JourneyFramework> | Defines journey projection |
| JourneyDocumentSegment | IJourneyDocumentSegmentRepository : IJourneyScopedRepository<JourneyDocumentSegment> | Segments exist in journeys |
| JourneySegmentAssessment | IJourneySegmentAssessmentRepository : IJourneyScopedRepository<JourneySegmentAssessment> | Assessments of segments |
| JourneyFormation | IJourneyFormationRepository : IJourneyScopedRepository<JourneyFormation> | Insights from journeys |
| Journal | IJournalRepository : IJourneyScopedRepository<Journal> | Journals belong to journeys |
| JournalEntry | IJournalEntryRepository : IJourneyScopedRepository<JournalEntry> | Entries in journals |
| SearchIndex | ISearchIndexRepository : IJourneyScopedRepository<SearchIndex> | Indexes for segments |

### Special Cases: Dual-Nature Entities

These entities exist at one level but interact with another:

| Entity | Repository Interface | Rationale |
|--------|---------------------|-----------|
| Document | IDocumentRepository : IRepository<Document> | Exists globally but projected into journeys |
| DocumentMetadata | Part of Document aggregate | Metadata follows document |
| SearchVector* | IVectorSearchRepository | Technical tables for vector operations |
| ProcessResult | IProcessResultRepository : IUserScopedRepository<ProcessResult> | Results belong to user's execution |

## Repository Method Patterns

### System-Level Pattern
```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
    IQueryable<T> Query(); // For complex queries
}
```

### User-Scoped Pattern
```csharp
public interface IUserScopedRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, Guid userId);
    Task<IReadOnlyList<T>> GetByUserAsync(Guid userId);
    Task<bool> BelongsToUserAsync(Guid entityId, Guid userId);
    Task<T> AddForUserAsync(T entity, Guid userId);
}
```

### Journey-Scoped Pattern
```csharp
public interface IJourneyScopedRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, Guid journeyId);
    Task<IReadOnlyList<T>> GetByJourneyAsync(Guid journeyId);
    Task<bool> BelongsToJourneyAsync(Guid entityId, Guid journeyId);
    Task<T> AddToJourneyAsync(T entity, Guid journeyId);
}
```

## Boundary Enforcement Rules

1. **System-Level**: No automatic filtering, but may have methods to check relationships
2. **User-Scoped**: ALWAYS filter by userId, throw UnauthorizedException if access denied
3. **Journey-Scoped**: ALWAYS filter by journeyId, validate journey belongs to requesting user

## Vector Search Special Handling

Vector operations cross boundaries and need special treatment:

```csharp
public interface IVectorSearchRepository
{
    // Search within a journey's projection space
    Task<List<SearchResult>> FindSimilarInJourneyAsync(
        Guid journeyId, 
        float[] embedding, 
        int dimension,
        float threshold,
        int limit = 10);
    
    // Search across user's journeys (for cross-journey insights)
    Task<List<SearchResult>> FindSimilarAcrossUserJourneysAsync(
        Guid userId,
        float[] embedding,
        int dimension,
        float threshold,
        int limit = 10);
}
```

## Key Principles

1. **Least Privilege**: Start with most restrictive scope, relax only when needed
2. **Explicit Boundaries**: Never implicit access across boundaries
3. **Journey Sovereignty**: Journey-scoped entities NEVER leak across journeys
4. **User Ownership**: User-scoped entities respect user boundaries
5. **System Transparency**: System-level entities have no hidden filtering