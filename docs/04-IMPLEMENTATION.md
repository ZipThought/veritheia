# Veritheia Implementation

## I. Purpose and Alignment

The implementation of Veritheia exists to realize the commitments described in the Vision and Architecture. It does not merely stand up software; it implements the principles of intellectual sovereignty, journey-specific meaning, and projection-space formation at the level of code and runtime. 

The design philosophy draws from Domain-Driven Design's core insight—that software should model the problem domain with precision—but deliberately rejects its conventional patterns of repositories, aggregate wrappers, and mock-friendly indirection. Here, the schema is the domain model. The database constraints, type safety, and process orchestration are not hidden behind abstractions but are the very mechanisms that preserve the integrity of user-authored understanding.

This implementation is local-first by default, anti-surveillance by design, and partitioned so that each user's intellectual space remains sovereign. Every decision in this document exists to serve the ultimate purpose defined in the Vision: formation through authorship, never extraction through automation.

> **Formation Note:** The implementation philosophy directly enables formation through authorship. PostgreSQL isn't hidden behind abstractions because the schema itself implements intellectual sovereignty—composite keys create partition boundaries, foreign keys maintain journey context, constraints preserve integrity. When you author your framework in natural language, these database structures make it govern all processing without system override. The code doesn't just implement features; it mechanically applies your authorship.

## II. Development Philosophy: Progressive Enhancement

The system is constructed through progressive passes, each completing the entire vertical slice of the architecture before refinement begins:

**Skeleton Pass** – The structure stands: schema created, CRUD operations functional, projection spaces instantiated, UI displays live data. This is not a prototype but the first working system, crude in details but correct in architecture.

**Validation Pass** – Journey-aware queries replace generic ones, projection logic respects journey boundaries, assessment flows confirm that AI measures within the user's conceptual space. The system begins to embody its philosophy through actual use.

**Precision Pass** – Embedding generation tunes to user vocabularies rather than generic models. Prompts refine through observed assessment patterns. Measurements validate against actual research outcomes. The system achieves precision through deployment validation, not speculation.

**Production Pass** – Edge cases surface and resolve. Performance bottlenecks identify and optimize. Operational resilience confirms through stress testing. The system hardens for sustained intellectual work.

Nothing speculative is implemented early. Field names, prompt formats, indexing strategies—these mature through observed use, not assumption. The architecture is fixed; the details evolve.

## III. Core Runtime Components

### 3.1 Knowledge Database

PostgreSQL 17 with pgvector extension provides unified storage for documents, metadata, and embeddings. This is not a compromise for simplicity but a recognition that the database embodies the domain.

The schema models the domain directly. Foreign keys maintain intellectual context—PostgreSQL prevents a Journey from existing without a Persona because every inquiry requires a perspective. Check constraints implement discovered lifecycle states—a Journey can be Active, Paused, Completed, or Abandoned, not because we decided but because these are the states users actually experience. Partition keys create sovereignty boundaries—every significant table begins with user_id, establishing natural boundaries for both scaling and privacy.

All projection-space data—segmentation, embeddings, assessments—exists in relational and vector form within the same ACID boundary. When a document enters a journey, its segments, embeddings, and assessments commit atomically. There is no eventual consistency between document and vector stores because there is only one store. HNSW indexes on vector columns provide logarithmic retrieval complexity while maintaining transactional guarantees.

Cross-partition foreign keys do not exist. When users share content, the system creates explicit bridge records that reference across partitions through application logic rather than database constraints. These bridges are auditable, revocable, and maintain clear ownership chains. The database structure implements the ethical principle: intellectual work remains private by default, shareable by choice.

### 3.2 Process Engine

Implemented in ASP.NET Core 9.0, the Process Engine orchestrates analytical workflows as first-class runtime entities. Processes are not scripts or procedures but managed objects with lifecycle, state, and guarantees.

UUIDv7 primary keys provide temporal ordering without external sequence management. Every entity's ID encodes its creation time, enabling natural pagination and forensic analysis. The system uses Guid.CreateVersion7() natively, avoiding custom implementations or string conversions.

CQRS applies only as a conceptual separation between read and write flows. There is no Repository pattern hiding the database. Entity Framework Core maps directly to schema entities, allowing constraints to participate in execution. When a service attempts to create an invalid state, PostgreSQL rejects it immediately, not after layers of abstraction fail to validate.

Processes operate entirely within the user's projection space. The ProcessContext assembles user journey, persona elements, and journal entries into a coherent narrative that guides execution. Platform services—document ingestion, embedding generation, assessment orchestration—are guaranteed available to all processes, providing consistent capabilities while respecting journey boundaries.

### 3.3 Presentation Tier

Blazor Server provides the primary interface for deep intellectual work. The SignalR connection maintains stateful communication, preserving journey context across hours or days of engagement. This is not a limitation but a recognition: serious intellectual work requires sustained context, not stateless transactions.

The REST API serves a different purpose: headless automation, third-party integration, and eventual ecosystem participation. Each endpoint represents a bounded capability with explicit contracts. External systems can orchestrate Veritheia's capabilities without understanding its internal architecture.

Both interfaces consume the same service layer, ensuring behavioral consistency. A document ingested through the API behaves identically to one uploaded through Blazor. A process executed headlessly produces the same results as one run interactively. The interfaces differ in interaction model, not capability.

### 3.4 Cognitive System Integration

The ICognitiveAdapter interface abstracts integration with language models while preserving journey-specific assessment. Adapters exist for local inference (LlamaCpp), on-premise orchestration (Semantic Kernel), and cloud services (OpenAI, Anthropic). Each adapter receives the same journey-calibrated prompts and returns measurements, not interpretations.

The cognitive system never sees raw documents. It receives segments projected through the journey's conceptual framework, prompts containing the user's research questions, and rubrics derived from their assessment criteria. The AI measures within the projection space the user has defined. When it assesses relevance, it measures against the user's specific questions, not generic importance. When it evaluates contribution, it uses the user's definitions of value, not universal metrics.

## IV. Data Model: Projection Space in Practice

The entity model implements the three-layer architecture described in the Vision:

**Raw Corpus** remains immutable. Documents preserve their original structure, metadata, and content. The system never modifies source materials, only creates projections from them.

**Journey Projection Spaces** transform documents according to journey-specific rules. The same PDF might be segmented by section for systematic review, by paragraph for close reading, or by concept for thematic analysis. Each segmentation creates JourneyDocumentSegment records linked to the journey that defined them. Embeddings generate with the journey's vocabulary as context. Assessments measure against the journey's criteria.

**Knowledge Layer** provides the queryable API constrained to projection boundaries. Semantic search operates within journey-scoped indexes. Queries naturally filter by user_id and journey_id, enforcing both privacy and relevance.

Every table's primary key begins with user_id, creating natural clustering for partition-based scaling. Vector storage is polymorphic—separate tables for different embedding dimensions (search_vectors_1536, search_vectors_768, search_vectors_384) with metadata in search_indexes tracking which embeddings exist for which segments. HNSW indexes on each vector table provide efficient similarity search within the journey's projection space.

## V. Service and Process Architecture

Platform services maintain core invariants for all processes:

**Document Ingestion** preserves structure while extracting searchable content. PDFs maintain page boundaries. Academic papers preserve sections. Web content retains semantic HTML structure. The ingestion service never summarizes or interprets, only extracts and preserves.

**Projection-Aware Embedding** generates vectors within journey context. The same text embedded for a technical review includes technical vocabulary in its context. Embedded for philosophical analysis, it includes conceptual frameworks. The embedding service enables semantic search to operate within the user's intellectual space.

**Partition-Safe Search** respects user boundaries. Queries automatically scope to the authenticated user's partition. Cross-partition search requires explicit bridges with audit trails. The search service implements sovereignty at the query level.

**Context Assembly** constructs coherent narratives from journals, personas, and journey state. Recent entries receive priority. Significant entries (marked Critical or Milestone) always include. The assembly service provides processes with full user context while respecting token limits.

Processes follow a unified execution contract:

1. **Input Collection** through strongly-typed forms (UI) or validated JSON (API)
2. **Context Assembly** from journey state, relevant journals, and persona elements
3. **Execution** with full access to platform services and database within journey scope
4. **Result Persistence** with complete provenance and versioning
5. **Rendering** through process-specific UI components or structured API responses

The reference processes—Systematic Screening and Guided Composition—demonstrate analytical and compositional patterns while respecting user authorship. They show how processes can orchestrate complex workflows while ensuring insights emerge from user engagement, not system generation.

## VI. Extension Model

Extensions integrate at defined boundaries without modifying core architecture:

**Process Extensions** implement IAnalyticalProcess, gaining access to all platform services and guarantees. A specialized literature review process, a domain-specific analysis workflow, or a custom composition assistant—all inherit the same journey context, projection space, and sovereignty guarantees.

**Data Model Extensions** add entities through Entity Framework migrations that respect core constraints. Extension entities must link to appropriate aggregates (User, Journey, ProcessExecution) and honor partition boundaries. They cannot create cross-partition foreign keys or bypass journey scoping.

**UI Component Extensions** provide process-specific interfaces in Blazor. They receive journey context, can access projection spaces, and render results. They cannot access other users' data or bypass authentication.

All extensions inherit partition rules, context assembly, and projection-space scope. They extend capability while preserving sovereignty.

## VII. Testing Philosophy

Testing follows the architectural stance: the database is the domain, mocking it mocks reality.

**No Internal Mocking** – PostgreSQL, Entity Framework, and platform services execute as they will in production. Tests run against real database instances with Respawn resetting state between runs. This is slower than mocking but validates actual behavior, not imagined contracts.

**Unit Tests** are reserved for pure, deterministic functions. A method that parses markdown, calculates similarity scores, or transforms data structures warrants unit testing. These tests are fast, focused, and numerous.

**Integration Tests** validate service-database behavior. They confirm that services respect constraints, transactions maintain consistency, and queries return expected results. These tests are slower but essential—they validate the domain model's enforcement.

**End-to-End Tests** confirm full workflow coherence. From login through document upload, journey creation, process execution, and result retrieval—these tests validate that the system works as users experience it.

**Performance Tests** target known bottlenecks: vector similarity search, embedding generation, and projection space creation. They establish baselines and prevent regression. They run against realistic data volumes within journey-scoped boundaries.

## VIII. Security and Privacy

Security and privacy are not features but architectural foundations:

**TLS Everywhere** – All connections, even local development, use TLS. There is no "internal" network where encryption is optional.

**Encryption at Rest** – PostgreSQL Transparent Data Encryption protects persistent storage. Backups encrypt separately with rotation keys.

**No Analytics** – The system collects no usage analytics, generates no recommendations, and performs no cross-user analysis. Telemetry is limited to operational metrics: response times, error rates, resource utilization.

**Consent-Based Sharing** – Cross-user operations require explicit, revocable consent with full audit trails. Sharing creates bridges, not copies, preserving ownership chains.

**Local-First Default** – The system deploys locally by default. Cloud deployments must maintain full partition isolation, treating each user's partition as a separate security domain.

## IX. Deployment

The implementation deploys through progressive environments:

**Development** runs via Docker Compose with .NET Aspire orchestration. Hot reload enables rapid iteration. Structured logging provides detailed debugging. The Aspire dashboard shows distributed traces and metrics.

**Staging** deploys to Kubernetes with production-like configuration but relaxed resource limits. Integration tests run here. Performance baselines establish. Security scans execute.

**Production** runs on Kubernetes with full resilience: horizontal pod autoscaling, database replication, automated backups, and disaster recovery. Health checks monitor all services. Alerts trigger on anomalies. Feature flags control rollout.

Each environment maintains the same architectural guarantees: user sovereignty, projection-space isolation, and anti-surveillance design.

### Data Integrity Principles

**Authentic or Nothing**

Every vector, score, and extraction must originate from neural processing in production and development. The system provides no fallbacks, no degraded modes, no approximate processing. When the language model is unavailable, processing stops. When constraints are violated, transactions rollback. When validation fails, storage is denied. This binary choice—authentic or nothing—preserves formation integrity absolutely.

Test environments may inject test doubles for integration path validation when language models are unavailable, but these must be clearly isolated in test-only code paths, generate deterministic output, and never affect production builds. Production code must always fail when neural processing is unavailable.

**Systematic with Transparent Failures**

Every document must receive identical processing attempt with explicit failure tracking. No document gets silently skipped, simplified, or given different treatment. The mechanical systematic application continues through failures while recording each one. When processing 3,000 documents results in 153 failures, the user receives complete transparency: detailed success/failure breakdown with full failure context. The prohibition is against silent failures, not against continuing with explicit failure tracking.

**Transparent or Rejected**

Every system state must be accurately communicated or the operation is rejected. No hiding errors in logs. No catching exceptions without propagation. No returning defaults to avoid failure. The user's right to understand system state supersedes any desire for continuous operation.

## X. Closing Alignment

This implementation is the mechanical embodiment of the philosophical commitments in the Vision and the architectural structures in the Architecture. It does not attempt to generalize for market trends or mimic industry patterns that undermine sovereignty.

Every component is constructed to hold the line: between measurement and interpretation, between projection and corpus, between system capacity and human authorship. The code is not merely correct—it is aligned.

The database constraints teach what intellectual sovereignty requires. The type system enforces semantic precision. The process orchestration maintains journey coherence. The partition strategy ensures privacy by design. The testing philosophy validates reality, not abstractions.

This is implementation as philosophy made mechanical: every line of code, every schema constraint, every service boundary exists to preserve the human capacity to form understanding through engagement rather than consume it through extraction.