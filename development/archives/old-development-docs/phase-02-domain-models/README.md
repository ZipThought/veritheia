⚠️ **ARCHITECTURAL IMPERATIVE VIOLATION WARNING** ⚠️
==================================================
THIS DOCUMENT CONTAINS OUTDATED PATTERNS THAT VIOLATE CURRENT ARCHITECTURAL IMPERATIVES:
- References repository pattern implementation in Phase 3

MARKED FOR REWRITE - DO NOT FOLLOW THESE PATTERNS
See: ARCHITECTURE.md Section 3.1, DESIGN-PATTERNS.md, IMPLEMENTATION.md
==================================================

# Phase 02: Core Domain Models

## Status
**Current State**: Active  
**Started**: 2025-08-09  
**Completed**: -  

## The Question This Phase Answers

How do we translate the journey projection database schema into domain models that preserve intellectual sovereignty while enabling clean separation between persistence and business logic?

## Relevant Specifications

- [CLASS-MODEL.md](../../../docs/CLASS-MODEL.md) - Domain class specifications
- [ENTITY-RELATIONSHIP.md](../../../docs/ENTITY-RELATIONSHIP.md) - Database schema from Phase 1
- [DESIGN-PATTERNS.md](../../../docs/DESIGN-PATTERNS.md) - Domain-Driven Design patterns

## The Tension to Resolve

Phase 1 implemented journey projection architecture in the database with `JourneyDocumentSegment` replacing the older `ProcessedContent` concept. Yet CLASS-MODEL.md shows both patterns. This investigation must resolve:

1. Should domain models mirror database entities exactly (anemic domain model)?
2. Should domain models encapsulate behavior (rich domain model)?
3. How do we handle the discrepancy between CLASS-MODEL.md and Phase 1 implementation?
4. Where does business logic live if not in domain models?

## What Phase 2 Must Enable

The domain models must:
1. Map to Phase 1 database entities
2. Preserve journey projection architecture
3. Enable repository pattern (Phase 3)
4. Support process engine operations (Phase 5)
5. Maintain intellectual sovereignty principles

## Navigation

- [JOURNEY.md](./JOURNEY.md) - The dialectical investigation
- [artifacts/](./artifacts/) - Code experiments and proofs