# Veritheia Architecture

### I. System Overview

The Veritheia architecture is a multi-tier system for the structured processing of a defined knowledge corpus. The design enforces a strict separation of concerns between data storage, process orchestration, and cognitive computation. The Process Engine acts as the central orchestrator, exclusively mediating interactions between the Knowledge Database and the Cognitive System.

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

This tier is a passive datastore that responds to structured queries from the Process Engine.

#### 2. The Process Engine

This is the core logical and orchestrating tier. It is the only component with the authority to query the Knowledge Database and dispatch tasks to the Cognitive System.

#### 3. The Presentation Tier

The Presentation tier is any client that interacts with the system via the Process Engine's formal API.

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

The Process Engine executes two distinct categories of processes.

*   **Static Processes (Core Services):** These are built-in, foundational processes required for the basic functioning and maintenance of the Knowledge Database. These include:
    *   Document rendering and text extraction from the Raw Corpus.
    *   Generation of baseline metadata.
    *   Maintenance of core relationships within the Processed Representation.

*   **Dynamic Processes (Analytical Workflows):** These are modular, extensible, and often user-initiated processes that perform analytical work. They interact with the Knowledge Database exclusively through the API provided by the Knowledge Layer. These workflows represent the formal methodologies for inquiry (e.g., Systematic Literature Review).

### VI. Integration Model

The architecture is designed for extensibility through a set of formal interfaces.

*   **Ingestion Connectors:** This is a defined interface that allows for the development of new data ingestion pathways. While the core system may only support direct file uploads, this model allows for the future addition of connectors for sources such as:
    *   Web scrapers
    *   Cloud storage providers (e.g., Google Drive, Dropbox)
    *   API-based data sources

    These connectors are considered extensions and are not part of the default implementation.
    
*   **Cognitive System Adaptors:** As specified in the Technology Specification, this interface decouples the Process Engine from the specific implementation of any given LLM or cognitive framework, allowing for flexible integration with local and external models.