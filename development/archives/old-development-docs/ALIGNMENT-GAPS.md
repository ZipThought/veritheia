# Development Documentation Alignment Gaps

## Overview

This document inventories gaps and divergences between development documentation and the current architectural imperatives established in ARCHITECTURE.md, IMPLEMENTATION.md, and DESIGN-PATTERNS.md.

## CRITICAL ARCHITECTURAL VIOLATIONS IN IMPLEMENTATION

### DATABASE SCHEMA VIOLATES PARTITION IMPERATIVES - CRITICAL

**Finding**: The database schema fundamentally violates the user partition architecture.

**Critical Violations Found**:
1. **PRIMARY KEYS ARE WRONG**: All entities use single `Guid Id` instead of composite `(UserId, Id)`
2. **NO PARTITION ENFORCEMENT**: DbContext uses `entity.HasKey(e => e.Id)` instead of `entity.HasKey(e => new { e.UserId, e.Id })`
3. **CROSS-PARTITION FOREIGN KEYS**: Journey→Persona and Document→Scope can cross user boundaries
4. **MISSING PARTITION INFRASTRUCTURE**: No IUserOwned interface, no query extensions for `.ForUser(userId)`
5. **INDEXES NOT PARTITIONED**: Should be `(user_id, ...)` for all user-owned entities

**Impact**: The entire horizontal scaling strategy is broken. The system cannot partition by user.

### TEST INFRASTRUCTURE VIOLATES NO-MOCKING IMPERATIVES

**Violations Found**:
1. **UseInMemoryDatabase**: Tests use in-memory database instead of real PostgreSQL
2. **Moq for Internal Services**: Tests mock internal services (only external allowed)
3. **No Respawn Setup**: Tests don't use Respawn for state reset
4. **Tests Don't Compile**: Missing EF InMemory package, wrong references

### Documentation Overstates Implementation Completeness

**Actual Assessment**:
- **Database**: ❌ 20% functional - schema exists but VIOLATES partition imperatives
- **APIs**: ⚠️ 40% functional - basic CRUD works but no partition enforcement
- **Process Engine**: ⚠️ 30% functional - framework exists, no journey awareness  
- **Cognitive Integration**: ⚠️ 30% functional - basic LLM works, no journey projection
- **Testing**: ❌ 0% functional - tests don't compile, violate imperatives
- **UI**: ❌ 5% functional - only "Hello, Veritheia!" skeleton page

**Honest Total**: ~20-30% of actual MVP functionality works correctly per imperatives.

### Specific Implementation Gaps vs Documentation Claims

#### Journey Projection Spaces - NOT IMPLEMENTED
- **Claimed**: "Documents transformed according to journey-specific rules"
- **Reality**: Database schema supports this but no implementation found of:
  - Journey-specific document segmentation logic
  - Embedding generation with journey context  
  - Assessment within projection boundaries

#### Process Engine Sophistication - OVERSTATED
- **Claimed**: "Orchestrates analytical workflows as first-class runtime entities"
- **Reality**: Basic framework exists, 2 reference processes, but no journey-aware processing

#### Cognitive System Integration - BASIC ONLY  
- **Claimed**: "Journey-specific assessment within projection spaces"
- **Reality**: Basic LLM calls without journey context or assessment framework

#### Testing Philosophy - BROKEN
- **Claimed**: "42+ tests, no internal mocking, real PostgreSQL"
- **Reality**: Test discovery broken, cannot verify claims, infrastructure exists but non-functional

### Root Cause Analysis

1. **Misunderstood Imperatives**: AI agent created standard EF Core patterns instead of partition-first design
2. **Skipped Journey Investigation**: Phases 4-10 rushed without understanding requirements
3. **Copy-Paste Testing**: Tests copied patterns that violate imperatives (InMemory, Moq)
4. **No Verification**: Claims made without running tests or checking against imperatives
5. **Fundamental Misalignment**: Database schema doesn't implement the core partition strategy

## CRITICAL CODE FIXES REQUIRED

### CRITICAL: Fix Database Schema Partition Keys
**Location**: `/veritheia.Data/VeritheiaDbContext.cs` and all entity configurations
**Violations**:
- Primary keys must be composite: `(UserId, Id)` for all user-owned entities
- Indexes must start with UserId for partition locality
- Foreign keys must not cross partition boundaries
**Required Changes**:
```csharp
// WRONG - Current
entity.HasKey(e => e.Id);

// CORRECT - Required
entity.HasKey(e => new { e.UserId, e.Id });
entity.HasIndex(e => new { e.UserId, e.CreatedAt });
```

### CRITICAL: Remove All Test Mocking Violations
**Location**: `/veritheia.Tests/`
**Violations**:
- Remove all `UseInMemoryDatabase` - use real PostgreSQL
- Remove all Moq usage for internal services
- Add Respawn for database state reset
**Required Changes**:
```csharp
// WRONG - Current
.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
var mockServiceProvider = new Mock<IServiceProvider>();

// CORRECT - Required
.UseNpgsql(connectionString)
await _respawner.ResetAsync(connectionString);
```

### CRITICAL: Implement Query Partition Extensions
**Location**: Create `/veritheia.Data/Extensions/QueryExtensions.cs`
**Missing**:
```csharp
public static IQueryable<T> ForUser<T>(this IQueryable<T> query, Guid userId) 
    where T : IUserOwned
{
    return query.Where(e => e.UserId == userId);
}
```

### HIGH: Implement Journey Projection Logic
**Location**: Services that handle documents
**Missing**: Journey-specific segmentation, embeddings with context, assessment within boundaries

## Legacy Documentation Alignment Gaps

### 1. Phase 3 Repository Pattern Documentation

**Location**: `/development/phases/phase-03-repository-pattern/`

**Current State**: 
- Extensive journey investigating repository patterns
- Proposes layered repository architecture
- Suggests IJourneyScoped interfaces

**Required Change**:
- Mark as "Investigation Leading to Rejection"
- Add epilogue explaining why we reject repository abstractions
- Document that DbContext IS the repository

### 2. ARCHITECTURAL-DIVERGENCES.md

**Location**: `/development/ARCHITECTURAL-DIVERGENCES.md`

**Current State**:
- Claims three-tier repository architecture was implemented
- Says IUnitOfWork was considered but DbContext used directly
- Discusses anemic vs rich domain models

**Required Change**:
- Complete rewrite reflecting current imperatives
- Document that schema IS the domain model
- Clarify rejection of DDD praxis while embracing ontology

### 3. PROGRESS.md Phase Descriptions

**Location**: `/development/PROGRESS.md`

**Issues**:
- Line 109: "Phases 2-4: Core infrastructure (models, repositories, APIs)"
- Multiple references to repository pattern implementation
- Test descriptions mention mocking

**Required Changes**:
- Replace "repositories" with "direct data access"
- Update test descriptions to mention real database with Respawn
- Clarify that Phase 3 was investigation, not implementation

## Secondary Gaps Needing Attention

### 4. Journey Files Mentioning Repositories

**Affected Files**:
- `/phases/phase-04-through-10/JOURNEY.md`
- `/phases/phase-05-process-engine-completion/JOURNEY.md`
- `/phases/phase-06-platform-services-completion/JOURNEY.md`

**Issues**: References to repository interfaces and implementations

**Required Changes**: Update to reflect direct DbContext usage

### 5. Test Strategy References

**Affected Files**:
- `/phases/dialectical-review-phase-4-7/JOURNEY.md`
- `/phases/epistemic-self-review/JOURNEY.md`

**Issues**: Mentions mocking repositories and services

**Required Changes**: 
- Clarify that only external services are mocked
- Emphasize real database testing with Respawn

### 6. DDD Pattern References

**Affected Files**:
- `/phases/phase-02-domain-models/JOURNEY.md`
- `/phases/phase-01-database/JOURNEY.md`

**Issues**: Positive references to DDD patterns without clarification

**Required Changes**:
- Add notes distinguishing ontology (good) from praxis (rejected)
- Clarify that entities are data projections, not domain objects with behavior

## Philosophical Misalignments

### 7. Process vs Entity Orientation

**Issue**: Some journey files discuss entity-centric design

**Reality**: Veritheia is process-oriented with anemic entities

**Required Clarification**: Intelligence lives in LLMs and Process Engine, not entities

### 8. Abstraction Philosophy

**Issue**: Multiple documents discuss "proper abstraction layers"

**Reality**: We reject unnecessary abstractions - EF Core and PostgreSQL provide what we need

**Required Clarification**: Direct usage is not "lacking abstraction" but deliberate simplicity

## PDCA Action Plan for Documentation Coherency

### PLAN - Identify and Prioritize Alignment Work

**Critical Documentation Fixes**:
- [ ] Inventory all files with architectural violation warnings
- [ ] Identify which represent historical investigation vs actual violations
- [ ] Determine which files need rewrite vs clarification notes
- [ ] Prioritize based on confusion potential for new contributors

**Implementation Integrity Verification**:
- [ ] Verify actual test count and coverage
- [ ] Assess real completion percentage vs claims
- [ ] Document gaps between specification and implementation
- [ ] Identify missing journey projection implementations

### DO - Execute Documentation Updates

**Rewrite Core Divergence Documentation**:
- [ ] Transform ARCHITECTURAL-DIVERGENCES.md into ARCHITECTURAL-IMPERATIVES.md
- [ ] Document current imperatives as authoritative guidance
- [ ] Include concrete code examples of correct patterns
- [ ] Add anti-pattern examples with explanations

**Update Historical Journey Files**:
- [ ] Add clarification headers to Phase 2 journey about DDD ontology vs praxis
- [ ] Update Phase 3 README to show investigation led to rejection
- [ ] Fix Phase 4-10 journey to clarify skeleton vs mock implementations
- [ ] Add notes to Phase 5 that MockCognitiveAdapter is correct (external service)

**Fix PROGRESS.md References**:
- [ ] Remove all references to repository implementations
- [ ] Update phase descriptions to reflect direct DbContext usage
- [ ] Correct test strategy descriptions (real DB with Respawn)
- [ ] Update completion percentages with honest assessment
- [ ] Add link to this ALIGNMENT-GAPS.md document

**Create Clarification Documents**:
- [ ] Write Phase 3 EPILOGUE.md explaining repository rejection rationale
- [ ] Create ARCHITECTURAL-PRINCIPLES.md as single source of truth
- [ ] Document why anemic domain model is correct for Veritheia
- [ ] Explain PostgreSQL as domain model participation

### CHECK - Verify Coherency Achievement

**Documentation Consistency Checks**:
- [ ] Verify no contradictions between VISION, ARCHITECTURE, IMPLEMENTATION, DESIGN-PATTERNS
- [ ] Ensure all journey files have appropriate historical context notes
- [ ] Confirm PROGRESS.md accurately reflects actual state
- [ ] Check that new contributors won't be misled

**Implementation Alignment Verification**:
- [ ] FIX: Database schema to use composite primary keys (UserId, Id)
- [ ] FIX: All indexes to start with UserId for partition locality  
- [ ] FIX: Foreign keys to respect partition boundaries
- [ ] CREATE: IUserOwned interface and query extensions
- [ ] REMOVE: All UseInMemoryDatabase from tests
- [ ] REMOVE: All Moq usage except for external services
- [ ] ADD: Respawn configuration for test cleanup
- [ ] IMPLEMENT: Journey projection space logic
- [ ] VERIFY: Services use DbContext directly (no repositories)
- [ ] VERIFY: Tests compile and run with real PostgreSQL

**Cross-Reference Validation**:
- [ ] All warning headers point to correct authoritative documents
- [ ] PROGRESS.md links to ALIGNMENT-GAPS.md
- [ ] Journey files reference appropriate architectural documents
- [ ] No orphaned or contradictory guidance remains

### ACT - Institutionalize Learning

**Establish Documentation Standards**:
- [ ] Create template for future journey investigations
- [ ] Document when historical preservation vs rewrite is appropriate
- [ ] Define process for architectural clarification documentation
- [ ] Establish review checklist for new documentation

**Prevent Future Divergence**:
- [ ] Add pre-implementation checklist referencing imperatives
- [ ] Create decision log template for architectural choices
- [ ] Document anti-patterns prominently
- [ ] Establish regular coherency review process

**Knowledge Transfer Preparation**:
- [ ] Create onboarding guide highlighting architectural imperatives
- [ ] Document common misconceptions and corrections
- [ ] Prepare FAQ addressing DDD confusion
- [ ] Build glossary of terms with Veritheia-specific meanings

## Implementation Notes

These gaps exist because the development occurred iteratively with evolving understanding. The journey files capture historical investigation that led to current imperatives. Rather than rewriting history, we should:

1. Preserve journey files as historical record
2. Add epilogues/notes explaining what we learned
3. Create clear summary of current imperatives
4. Ensure all new documentation follows imperatives

## Severity Assessment

### Why This is CRITICAL

1. **Partition Strategy Broken**: Without composite keys, cannot scale horizontally
2. **No User Isolation**: Queries can accidentally cross user boundaries  
3. **Foreign Keys Unsafe**: Can reference data from other users' partitions
4. **Tests Useless**: Mocked tests don't catch real constraint violations
5. **Architecture Compromised**: Core imperatives not implemented in code

### Impact if Not Fixed

- System cannot scale beyond single database
- User data not properly isolated
- Tests provide false confidence
- Future migration to fix keys will be extremely difficult
- Violates fundamental architectural promises

## Status

- **Created**: 2025-08-10
- **Updated**: 2025-08-11 - Added critical schema violations
- **Priority**: CRITICAL - System architecture fundamentally broken
- **Owner**: Development team
- **Deadline**: IMMEDIATE - Before any further development