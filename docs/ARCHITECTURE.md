# Veritheia Architecture

## I. System Overview

Veritheia is an epistemic infrastructure that enables users to author understanding through structured engagement with source materials. The architecture implements a four-tier design: Knowledge Database for document storage and retrieval, Process Engine for workflow orchestration, Cognitive System for assessment operations, and Presentation tier for user interaction. Each component enforces the principle that insights emerge from user engagement, not system generation.


```
+-----------------------------------------------------+
|                  III. PRESENTATION                  |
|         (Client: Desktop, Web, CLI, API)            |
+-----------------------------------------------------+
                        ^
                        | (API Calls)
                        v
+-----------------------------------------------------+      +-----------------------------+
|                II. PROCESS ENGINE                   |<---->|    IV. COGNITIVE SYSTEM     |
|   (Stateful Workflow Orchestration & Logic)         |      | (Via Adaptor Interface)     |
+-----------------------------------------------------+      +-----------------------------+
                        ^
                        | (Data Operations)
                        v
+-----------------------------------------------------+
|               I. KNOWLEDGE DATABASE                 |
|      (Passive Datastore & Semantic API)             |
+-----------------------------------------------------+
```

## II. System Components

### 2.1 Knowledge Database

The Knowledge Database provides persistent storage for source documents and derived representations. It maintains three data layers: Raw Corpus (original documents), Processed Representation (embeddings, metadata, relationships), and Knowledge Layer (semantic query API). The database preserves provenance and versioning for all transformations.

### 2.2 Process Engine

The Process Engine executes analytical workflows through the IAnalyticalProcess interface. Each process maintains journey context, including user research questions, conceptual vocabulary, and formation markers. The engine provides platform services (document ingestion, embedding generation, metadata extraction) while ensuring process outputs reflect user interpretation rather than automated analysis.

### 2.3 Presentation Tier

The Presentation tier implements user interfaces for journey management, journal composition, and process execution. It maintains strict separation between user-authored content and system-provided structure. All displays reflect the user's developing understanding without imposing system-generated interpretations.

## III. Architectural Patterns

### 3.1 Database Architecture

The system employs PostgreSQL with pgvector extension for unified storage of relational and vector data. This design choice eliminates synchronization complexity between separate databases while maintaining query performance through appropriate indexing strategies (B-tree for relational queries, IVFFlat for vector similarity).

#### 2. Client Architecture

The Presentation tier implements a web-based interface that maintains architectural separation from backend services.

*   **Rationale:** This separation ensures that presentation logic remains independent of business logic and data access concerns.

#### 3. Cognitive System Integration: Assessment Engine

The Process Engine interacts with the Cognitive System as an assessment engine, not a decision maker.

*   **Function:** The cognitive adapter performs structured assessments in specific roles (librarian, peer reviewer, instructor) while the user maintains interpretive sovereignty.

*   **Key Principle:** AI performs assessments but users make decisions. In Systematic Screening, AI measures relevance and contribution, but researchers decide which papers are core, contextual, or peripheral to their inquiry.

*   **Rationale:** This ensures that:
    *   Understanding emerges from human interpretation of AI assessments
    *   Insights arise from the user's engagement with assessed materials
    *   Formation develops through accumulated insights across journeys
    *   The journey shapes how AI assessments are interpreted

The cognitive system measures and records rather than replacing human judgment. Each journey generates insights that contribute to formation.

### IV. Data Model

The data within the Knowledge Database is structured into three distinct, extensible layers. All computation and transformation between these layers is performed by the Process Engine.

*   **Raw Corpus:** This layer represents the ground truth. It consists of the original, unmodified source artifacts (e.g., PDF, text files, images) provided by the user.

*   **Processed Representation:** This layer contains structured data derived directly from the Raw Corpus. It is stored in the relational plane of the database and includes:
    *   Semantic extractions (entities, topics)
    *   Tags and metadata
    *   Summaries
    *   Extracted relationships
    *   Vector embeddings
    *   Full-text search indexes
    
    All entries in this layer are versioned and include metadata about their provenance (e.g., the model version used for summary generation, the embedding model name).

*   **Knowledge Layer:** This is the queryable, semantic API exposed by the Knowledge Database to the Process Engine. It provides high-level capabilities to access the underlying data, such as semantic search via vector distance calculations (e.g., cosine similarity).

### V. Process Model

The Process Engine executes two distinct categories of processes through a unified interface architecture.

*   **Platform Services:** These foundational capabilities ensure intellectual sovereignty:
    *   Document ingestion that preserves organizational structure
    *   Metadata extraction that respects user categorization
    *   Embedding generation that reflects conceptual space
    *   Indexing that supports search patterns
    *   Database operations that maintain journey context
    
    These services never generate insights—they prepare materials for analysis. They are triggered by user actions and serve the inquiry.

*   **Reference Processes:** The platform includes two fully-implemented processes that demonstrate the architecture:
    *   **SystematicScreeningProcess:** Analytical pattern for literature review with dual relevance/contribution assessment
    *   **GuidedCompositionProcess:** Compositional pattern for structured content creation with constraints and evaluation
    
    These reference implementations show how processes orchestrate intellectual work through the platform services.

*   **Process Categories:** Extensions typically fall into these patterns:
    *   **Methodological Processes:** Guide structured inquiry through established methodologies
    *   **Developmental Processes:** Scaffold skill development through progressive challenges
    *   **Analytical Processes:** Support pattern discovery through systematic examination
    *   **Compositional Processes:** Develop expressive capability through structured creation
    *   **Reflective Processes:** Deepen understanding through guided contemplation
    
    Every process produces outputs that are unique to the author—shaped by their questions, guided by their framework, and meaningful only within their journey.

#### Process Execution Architecture

All processes implement a common interface that enables uniform execution, monitoring, and result handling:

*   **Process Definition:** Metadata describing inputs, outputs, and execution requirements
*   **Process Context:** Runtime environment providing access to platform services
*   **Process Results:** Structured outputs with extensible schema for diverse result types

This architecture ensures that new processes can be added without modifying the core engine.

### VI. Integration Model

The architecture is designed for extensibility through a set of formal interfaces.

*   **Process Integration:** New analytical processes integrate through the common process interface, gaining access to all platform services and guarantees. This enables:
    *   Custom analytical workflows
    *   Domain-specific methodologies
    *   Specialized result renderers
    *   Process composition patterns

*   **Ingestion Connectors:** This is a defined interface that allows for the development of new data ingestion pathways. While the core system may only support direct file uploads, this model allows for the future addition of connectors for sources such as:
    *   Web scrapers
    *   Cloud storage providers (e.g., Google Drive, Dropbox)
    *   API-based data sources

    These connectors are considered extensions and are not part of the default implementation.
    
*   **Cognitive System Adaptors:** This interface decouples the Process Engine from the specific implementation of any given LLM or cognitive framework, allowing for integration with local and external models.

### VII. Extension Architecture

The system implements extensibility through composition rather than modification.

#### Process Extension Model

All processes implement the `IAnalyticalProcess` interface. The platform provides two reference implementations that demonstrate the pattern:

```
Core Platform
├── Process Engine (execution runtime)
├── Platform Services (guaranteed capabilities)
└── Reference Processes
    ├── SystematicScreeningProcess
    └── GuidedCompositionProcess

Extensions
├── Methodological Processes (research methodologies)
├── Developmental Processes (skill progression)
├── Analytical Processes (domain-specific analysis)
├── Compositional Processes (creative workflows)
└── Reflective Processes (contemplative practices)
```

#### Extension Capabilities

Extensions are full-stack components that may include:

*   Process implementation (`IFormationProcess`)
*   Journey-specific data models (Entity Framework entities)
*   UI components that reflect authorship (Blazor components)
*   Formation services
*   Personal result renderers

#### Platform Service Guarantees

Extensions rely on these always-available services:

*   Document processing pipeline
*   Embedding generation and storage
*   Knowledge database operations
*   Cognitive system access
*   User context management
*   Result persistence and versioning

These services are provided through dependency injection and maintain consistent interfaces across versions.

#### Process Context Flow

Every process execution receives a `ProcessContext` containing:

*   Current knowledge scope
*   User journey history
*   Process-specific inputs
*   Execution metadata
*   Platform service references

This context ensures outputs remain personally relevant and meaningful within the specific inquiry.

#### Extension Registration

```csharp
// Core processes (included)
services.AddProcess<SystematicScreeningProcess>();
services.AddProcess<GuidedCompositionProcess>();

// Extended processes (additional)
services.AddProcess<YourCustomProcess>();
```

#### Data Model Extensions

Extensions can define their own entities that integrate with the core schema:

```csharp
public class ProcessSpecificData : BaseEntity
{
    public Guid ProcessExecutionId { get; set; }
    public ProcessExecution ProcessExecution { get; set; }
    // Process-specific properties
}
```

The platform handles migrations and ensures data consistency across extensions. See [EXTENSION-GUIDE.md](./EXTENSION-GUIDE.md) for implementation details.

### VIII. User and Journey Architecture

The system models users as authors of their own understanding through a journey and journal system.

#### User Model

Users are the constant in the system, maintaining:
- **Identity**: Authentication and basic profile
- **Persona**: Evolving representation of their intellectual style
- **Knowledge Base**: Their corpus of documents
- **Capabilities**: Process access permissions

#### Journey Model

Journeys represent specific instances of users engaging with processes:
- Each journey = User + Process + Purpose + Time
- Journeys maintain state and context specific to that intellectual endeavor
- Multiple journeys can exist for the same user-process combination
- Journeys are inherently personal and non-transferable

#### Journal System

Journals capture the narrative of intellectual development:
- Multiple journals per journey (Research, Method, Decision, Reflection)
- Written as coherent narratives, not logs
- Assembled into context for process execution
- Designed for potential future sharing while maintaining privacy in MVP

#### Context Management

The system assembles context from:
- Current journey state and purpose
- Relevant journal entries
- Persona elements
- Process-specific needs

Context is managed to fit within cognitive system limits while maintaining narrative coherence and the user's voice.

See [USER-MODEL.md](./USER-MODEL.md) for detailed specifications.