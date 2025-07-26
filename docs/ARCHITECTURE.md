# Veritheia Architecture

### I. System Overview

The Veritheia architecture is designed to ensure that users author their own understanding rather than consume system-generated insights. Every architectural decision supports this principle: the system amplifies human intellectual capability without replacing human judgment. The Process Engine orchestrates the journey through knowledge, the Knowledge Database preserves conceptual connections, and the Cognitive System responds to questions—never thinking for users, always thinking with them.

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

### II. System Components

#### 1. The Knowledge Database

This tier preserves the sources and connections discovered through inquiry. It does not generate insights but maintains the raw materials from which understanding is composed.

#### 2. The Process Engine

This tier orchestrates the intellectual journey. It ensures that every process reflects the user's questions, follows their conceptual path, and produces results that bear their intellectual fingerprint.

#### 3. The Presentation Tier

This tier presents the user's work back to them. It does not display system conclusions but reflects developing understanding, discovered connections, and authored insights.

### III. Architectural Patterns

#### 1. Database Architecture

The Knowledge Database employs a unified storage model that supports both structured relational data and high-dimensional vector representations.

*   **Rationale:** A unified storage approach reduces operational complexity and ensures data consistency across different representation types.

#### 2. Client Architecture

The Presentation tier implements a web-based interface that maintains architectural separation from backend services.

*   **Rationale:** This separation ensures that presentation logic remains independent of business logic and data access concerns.

#### 3. Cognitive System Integration: Adapter Pattern

The Process Engine interacts with the Cognitive System through a standardized adapter interface.

*   **Function:** This pattern defines a contract for cognitive operations (text generation, embedding creation) while abstracting implementation details.

*   **Rationale:** The adapter pattern enables deployment-time selection of cognitive backends without modifying core application logic. This supports integration with:
    *   Abstraction frameworks
    *   Vendor-specific implementations
    *   Local inference engines

This architectural pattern ensures the system remains agnostic to specific cognitive implementations.

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

*   **Static Processes (Core Platform Services):** These foundational capabilities ensure intellectual sovereignty:
    *   Document ingestion that preserves organizational structure
    *   Metadata extraction that respects user categorization
    *   Embedding generation that reflects conceptual space
    *   Indexing that supports search patterns
    *   Database operations that maintain journey context
    
    These processes never generate insights—they prepare materials for analysis. They are triggered by user actions and serve the inquiry.

*   **Dynamic Processes (Analytical Workflows):** These orchestrate intellectual work:
    *   Systematic screening that applies user-defined criteria
    *   Research question evolution through engagement
    *   Relevance assessment based on personal understanding
    *   Analytical pipelines shaped by individual methodology
    
    Every Dynamic Process produces outputs that are unique to the author—shaped by their questions, guided by their framework, and meaningful only within their journey.

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
    
*   **Cognitive System Adaptors:** As specified in the Technology Specification, this interface decouples the Process Engine from the specific implementation of any given LLM or cognitive framework, allowing for flexible integration with local and external models.

### VII. Extensibility Architecture

The system implements extensibility through composition rather than modification.

#### Process Interface Contract

All processes, whether built-in or extended, implement a uniform interface that provides:

*   Process metadata and input requirements
*   Execution logic with access to platform services
*   Result rendering specifications
*   Event trigger definitions (for automatic processes)

#### Platform Service Guarantees

Static Processes provide guaranteed services that all extensions can depend upon:

*   Document processing pipeline
*   Embedding generation and storage
*   Knowledge database operations
*   Cognitive system access
*   Result persistence

These services are always available and maintain consistent quality across all processes.

#### Extension Pattern

New capabilities are added by implementing the process interface and registering with the Process Engine. The MVP demonstrates this pattern through its built-in analytical processes, which use the same extension mechanisms that future processes will employ.