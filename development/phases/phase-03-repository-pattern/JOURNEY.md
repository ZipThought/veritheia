# Phase 03 Journey: Repository Pattern

## The Investigation Begins

**2025-08-09 11:30 UTC** - Starting Phase 3 dialectical investigation for repository pattern implementation.

## Pre-Investigation Discovery

Looking back at Phase 1, it had "Repository pattern implementation (NOT STARTED)" in its CHECK section. This reveals an important distinction:
- Phase 1 needed **basic data access** to test if the database worked
- Phase 3 implements **domain repository pattern** with journey boundaries
- These are different abstraction layers of "repository"

This confusion is common in DDD - the word "repository" means different things at different layers.

## The Central Questions

1. How do we implement data access that respects journey-specific projections?
2. Should repositories know about journey boundaries or should that be enforced elsewhere?
3. How do we handle pgvector operations that EF Core doesn't fully support?

## 5W+1H Investigation

### WHAT is a repository in Veritheia's context?

**Initial Understanding**: A repository provides data access abstraction over the database.

**Deeper Question**: In a system where documents only have meaning through journey projections, what does "data access" mean?

**Thesis**: Repositories are simple CRUD wrappers over DbSets
- Just abstract EF Core operations
- Return IQueryable for flexibility
- Let services handle business logic

**Antithesis**: Repositories enforce journey boundaries
- Never return data outside current journey
- Embed journey context in every query
- Repository IS the boundary guardian

**Emerging Synthesis**: Repositories provide **journey-aware data access**
- Base repositories handle simple CRUD
- Journey-scoped repositories add boundary enforcement
- Services orchestrate cross-journey operations when allowed

### WHO uses repositories?

**Direct Users**:
- Services (Phase 6) - for business logic implementation
- Process Engine (Phase 5) - for process execution
- API Controllers (Phase 4) - NEVER directly (through services)

**Indirect Users**:
- UI (Phase 11) - through API calls
- Tests (Phase 12) - through mocking interfaces

**Critical Insight**: Repositories are not just for data access - they're the enforcement point for intellectual sovereignty. They ensure users only see projections from their own journeys.

### WHEN are repositories created vs used?

**Creation Time**:
- Registered in DI container at startup
- Scoped lifetime (per request)
- DbContext injected

**Usage Pattern**:
```csharp
// In a service
public async Task<JourneyFormation> RecordFormationAsync(Guid journeyId, string insight)
{
    // Repository ensures this journey belongs to current user
    var journey = await _journeyRepo.GetByIdAsync(journeyId);
    if (journey == null) throw new UnauthorizedException();
    
    // Create formation within journey bounds
    var formation = new JourneyFormation { ... };
    return await _formationRepo.AddAsync(formation);
}
```

### WHERE do repositories live?

**Thesis**: In veritheia.Data with entities
- Close to what they access
- Can use internal DbContext knowledge

**Antithesis**: In veritheia.Core as interfaces, implementations in Data
- Interfaces are contracts (core)
- Implementations are details (data)

**Synthesis**: 
- **Interfaces** in `veritheia.Core/Interfaces/Repositories/`
- **Implementations** in `veritheia.Data/Repositories/`
- This allows Core to define contracts without knowing about EF Core

### WHY do we need repositories when we have DbContext?

**Beyond Simple Abstraction**:

1. **Journey Boundary Enforcement**
   ```csharp
   // BAD: Direct DbContext access
   var segments = _context.JourneyDocumentSegments.Where(s => s.JourneyId == id);
   
   // GOOD: Repository enforces boundaries
   var segments = await _segmentRepo.GetByJourneyAsync(journeyId, userId);
   ```

2. **Vector Operations Abstraction**
   ```csharp
   // Repository hides pgvector complexity
   public async Task<List<JourneyDocumentSegment>> FindSimilarAsync(
       Guid journeyId, float[] embedding, float threshold)
   {
       // Raw SQL for vector similarity
       return await _context.JourneyDocumentSegments
           .FromSqlRaw(@"SELECT * FROM journey_document_segments s
                        JOIN search_indexes i ON s.id = i.segment_id
                        JOIN search_vectors_1536 v ON i.id = v.index_id
                        WHERE s.journey_id = {0}
                        AND v.embedding <=> {1} < {2}",
                        journeyId, embedding, threshold)
           .ToListAsync();
   }
   ```

3. **Testability**
   - Mock IRepository instead of DbContext
   - Simpler test setup
   - Clear boundaries

### HOW should repositories be structured?

**Pattern Discovery Through Dialectical Testing**:

#### Pattern 1: Generic Repository Only
```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}
```
**Problem**: No journey awareness, no vector operations

#### Pattern 2: Specific Repositories Only
```csharp
public interface IJourneyRepository
{
    Task<Journey?> GetByIdAsync(Guid id, Guid userId);
    Task<List<Journey>> GetUserJourneysAsync(Guid userId);
    // ... specific methods
}
```
**Problem**: Lots of duplication, no common interface

#### Pattern 3: Layered Approach (SYNTHESIS)
```csharp
// Base generic interface
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    IQueryable<T> Query();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}

// Journey-aware base
public interface IJourneyScoped<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, Guid journeyId);
    Task<List<T>> GetByJourneyAsync(Guid journeyId);
}

// Specific repository with both patterns
public interface IJourneyDocumentSegmentRepository : 
    IRepository<JourneyDocumentSegment>,
    IJourneyScoped<JourneyDocumentSegment>
{
    // Vector operations specific to segments
    Task<List<JourneyDocumentSegment>> FindSimilarAsync(
        Guid journeyId, float[] embedding, float threshold);
}
```

## Critical Discoveries

### 1. Journey Boundaries Are First-Class

Repositories MUST enforce journey boundaries. This isn't optional security - it's fundamental to the architecture where meaning exists only within journey projections.

### 2. Vector Operations Need Special Handling

EF Core can't generate vector similarity queries. Repositories must:
- Use FromSqlRaw for vector operations
- Abstract the complexity from services
- Handle different vector dimensions (1536, 768, 384)

### 3. Unit of Work Pattern

**Decision**: Use DbContext as implicit Unit of Work
- EF Core already implements this pattern
- Adding IUnitOfWork wrapper adds complexity without benefit
- Services can coordinate multiple repository operations in one transaction

### 4. Specification Pattern

**Decision**: Use simple LINQ predicates, not full Specification pattern
- Specification pattern is overengineering for MVP
- LINQ expressions are sufficient
- Can add specifications later if needed

## The Repository Architecture

Based on investigation, Phase 3 will implement:

```
veritheia.Core/
├── Interfaces/
│   └── Repositories/
│       ├── IRepository.cs                    # Base generic
│       ├── IJourneyScoped.cs                 # Journey-aware base
│       ├── IUserRepository.cs                # User-specific
│       ├── IJourneyRepository.cs             # Journey-specific
│       ├── IDocumentRepository.cs            # Document-specific
│       ├── IJourneyDocumentSegmentRepository.cs  # Segment-specific with vector ops
│       └── ... (one for each aggregate root)

veritheia.Data/
├── Repositories/
│   ├── BaseRepository.cs                     # Generic implementation
│   ├── JourneyScopedRepository.cs           # Journey-aware base implementation
│   ├── UserRepository.cs                     # Implements IUserRepository
│   ├── JourneyRepository.cs                  # Implements IJourneyRepository
│   ├── DocumentRepository.cs                 # Implements IDocumentRepository
│   ├── JourneyDocumentSegmentRepository.cs   # Implements segment repo with vector ops
│   └── ... (implementations)
```

## Decision Records

### Decision 1: Layered Repository Pattern
**Choice**: Generic base + journey-aware base + specific interfaces
**Rationale**: 
- Reduces duplication
- Enforces journey boundaries consistently
- Allows specific operations per entity type

### Decision 2: Interfaces in Core, Implementations in Data
**Choice**: Split interfaces and implementations
**Rationale**:
- Core defines contracts without EF Core dependency
- Data has EF Core specific implementations
- Enables testing through mocking

### Decision 3: DbContext as Unit of Work
**Choice**: No explicit IUnitOfWork wrapper
**Rationale**:
- DbContext already provides this
- Simpler for MVP
- Can add if needed later

### Decision 4: Raw SQL for Vector Operations
**Choice**: Use FromSqlRaw in repositories for pgvector
**Rationale**:
- EF Core doesn't support vector operations
- Repositories abstract this complexity
- Services don't need to know about SQL

## What This Enables

With repositories in place:
- Phase 4 can build APIs that use repositories through services
- Phase 5 Process Engine can persist executions
- Phase 6 Services have data access layer
- Testing becomes possible through mocking

## Alignment with DESIGN-PATTERNS.md

After investigation, I found that DESIGN-PATTERNS.md already specifies:
- `IJourneyAwareRepository<T>` interface that filters everything through journey context
- Specification pattern for complex queries
- Journey boundaries built into every repository method

This confirms our investigation findings - journey boundaries are NOT optional.

## Open Questions for Implementation

1. Should we have a separate IVectorSearchRepository?
2. How to handle pagination efficiently?
3. Should repositories return DTOs or entities? (Entities for repositories, DTOs for APIs)
4. How to handle includes/eager loading? (Specification pattern handles this)
5. How to reconcile generic IRepository<T> with IJourneyAwareRepository<T>?

These will be resolved during implementation.

## Critical Disambiguation Problem

**2025-08-09 11:45 UTC** - Human correctly identifies ambiguity issue.

### The Problem

We have conflicting patterns:
1. **Generic IRepository<T>** - Standard CRUD operations
2. **IJourneyAwareRepository<T>** - Everything filtered by journey
3. But some entities DON'T belong to journeys (User, ProcessDefinition)

### The Disambiguation Investigation

#### Which Entities Are Journey-Scoped?

**Journey-Scoped** (need journey context):
- JourneyDocumentSegment (segments exist within journeys)
- JourneyFormation (insights from journeys)
- JourneySegmentAssessment (assessments within journeys)
- Journal & JournalEntry (belong to journeys)

**User-Scoped** (need user context, not journey):
- Journey (user has many journeys)
- Persona (user has personas)
- ProcessCapability (user capabilities)

**System-Level** (no journey or user scope):
- User (the root entity)
- ProcessDefinition (system configuration)
- Document (exists independently, projected into journeys)
- KnowledgeScope (organizational structure)

#### The Naming Conflict

If we have:
- `IRepository<User>` for users
- `IJourneyAwareRepository<JourneyDocumentSegment>` for segments

But what about:
- Documents that exist globally but are projected into journeys?
- Journeys that belong to users but not to other journeys?

### Proposed Resolution: Three-Tier Repository Architecture

```csharp
// Tier 1: System-level repositories (no scoping)
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}

// Tier 2: User-scoped repositories
public interface IUserScopedRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, Guid userId);
    Task<IReadOnlyList<T>> GetByUserAsync(Guid userId);
    Task<bool> BelongsToUserAsync(Guid entityId, Guid userId);
}

// Tier 3: Journey-scoped repositories
public interface IJourneyScopedRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, Guid journeyId);
    Task<IReadOnlyList<T>> GetByJourneyAsync(Guid journeyId);
    Task<bool> BelongsToJourneyAsync(Guid entityId, Guid journeyId);
}
```

### Specific Repository Examples

```csharp
// User repository - system level only
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> ExistsAsync(string email);
}

// Journey repository - user-scoped
public interface IJourneyRepository : 
    IRepository<Journey>,
    IUserScopedRepository<Journey>
{
    Task<Journey?> GetActiveJourneyAsync(Guid userId, string processType);
}

// Document repository - system level with projection awareness
public interface IDocumentRepository : IRepository<Document>
{
    // Documents exist globally
    Task<Document?> GetByFilePathAsync(string path);
    
    // But can check journey projections
    Task<bool> HasProjectionInJourneyAsync(Guid documentId, Guid journeyId);
}

// Segment repository - journey-scoped with vector ops
public interface IJourneyDocumentSegmentRepository : 
    IRepository<JourneyDocumentSegment>,
    IJourneyScopedRepository<JourneyDocumentSegment>
{
    Task<List<JourneyDocumentSegment>> FindSimilarAsync(
        Guid journeyId, 
        float[] embedding, 
        float threshold);
}
```

### The Key Insight

**Not everything is journey-scoped!** The disambiguation comes from recognizing three distinct scoping levels:

1. **System**: Entities that exist independently (User, Document, ProcessDefinition)
2. **User**: Entities owned by users but not journeys (Journey, Persona)
3. **Journey**: Entities that only exist within journey projections

This prevents us from forcing journey context where it doesn't belong while still enforcing it where it's critical.

## Test Infrastructure Investigation

**2025-08-09 12:30 UTC** - Investigating how to properly test Phase 1 & 2 before proceeding.

### The Testing Challenge

Before implementing Phase 3 repositories, we need to verify Phase 1 (database) and Phase 2 (domain models) actually work. This requires a test infrastructure decision.

### Test Isolation Options Evaluated

#### Option 1: Transaction Rollback
```csharp
// Begin transaction → Run test → Rollback
```
**Problem**: Can't test vector operations or raw SQL in rolled-back transactions.

#### Option 2: Fresh Container Per Test
```csharp
// New PostgreSQL container for each test
```
**Problem**: Too slow (2-3 seconds per test), resource intensive.

#### Option 3: Respawn - Database Reset (CHOSEN)
```csharp
// Single container → Run test → Reset data (keep schema)
```
**Benefits**: 
- Fast (~50ms per reset)
- Tests real PostgreSQL features
- True isolation between tests
- Can test transactions

### Decision: Respawn Model

**Choice**: Use Respawn library with single PostgreSQL container
**Rationale**:
1. Journey projections need real PostgreSQL (vectors, JSONB)
2. Fast enough for rapid testing
3. True isolation without container overhead
4. Can test everything including raw SQL

### Test Structure Decision

**Choice**: Expand existing `veritheia.Tests` project
**Rationale**:
- Already exists with Aspire integration tests
- Single test project easier to manage
- Can organize by phase/layer internally

```
veritheia.Tests/
├── TestBase/
│   ├── DatabaseFixture.cs       # Shared container + Respawn
│   └── DatabaseTestBase.cs      # Base class for all DB tests
├── Phase1_Database/
│   ├── BasicCrudTests.cs
│   ├── RelationshipTests.cs
│   └── VectorStorageTests.cs
├── Phase2_DomainModels/
│   ├── EnumSerializationTests.cs
│   └── ValueObjectTests.cs
└── Phase3_Repositories/
    └── (future)
```

### PostgreSQL-Only Decision

**Choice**: All tests use PostgreSQL, no in-memory database
**Rationale**:
- Most features require PostgreSQL anyway (vectors, JSONB, ranges)
- Eliminates false positives from in-memory differences
- Single mental model for testing
- Confidence that tests validate production behavior

## Next Steps (Revised)

1. **Complete Phase 1 & 2 Testing First**
   - Add Respawn and Testcontainers to veritheia.Tests
   - Create DatabaseFixture with container management
   - Write BasicDataAccess class for simple CRUD
   - Test Phase 1 schema actually accepts data
   - Test Phase 2 value objects work correctly

2. **Then Proceed to Phase 3**
   - Create three repository interface tiers in Core
   - Implement base classes for each tier
   - Create specific repositories based on entity scope
   - Add vector operations to journey-scoped segment repository
   - Write tests verifying scope enforcement at each tier