# Development Documentation Alignment Gaps

## Overview

This document inventories gaps and divergences between development documentation and the current architectural imperatives established in ARCHITECTURE.md, IMPLEMENTATION.md, and DESIGN-PATTERNS.md.

## Critical Gaps Requiring Immediate Resolution

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

## Recommended Resolution Approach

### Phase 1: Critical Updates (Immediate)
1. Add EPILOGUE.md to Phase 3 explaining rejection
2. Rewrite ARCHITECTURAL-DIVERGENCES.md completely
3. Update PROGRESS.md phase descriptions

### Phase 2: Journey Updates (Next)
1. Add clarification notes to affected journey files
2. Update test references throughout
3. Clarify DDD stance in domain model discussions

### Phase 3: Consolidation (Final)
1. Create ARCHITECTURAL-PRINCIPLES.md summarizing imperatives
2. Update README.md in development folder
3. Add cross-references to main documentation

## Implementation Notes

These gaps exist because the development occurred iteratively with evolving understanding. The journey files capture historical investigation that led to current imperatives. Rather than rewriting history, we should:

1. Preserve journey files as historical record
2. Add epilogues/notes explaining what we learned
3. Create clear summary of current imperatives
4. Ensure all new documentation follows imperatives

## Status

- **Created**: 2025-08-10
- **Priority**: High - causes confusion for new contributors
- **Owner**: Development team
- **Deadline**: Before Phase 11 UI implementation