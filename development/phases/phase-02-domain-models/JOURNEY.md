# Phase 02 Journey: Core Domain Models

## The Investigation Begins

**2025-08-09 10:45 UTC** - Starting Phase 2 PDCA cycle. Immediately discovered we need dialectical investigation before implementation.

## The Central Question

How should domain models relate to the journey projection database schema created in Phase 1?

## 5W+1H Investigation

### WHAT are domain models in Veritheia's context?

**Thesis**: Domain models are C# classes that directly mirror database entities (anemic domain model)
- Simple property bags
- One-to-one mapping with tables
- Entity Framework handles all persistence

**Antithesis**: Domain models encapsulate business logic and rules (rich domain model)
- Behavior lives in the model
- Models enforce invariants
- Models know how to project documents

**Synthesis**: Domain models in Veritheia are **projection vessels** - they hold journey-specific transformations but don't generate them. The models:
- Mirror database entities for clean mapping
- Contain navigation properties for relationships
- DON'T contain business logic (that's in services)
- DON'T generate insights (that violates sovereignty)

### WHO uses these domain models?

- **Repositories** (Phase 3) - for CRUD operations
- **Services** (Phase 6) - for business logic
- **Process Engine** (Phase 5) - for orchestration
- **NOT the UI directly** - DTOs will handle that

### WHEN are domain models created vs entities?

**Discovery**: We already have entities in veritheia.Data from Phase 1!
- BaseEntity.cs
- Journey.cs
- JourneyDocumentSegment.cs
- etc.

These ARE the domain models! Phase 1 created them in the Data project.

### WHERE should domain models live?

**Thesis**: In veritheia.Core as pure domain objects
**Antithesis**: In veritheia.Data with the DbContext
**Synthesis**: They're already in veritheia.Data/Entities - this is acceptable for MVP

### WHY the confusion about ProcessedContent vs JourneyDocumentSegment?

**Investigation**: 
- CLASS-MODEL.md appears to be from an earlier design iteration
- Phase 1 journey discovered journey projections AFTER CLASS-MODEL.md was written
- ENTITY-RELATIONSHIP.md was updated but CLASS-MODEL.md wasn't

**Resolution**: Update CLASS-MODEL.md to reflect Phase 1 discoveries

### HOW do we proceed?

1. The "domain models" already exist as entities in veritheia.Data
2. They correctly implement journey projections
3. Phase 2 should:
   - Add any missing navigation properties
   - Add value objects (ProcessContext, JourneyContext, etc.)
   - Update CLASS-MODEL.md to match reality
   - Create domain enums

## The Dialectical Test

### Pattern 1: Separate Domain Models from Entities

**Thesis**: Create clean domain models in Core, map to entities
```csharp
// In Core
public class JourneyDomain {
    public Guid Id { get; private set; }
    public void Project(Document doc) { ... }
}

// In Data  
public class JourneyEntity {
    public Guid Id { get; set; }
}
```

**Problems**: 
- Duplicate code
- Complex mapping
- No clear benefit for MVP

### Pattern 2: Entities AS Domain Models

**Thesis**: Use entities directly as domain models
```csharp
// In Data/Entities
public class Journey : BaseEntity {
    // Properties (data)
    public string Purpose { get; set; }
    
    // Navigation (relationships)  
    public ICollection<JourneyDocumentSegment> Segments { get; set; }
    
    // NO business logic here
}
```

**Benefits**:
- Already implemented in Phase 1
- Clean EF Core mapping
- Single source of truth

**This is the correct pattern for Veritheia MVP**

## Critical Discovery

Phase 2 "Core Domain Models" doesn't mean creating new classes - it means:
1. Completing the entity definitions from Phase 1
2. Adding value objects and enums
3. Ensuring navigation properties work
4. Updating documentation to match

The entities ARE the domain models in this architecture.

## Decision Record

**Decision**: Use entities as domain models (Pattern 2)

**Rationale**:
1. Entities already exist from Phase 1
2. They implement journey projections correctly
3. Business logic belongs in services, not models
4. This maintains intellectual sovereignty (models don't generate)
5. Simpler for MVP

**What Phase 2 Actually Delivers**:
- Completed entity definitions
- Value objects (ProcessContext, JourneyContext, etc.)
- Domain enums
- Updated CLASS-MODEL.md
- NO duplicate model classes

## What This Means

Phase 2 is about **completing** the domain model that Phase 1 started, not creating a parallel structure. The journey projection architecture is preserved in the entities themselves.