⚠️ **ARCHITECTURAL IMPERATIVE VIOLATION WARNING** ⚠️
==================================================
THIS DOCUMENT CONTAINS OUTDATED PATTERNS THAT VIOLATE CURRENT ARCHITECTURAL IMPERATIVES:
- References repository patterns (WE REJECT REPOSITORIES - DbContext IS the data layer)
- Discusses DDD implementation (WE REJECT DDD praxis, embrace only its ontology)
- Mentions mocking internal services (WE MOCK ONLY EXTERNAL services, test with real DB)

MARKED FOR REWRITE - DO NOT FOLLOW THESE PATTERNS
See: ARCHITECTURE.md Section 3.1, DESIGN-PATTERNS.md, IMPLEMENTATION.md
==================================================

# Architectural Divergences from Core Documentation

This document tracks where our implementation diverges from the core documentation, with rationale for each decision.

## Overview

During implementation, several architectural decisions were made that diverge from the original specifications. These divergences are intentional, documented, and justified based on discoveries during dialectical investigation.

## Major Divergences

### 1. Repository Pattern Architecture

**Documentation Says**: `IJourneyAwareRepository<T>` where all queries are filtered through journey context (DESIGN-PATTERNS.md)

**What We Implemented**: Three-tier repository architecture:
- `IRepository<T>` - System-level entities (User, Document, ProcessDefinition)
- `IUserScopedRepository<T>` - User-owned entities (Journey, Persona)
- `IJourneyScopedRepository<T>` - Journey projections (Segments, Formations)

**Rationale**: Not all entities are journey-scoped. Users exist at system level, Journeys belong to users but not other journeys, only projections exist within journey context.

**Impact**: More precise scoping, cleaner boundaries, prevents forcing journey context where it doesn't belong.

### 2. Unit of Work Pattern

**Documentation Says**: Implement `IUnitOfWork` interface with transaction management (DESIGN-PATTERNS.md)

**What We Implemented**: Use DbContext directly as Unit of Work

**Rationale**: Entity Framework's DbContext already implements the Unit of Work pattern. Adding a wrapper would be redundant abstraction without benefit for the MVP.

**Impact**: Simpler architecture, less code to maintain, standard EF Core pattern.

### 3. Document Ownership Tracking

**Documentation Says**: Documents table has no UserId (ENTITY-RELATIONSHIP.md)

**What We Implemented**: Added required `UserId` foreign key to Documents table

**Rationale**: 
- Legal/copyright tracking essential even in single-user MVP
- Future sharing needs ownership attribution
- Audit trail requirements
- Clean migration path to multi-user

**Impact**: RESTRICT delete behavior enforced, documents always have traceable owner.

### 4. Domain Model Pattern

**Documentation Says**: Domain-Driven Design with rich domain models (implied by DDD references)

**What We Implemented**: Anemic domain model with entities as data containers

**Rationale**:
- Veritheia is process-oriented, not entity-oriented
- Intelligence lives in external LLMs, not entities
- Business rules in Process Engine, not domain objects
- Similar to workflow/orchestration platforms

**Impact**: All behavior in services and processes, entities purely for state.

### 5. Core/Data Dependencies

**Documentation Says**: Repository interfaces with entity types (DESIGN-PATTERNS.md shows typed interfaces)

**What We Implemented**: Repository interfaces use `object` or are non-generic in Core

**Rationale**: Core project cannot reference Data project entities (circular dependency). Interfaces define contracts, implementations provide types.

**Impact**: Clean dependency direction, some loss of compile-time type safety at interface level.

### 6. MVP Architecture Clarification

**Documentation Says**: Ambiguous about single vs multi-user (MVP-SPECIFICATION.md has user management but says "desktop")

**What We Implemented**: Single-user desktop application with future cloud sharing path

**Rationale**: Desktop-first architecture like reference managers, with progressive enhancement to cloud sharing.

**Impact**: Simpler MVP, clear evolution path, no initial cloud dependencies.

## Minor Divergences

### Process Types vs Enums

**Documentation**: Shows process types as strings in examples
**Implementation**: Created enums for type safety
**Rationale**: Compile-time validation, IntelliSense support

### Check Constraints

**Documentation**: Lowercase column names in constraints
**Implementation**: Quoted PascalCase names matching EF Core conventions
**Rationale**: EF Core generates PascalCase by default

## Validation Approach

All divergences were:
1. Discovered through dialectical investigation
2. Documented in phase journey files
3. Tested to ensure functionality
4. Recorded in Decision Log

## Future Considerations

These divergences may need revisiting when:
- Moving from MVP to production
- Adding multi-user support
- Implementing cloud features
- Scaling beyond single desktop

## References

- Phase 1 Journey: Database ownership discovery
- Phase 2 Journey: Anemic model justification
- Phase 3 Journey: Repository architecture investigation
- PROGRESS.md: Decision Log with all divergences