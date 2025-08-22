⚠️ **ARCHITECTURAL IMPERATIVE VIOLATION WARNING** ⚠️
==================================================
THIS DOCUMENT CONTAINS OUTDATED PATTERNS THAT VIOLATE CURRENT ARCHITECTURAL IMPERATIVES:
- Discusses DDD principles without clarifying we reject the praxis
- References repository pattern as future work
- May contain patterns we now reject

HISTORICAL JOURNEY - Shows evolution of thinking
Does not reflect current architectural stance
See: ARCHITECTURE.md Section 3.1, DESIGN-PATTERNS.md, IMPLEMENTATION.md
==================================================

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

---

## 2025-08-10 - Anemic Domain Model Justification

### Context

Phase 2 implements an anemic domain model where entities are data containers and all behavior lives in services. This needs justification against DDD principles.

### Analysis Against DDD Criteria

**Why Anemic is RIGHT for Veritheia:**

1. **Integration-Driven Architecture** (Criterion #3)
   - Core purpose: Orchestrating cognitive processes through external LLMs
   - Entities are projection spaces for AI processing
   - Business logic = routing documents through analytical processes
   - Our "domain" is orchestration, not the entities themselves

2. **Performance and Serialization Constraints** (Criterion #5)
   - Vector operations require efficient data structures
   - Large document processing needs minimal object overhead
   - Heavy JSONB usage requires clean serialization
   - Journey projections involve intensive I/O operations

3. **Not System of Record for Intelligence** (Criterion #2)
   - Real intelligence lives in LLMs (external)
   - Business rules in Process Definitions (configurable)
   - Analytical structures in Journey Frameworks (user-defined)
   - Entities just hold state between cognitive operations

4. **Process-Oriented Domain**
   - Similar to workflow engines or BPM systems
   - Domain logic lives in Process Engine, not entities
   - Processes are first-class citizens, entities are state
   - Behavior = Process Execution, not entity methods

### What This Means

```csharp
// WRONG for Veritheia (Rich Domain Model):
public class Journey
{
    public void AddSegment(Document doc, string content)
    {
        // Business logic embedded in entity
        ValidateSegment(content);
        ApplyProjectionRules();
        UpdateFormations();
    }
}

// RIGHT for Veritheia (Anemic Model + Process Engine):
public class Journey : BaseEntity
{
    // Pure state container
    public Guid UserId { get; set; }
    public string State { get; set; }
    public Dictionary<string, object> Context { get; set; }
}

public class SegmentationProcess : IProcess
{
    public async Task Execute(Journey journey, Document doc)
    {
        // All behavior in process
        var segments = await CognitiveSystem.Segment(doc);
        await Repository.SaveSegments(journey.Id, segments);
        await FormationProcess.Update(journey.Id);
    }
}
```

### The Key Principle

**"Entities are projection spaces, Processes are projectors"**

- Entities: Hold journey state and projections
- Process Engine: Executes all transformations
- Cognitive System: Provides intelligence
- Repositories: Handle persistence

This is not an anti-pattern but a conscious architectural choice aligned with Veritheia's orchestration-centric design.

### Validation

The anemic model is justified because:
1. Domain complexity is in process orchestration, not entity behavior
2. Intelligence is external (LLMs), not embedded
3. Performance requires lightweight data structures
4. Journey projections are data transformations, not business invariants

If we had complex business rules about document ownership, journey state transitions, or formation validation that were intrinsic to the entities, we'd need a rich model. But these rules are:
- Configurable (Process Definitions)
- External (Cognitive System)
- User-defined (Journey Frameworks)

Therefore, the anemic model is not only acceptable but optimal for Veritheia's architecture.

---

## 2025-08-10 - Test Implementation Findings

### The Fundamental Discovery

Phase 2's journey revealed a critical architectural insight: **The entities created in Phase 1 ARE the domain models**. This wasn't immediately obvious from the documentation.

### Value Objects: Design vs Reality

The PersonaContext evolved from tracking simple vocabulary to tracking patterns of inquiry:

**What Tests Expected**:
```csharp
public class PersonaContext {
    public Guid PersonaId { get; set; }
    public string Domain { get; set; }
    public Dictionary<string, int> ConceptualVocabulary { get; set; }
}
```

**What Actually Exists**:
```csharp
public class PersonaContext {
    public string? DomainFocus { get; set; }
    public List<string> RelevantVocabulary { get; set; }
    public List<InquiryPattern> ActivePatterns { get; set; }
    public List<string> MethodologicalPreferences { get; set; }
}
```

The actual implementation is richer—it tracks patterns of inquiry, not just vocabulary frequency.

### Critical Implementation Gaps

1. **JourneyContext Missing JourneyId**: The context is OF a journey, not ABOUT a journey
2. **InputDefinition's Sophisticated Design**: Fluent API with builder pattern
3. **InquiryPattern as First-Class Concept**: System tracks HOW users think, not just WHAT

### Philosophical Alignment

The value objects embody Veritheia's principles:
- **FormationMarker**: Captures moments of understanding (formation over extraction)
- **InquiryPattern**: Tracks how users think (sovereignty of method)
- **JourneyContext**: Preserves narrative continuity (journey-specific meaning)
- **PersonaContext**: Evolves with user (growth over static profile)

### The Meta-Insight

Testing Phase 2 revealed that the confusion about "domain models" reflects a deeper truth: In a system about knowledge formation, even the code formation process involves discovering that what exists (entities) already serves the need we thought required new construction (separate domain models). The system teaches even as we build it.