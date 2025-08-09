# Phase 01: Database Infrastructure

## Status
**Current State**: Active  
**Started**: 2025-08-09  
**Completed**: -  

## The Question This Phase Answers

How do we store both structured knowledge and high-dimensional vectors in a way that preserves data sovereignty while enabling semantic search at scale?

## Relevant Specifications

- [IMPLEMENTATION.md#data-architecture](../../../docs/IMPLEMENTATION.md#data-architecture) - Database design requirements
- [ENTITY-RELATIONSHIP.md](../../../docs/ENTITY-RELATIONSHIP.md) - Schema specifications  
- [ARCHITECTURE.md#iv-data-model](../../../docs/ARCHITECTURE.md#iv-data-model) - Three-layer data model

## Current Understanding

Through dialectical investigation, I have come to understand that Veritheia's database is not about storing documents and chunks but about enabling **projection spaces** where documents are transformed through journey-specific intellectual frameworks.

### Key Discoveries

1. **Journey-Centric Architecture**: Documents don't have universal chunks or embeddings—these exist only within journey projections
2. **Projection Spaces**: Each journey creates its own intellectual space where documents are segmented, embedded, and assessed according to user-defined frameworks
3. **Formation Through Scale**: The system enables engagement with thousands of documents through measurement and projection, not summarization
4. **Cross-Disciplinary Bridges**: Different journeys can project the same documents differently, revealing shared phenomena across disciplines

### Critical Decisions

- PostgreSQL with pgvector for unified storage (not separate databases)
- UUIDv7 for temporal ordering (not ULID)
- HNSW indexing for vector search (not IVFFlat)
- Entity Framework Core with strategic raw SQL (not Dapper or pure SQL)
- Journey-specific projections (not universal processing)

### What Phase 1 Must Enable

The database infrastructure must support:
1. Journey framework definitions (research questions, vocabulary, criteria)
2. Journey-specific document projections
3. Progressive refinement tracking
4. Cross-journey concept mapping
5. Formation accumulation

This is not generic storage but infrastructure for intellectual sovereignty at scale.

## Navigation

- [JOURNEY.md](./JOURNEY.md) - The chronological record of investigation and decision
- [artifacts/](./artifacts/) - Code experiments, proofs, and fragments