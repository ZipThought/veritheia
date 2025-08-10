⚠️ **ARCHITECTURAL IMPERATIVE VIOLATION WARNING** ⚠️
==================================================
THIS DOCUMENT CONTAINS OUTDATED PATTERNS THAT VIOLATE CURRENT ARCHITECTURAL IMPERATIVES:
- Investigates repository patterns (WE REJECT REPOSITORIES - DbContext IS the data layer)
- References IRepository interfaces (NO REPOSITORY ABSTRACTIONS)
- Discusses Unit of Work pattern (DbContext IS the Unit of Work)

See EPILOGUE.md for why this investigation led to REJECTION of repository patterns
MARKED FOR HISTORICAL REFERENCE ONLY - DO NOT IMPLEMENT
See: ARCHITECTURE.md Section 3.1, DESIGN-PATTERNS.md, IMPLEMENTATION.md
==================================================

# Phase 03: Repository Pattern

## Status
**Current State**: Investigation  
**Started**: 2025-08-09  
**Completed**: -  

## The Question This Phase Answers

How do we implement data access that respects journey boundaries while enabling efficient queries across the projection space architecture?

## Relevant Specifications

- [DESIGN-PATTERNS.md#2-repository-pattern](../../../docs/DESIGN-PATTERNS.md#2-repository-pattern) - Repository pattern specifications
- [API-CONTRACTS.md](../../../docs/API-CONTRACTS.md) - Repository interfaces defined
- Phase 1 entities in `/veritheia.Data/Entities/`
- Phase 2 value objects in `/veritheia.Core/ValueObjects/`

## The Tensions to Resolve

### 1. Generic vs Specific Repositories

Should we have:
- Generic `IRepository<T>` for all entities?
- Specific repositories like `IJourneyRepository`?
- Both with inheritance?

### 2. Journey Boundary Enforcement

How do we ensure:
- Queries respect journey boundaries?
- Users can't access other journeys' projections?
- Cross-journey analysis when explicitly allowed?

### 3. Vector Operations

PostgreSQL pgvector requires special handling:
- EF Core doesn't fully support vector operations
- Need raw SQL for similarity searches
- How to abstract this in repositories?

### 4. Unit of Work Pattern

Should we:
- Use DbContext directly as Unit of Work?
- Create explicit IUnitOfWork wrapper?
- How does this interact with .NET Aspire?

### 5. Query Specifications

For complex queries:
- Use Specification pattern?
- LINQ expressions?
- Raw SQL when needed?

## What Phase 3 Must Enable

The repository implementation must:
1. Provide CRUD operations for all entities
2. Respect journey projection boundaries
3. Enable vector similarity searches
4. Support efficient relationship loading
5. Allow testability through interfaces

## Investigation Approach

1. **5W+1H Analysis** of repository needs
2. **Dialectical examination** of design patterns
3. **Code experiments** with vector queries
4. **Performance testing** of different approaches
5. **Decision documentation** with rationale

## Navigation

- [JOURNEY.md](./JOURNEY.md) - The chronological investigation record
- [artifacts/](./artifacts/) - Code experiments and proofs