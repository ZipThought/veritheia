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

> **Formation Note:** This architecture ensures that every technical component serves a single purpose: enabling users to author their own understanding. The four-tier design isn't arbitrary—each tier maintains a boundary that preserves user sovereignty. The Knowledge Database stores documents without interpretation. The Process Engine orchestrates without deciding. The Cognitive System measures without concluding. The Presentation tier displays without generating.

## II. System Components

### 2.1 Knowledge Database

**Implementation Priority: P0-Foundation**  
The Knowledge Database must exist before any other component can function. It forms the foundational storage layer for all user documents and journey projections.

The Knowledge Database provides persistent storage for source documents and derived representations. It maintains three data layers: Raw Corpus (original documents), Processed Representation (embeddings, metadata, relationships), and Knowledge Layer (semantic query API). The database preserves provenance and versioning for all transformations.

### 2.2 Process Engine: Neurosymbolic Orchestration

**Implementation Priority: P1-Core**  
The Process Engine depends on the Knowledge Database and enables all journey-based processing. Without it, documents cannot be projected through user frameworks.

The Process Engine implements **neurosymbolic architecture, transcended**—the critical innovation that differentiates Veritheia from all legacy systems. Unlike traditional neurosymbolic systems where symbolic rules are coded in formal languages (Prolog, LISP, Python), Veritheia transcends this limitation by enabling users to author their symbolic systems in natural language. The user's research questions, definitions, and criteria ARE the symbolic framework, interpreted and applied through neural semantic understanding. This transcendence means every user becomes a knowledge engineer without knowing programming—they author the symbolic system that governs document processing simply by expressing their intellectual framework in their own words.

> **Formation Note: The Transcendent Innovation** - This neurosymbolic transcended architecture is what makes formation through authorship possible. Traditional systems require programmers to encode rules; Veritheia enables users to author their own symbolic systems through natural language. When you write "relevant papers must provide empirical evidence," that natural language statement becomes the symbolic rule governing assessment. You are not using the system—you are authoring the symbolic framework that defines how the system operates within your journey.

**As Demonstrated in Foundational Research:**
- **[LLAssist](./papers/2407.13993v3.pdf)** processed datasets of 17, 37, 115, and 2,576 articles with identical methodology - demonstrating mechanical orchestration that scales without bias
- **[EdgePrompt](./papers/3701716.3717810.pdf)** applied the same evaluation framework to ALL student responses regardless of quality - showing systematic fairness through mechanical processing
- **[Contextualized AI](./papers/2409.13524v1.pdf)** Method B used consistent multi-stage processing across large document sets - exemplifying systematic application of user-defined frameworks

The Process Engine operates through **projection spaces**—journey-specific intellectual environments where documents are transformed according to user-authored frameworks that become dynamic symbolic systems. The Process Engine mechanically ensures that:

1. **ALL Documents Receive Identical Treatment**: Whether processing 200 papers or 3000 papers, every single document gets the same systematic processing through the user's framework
2. **No Selective Processing**: The engine mechanically processes every document without LLM judgment about which documents are "worth" processing
3. **Complete Coverage Guarantee**: The mechanical orchestration ensures no document is skipped or treated differently
4. **Framework Consistency**: The same user-authored symbolic framework gets applied to every document systematically

**Within each document processing step, the neurosymbolic transcendence occurs:**

*   **User-Authored Symbolic Framework**: The user's natural language framework becomes their personal symbolic system
*   **Neural Semantic Understanding**: The LLM comprehends the user's authored symbolic framework and applies that understanding to each document
*   **Mechanical Application**: The Process Engine mechanically ensures this neural understanding of the symbolic framework gets applied to ALL documents

**The Projection Process:**
1. **Mechanical Document Iteration**: Process Engine systematically takes each document in the corpus
2. **Neural Framework Comprehension**: LLM semantically understands the user's authored framework for this specific document
3. **Symbolic Application**: The user's framework (as symbolic system) gets applied through neural semantic understanding
4. **Mechanical Storage**: Results are systematically stored for user engagement
5. **Formation Enablement**: User develops understanding through engagement with systematically processed documents

This projection mechanism enables scale—thousands of documents become tractable through mechanical systematic processing guided by neural understanding of user-authored symbolic frameworks—while preserving intellectual sovereignty through complete, fair, and consistent treatment of ALL documents.

### 2.3 Presentation Tier

**Implementation Priority: P2-MVP**  
The Presentation tier requires both Knowledge Database and Process Engine to function. It provides the interface through which users engage with their projected documents.

The Presentation tier implements user interfaces for journey management, journal composition, and process execution. It maintains strict separation between user-authored content and system-provided structure. All displays reflect the user's developing understanding without imposing system-generated interpretations.

## III. Architectural Patterns

### 3.1 Database Architecture

The system employs PostgreSQL as an Object-Relational Database Management System (ORDBMS), leveraging its full capabilities rather than treating it as a simple data store. PostgreSQL's pgvector extension provides semantic search through high-dimensional vector operations, indexed using Hierarchical Navigable Small World (HNSW) graphs for logarithmic query complexity even at scale. The JSONB data type stores semi-structured data with full indexing support, enabling flexible schema evolution within rigorous relational boundaries. Array types capture multi-valued attributes without junction tables, while range types represent intervals with proper algebraic operations. This unified approach eliminates the synchronization complexity that would arise from separating relational, document, and vector stores into distinct systems.

The database embodies the core domain rather than serving as infrastructure. This architectural principle recognizes that in knowledge management systems, the schema defines not just data storage but the fundamental relationships that constitute understanding. When PostgreSQL enforces that every Journey must reference a valid Persona, this constraint expresses a domain truth: intellectual work requires context. When foreign keys prevent DocumentSegments from existing without their source Documents, they preserve the provenance chain essential to epistemic integrity. When check constraints limit JourneyState to specific values, they encode the discovered lifecycle of intellectual engagement.

> **Formation Note:** These database constraints aren't arbitrary technical decisions—they encode discovered truths about intellectual work. A Journey without a Persona would be inquiry without perspective, which we've learned is impossible. A DocumentSegment without its source Document would be insight without provenance, breaking the chain of understanding. The schema enforces what formation requires.

This domain-centric database architecture has profound implications for testing strategy. Since the database enforces business invariants through its schema, attempts to mock it would bypass the very rules that define system correctness. A mock that allows a Journey without a User would permit states the domain considers impossible. Therefore, all tests execute against real PostgreSQL instances, using Respawn to restore clean state between test runs. This approach validates not just application logic but the full stack of domain rules from constraint to code. Only truly external services—language models, file storage systems, third-party APIs—warrant mocking, as they exist outside the domain boundary.

This architecture represents a deliberate departure from Domain-Driven Design's practical patterns while embodying its deeper ontology and telos. DDD's praxis—with its Repositories abstracting persistence, its Aggregates maintaining consistency boundaries, its Value Objects ensuring immutability—assumes the database is mere infrastructure to be hidden behind abstractions. This separation makes sense when the domain logic is complex and the persistence mechanism is incidental. However, in Veritheia, the relational model IS the domain model. The foreign keys ARE the aggregate boundaries. The constraints ARE the business rules. To abstract PostgreSQL behind Repository interfaces would be to deny its participation in domain modeling, reducing a sophisticated ORDBMS to a dumb store. 

We embrace DDD's ontology—that software should model the problem domain with precision—by recognizing PostgreSQL's schema as a first-class expression of that domain. We honor DDD's telos—maintaining model integrity through explicit boundaries—by letting PostgreSQL enforce those boundaries through referential integrity and check constraints. The database is not infrastructure supporting the domain; the database schema is the domain's foundational expression. This is why we reject DDD's implementation patterns while achieving its philosophical goals through deeper integration with our chosen persistence mechanism.

This philosophical stance extends to our testing strategy through the explicit rejection of internal mocking. Modern testing practices often advocate mocking every dependency—databases, services, even internal components—to achieve "unit" isolation. This approach assumes components are interchangeable parts that can be validated in isolation. But in a system where PostgreSQL's constraints participate in domain logic, where services orchestrate complex workflows with transactional guarantees, and where the interaction between components defines correctness, such isolation is illusion. A UserService tested with a mocked database that permits invalid states teaches nothing about system behavior. A ProcessEngine tested with mocked services that return predetermined responses validates nothing about actual workflow execution.

Therefore, we mock only external dependencies—language models whose responses vary, file systems whose availability fluctuates, third-party APIs whose behavior we cannot control. Everything within our stack—database, services, domain logic—must be tested as it actually operates. This means integration tests that exercise real database constraints, service tests that validate actual transaction boundaries, and end-to-end tests that confirm the full stack operates coherently. The temporary inconvenience of slower test execution is offset by the permanent confidence that our tests validate actual system behavior rather than mocked approximations. When a test passes, it means the system works, not merely that our mocks align with our assumptions.

Unit tests have their place, but that place is narrow: stateless building blocks and encapsulated deterministic transformations. A function that converts Markdown to HTML warrants unit testing. A method that calculates embedding similarity deserves isolated validation. A utility that parses document metadata benefits from focused assertion. These components exhibit deterministic behavior—given input X, they produce output Y regardless of context. But the moment behavior depends on state, transaction boundaries, or system interaction, unit tests become deceptive. They validate what the programmer imagined rather than what the system does. A UserService's CreateUser method cannot be meaningfully unit tested because its correctness depends on database constraints, transaction semantics, and cascade behaviors that mocks cannot faithfully reproduce. System-level behavior emerges from component interaction, not component isolation. Therefore, we test at the level where behavior manifests: integration tests for service orchestration, end-to-end tests for user workflows, and unit tests only for pure transformations.

The JSONB fields within entities like Persona demonstrate controlled flexibility within structure. The ConceptualVocabulary field stores domain-specific terminology as nested JSON while maintaining foreign key integrity to its owning User. The Patterns array captures recurring intellectual structures without requiring a predetermined schema. These semi-structured elements evolve with user understanding while the relational skeleton maintains system coherence. The database thus provides both the stability required for long-term knowledge preservation and the flexibility necessary for intellectual growth.

HNSW indexing on vector columns enables semantic search as a first-class domain operation rather than an external service. When the system searches for documents similar to a query, it performs this operation within the same transactional context as relational queries. A single query can join semantic similarity with relational filters, maintaining ACID properties across both vector and scalar operations. This unified querying eliminates the eventual consistency problems that plague systems splitting these concerns across multiple databases.

#### 2. Client Architecture

The Presentation tier implements two distinct interfaces serving complementary purposes: Blazor Server for full-capability interaction and Web API for headless extensibility.

Blazor Server provides direct access to the system's complete functionality through stateful, real-time, bidirectional communication. The SignalR connection maintains live server state, enabling responsive UI updates without request-response overhead. Complex workflows like iterative document assessment, real-time journey progression, and interactive formation development operate through this channel without translation layers between server logic and UI state. This approach trades consumer internet compatibility—requiring persistent WebSocket connections, consuming server memory per user, depending on .NET runtime—for development efficiency and capability depth. The UI components directly invoke service methods, share domain models, and participate in transactions without serialization boundaries. This is the interface for serious users engaged in sustained intellectual work, where capability matters more than scale.

The Web API serves a different constituency: headless automation, third-party integration, and eventual internet-scale exposure. RESTful endpoints provide stateless access to core operations—document ingestion, journey initiation, process execution, result retrieval. Each endpoint represents a bounded capability with explicit contracts, versioning, and authentication. External systems can orchestrate Veritheia's capabilities without understanding its internal architecture. Future consumer applications can access selective functionality through gateway patterns that manage resource consumption and enforce usage boundaries. This is the interface for ecosystem participation, where interoperability matters more than depth.

This dual-interface architecture reflects a fundamental recognition: different consumers require different interaction models. Power users engaged in complex intellectual work benefit from Blazor's rich, stateful interaction. Automated systems and lightweight consumers need the simplicity and standardization of REST APIs. By providing both, the system avoids forcing either constituency into an inappropriate interaction pattern. The same service layer supports both interfaces, ensuring consistency while allowing each to optimize for its specific use case.

#### 3. Scalability Through Neurosymbolic Transcendence

The system's scalability model derives from its neurosymbolic transcended architecture: mechanical orchestration of user-authored symbolic frameworks through neural semantic understanding enables unprecedented scale while maintaining formation through authorship.

**Demonstrated Scalability in Foundational Research:**
- **[LLAssist](./papers/2407.13993v3.pdf)** scaled from 17 articles to 2,576 articles using identical methodology - the mechanical orchestration handled 150x volume increase without degradation
- **[EdgePrompt](./papers/3701716.3717810.pdf)** processed ALL student responses (whether 10 or 100) with identical evaluation framework - demonstrating mechanical fairness that scales
- **[Contextualized AI](./papers/2409.13524v1.pdf)** Method B processed large datasets through multi-stage consistent frameworks - showing systematic processing scales across volume

**Neurosymbolic Scalability Model:**

The mechanical orchestration component scales deterministically - whether processing 100 or 10,000 documents, the same systematic steps are applied to each document. The neural semantic understanding component applies the user's authored symbolic framework consistently at any scale. This creates **linear computational complexity** where doubling the documents doubles the processing time, but maintains identical quality and completeness.

**Formation-Centric Scalability:** Each user's journey creates a bounded projection space containing only the documents relevant to their inquiry. A researcher examining 3,000 papers doesn't burden the system with 3,000 global documents but creates a focused lens through which those documents gain meaning through their authored framework. Ten thousand users each with their own 3,000-document corpus don't create a system managing 30 million documents but rather 10,000 individual formation spaces, each internally coherent and bounded by user-authored symbolic systems.

The stateful Blazor connections that would seem unscalable for consumer internet become entirely appropriate for this model. A user engaged in deep intellectual work maintains a session for hours or days, not seconds. The server resources dedicated to maintaining their state pale compared to the intellectual value being created. This is not a system where millions browse casually but where thousands engage seriously. The "scalability problem" of maintaining state per user becomes the scalability solution of maintaining context per journey.

Even the database architecture supports this formation-centric scalability. HNSW indexes on journey-specific embeddings mean semantic search operates within bounded spaces rather than global ones. Queries that would be intractable across millions of documents become efficient within journey projections. The same document embedded differently in different journeys doesn't create redundancy but rather multiple lenses of understanding—each optimized for its specific inquiry.

This is scalability rightly understood: not as mechanical reproduction of knowledge artifacts but as parallel formation of individual understanding. The system scales by supporting more journeys, not by processing more data. It scales by deepening engagement, not by broadening reach. It scales as a formative tool that remains responsive to individual intellectual need regardless of how many individuals it serves.

The practical implementation of this scalability philosophy manifests in the database architecture: the natural partition key is the user. Every significant entity—Journeys, Personas, Documents, Formations—relates back to a specific user. This creates natural sharding boundaries that enable horizontal scaling without sacrificing transactional integrity. A user's entire intellectual workspace—their documents, journeys, assessments, formations—can reside on a single node, maintaining ACID guarantees for all operations within their formation space. 

Cross-user operations are not rare but explicit—they occur only with conscious user consent. When a researcher shares their journey for peer review, when collaborators merge their formations, when knowledge transfers between accounts, these operations cross partition boundaries by design. The system treats such operations as special events requiring explicit authorization, careful orchestration, and often asynchronous processing. This isn't a limitation but a feature: the boundary crossing forces deliberation about intellectual property, attribution, and consent. When the system needs to scale beyond a single database server, it partitions by user ID, achieving effectively infinite horizontal scalability. Each shard maintains full PostgreSQL capabilities—foreign keys, constraints, transactions—within its user partition. Cross-partition operations, when explicitly requested, execute through carefully designed protocols that maintain consistency while respecting sovereignty. The system thus scales not by weakening its guarantees but by recognizing that the domain naturally partitions along user boundaries, with explicit bridges where users choose to connect.

This partitioning strategy has immediate implications for database design. Every table's primary key begins with user_id, creating natural clustering. Indexes are structured as (user_id, ...) to maintain partition locality. Foreign keys reference within the same user's partition, never across. HNSW vector indexes are built per-user, keeping semantic search bounded. When a user shares content, the system creates explicit "bridge" records that reference across partitions through application logic rather than foreign keys. These bridges are auditable, revocable, and maintain clear ownership chains. The practical result: each user's data forms a self-contained universe that can be moved, backed up, or deleted as a unit, while still participating in larger collaborative structures when explicitly authorized.

Identity in this architecture is sovereign at the conceptual layer. Authorization belongs to the user, not the system—users grant access to their intellectual work, the system merely enforces their decisions. Authentication serves to verify that the right person is accessing their own data, not to gate-keep system resources. This inverts traditional access control: instead of the system granting users permission to use its features, users grant the system permission to operate on their behalf. A user's login doesn't request access to Veritheia; it establishes ownership of their partition. Their documents, journeys, and formations are not "in" the system but "theirs" within a system that provides computational infrastructure. This sovereignty extends to data portability—a user can export their entire partition, run their own instance, or transfer their intellectual workspace elsewhere. The system is custodian, not owner, of user understanding.

The architecture is anti-surveillance by design while enabling explicit, consensual data sharing. No global queries traverse user partitions. No analytics aggregate across journeys. No recommendation engines mine collective behavior. The system literally cannot observe patterns across users because the database structure prevents it—foreign keys don't cross partition boundaries, indexes are user-scoped, and queries are naturally limited to authenticated partitions. Even system administrators cannot casually browse user content; access requires deliberate action that leaves audit trails. 

Yet the same architecture that prevents surveillance enables rich sharing when users choose it. A researcher can publish their journey for peer review, creating an explicit bridge that others can traverse with permission. Collaborators can federate their formation spaces, maintaining distinct ownership while enabling cross-pollination. Knowledge can be transferred, cited, and built upon—but only through conscious acts of sharing that preserve attribution chains. The technical mechanism enforces the ethical principle: intellectual work remains private by default, shareable by choice, and never subject to ambient surveillance. The system knows only what users explicitly choose to share, when they choose to share it, with whom they choose to share.

Note: Not all capabilities described in this architecture—particularly cross-user sharing, federation, and multi-node partitioning—will be available in the MVP or even the first release. However, the system is designed from the foundation to support these capabilities without architectural revision. The database schema, partition strategy, and identity model are structured to enable these features when needed. Building with the end in mind ensures that early implementation decisions don't preclude future capabilities. The MVP focuses on single-user formation within a monolithic deployment, but every design choice preserves the path to collaborative, distributed operation.

#### 3. Neurosymbolic Architecture: Transcended Integration

Veritheia implements a neurosymbolic architecture that transcends traditional approaches by transforming user-authored natural language frameworks into dynamic symbolic systems. This transcendence manifests through the mechanical orchestration of neural semantic understanding applied to user-defined intellectual structures.

The architecture draws directly from foundational research that demonstrates this transcendent integration in practice. EdgePrompt (Syah et al., 2025) establishes the neurosymbolic pattern where teacher-authored rubrics and safety constraints function as the symbolic system, while large language models provide neural comprehension, with mechanical orchestration guaranteeing identical treatment across all student responses regardless of volume or quality variation. LLAssist (Haryanto, 2024) exemplifies this architecture through systematic processing that scales from 17 to 2,576 academic papers while maintaining identical evaluation methodology, demonstrating how researcher-authored questions and definitions create personalized symbolic frameworks that neural systems can comprehend and apply consistently. The Cognitive Silicon framework (Haryanto & Lomempow, 2025) provides the philosophical foundation by establishing formation through authorship as the core principle, where users create their intellectual frameworks as living symbolic systems and develop understanding through engagement with systematically processed results.

The transcendent neurosymbolic design integrates three essential components that operate in coordination rather than isolation. The neural component, implemented through large language models, provides semantic understanding of user-authored natural language frameworks, interpreting complex intellectual stances expressed in natural discourse rather than formal notation. The symbolic component emerges from the user's intellectual framework itself, which becomes the symbolic system governing processing—not predetermined rules encoded by system designers, but authored intellectual stances that reflect individual theoretical orientations, research methodologies, and assessment criteria. The mechanical orchestration, implemented through the Process Engine, ensures systematic application of the symbolic framework derived from neural understanding to every document in the corpus without exception, maintaining consistency and fairness through deterministic processing rather than selective judgment.

The user-authored symbolic systems distinguish this architecture from traditional neurosymbolic approaches that rely on hardcoded symbolic rules. Users express their intellectual frameworks through natural language discourse that reflects their authentic scholarly voice: research questions articulated as the researcher would naturally phrase them within their disciplinary context, definitions that embody the user's theoretical perspective and specialized vocabulary, assessment criteria that express their scholarly expectations and methodological standards, and comprehensive approaches described in their own intellectual idiom rather than formalized notation.

Neural semantic understanding operates through large language models that provide comprehensive interpretation of these user-authored frameworks. The neural component comprehends research intent holistically rather than parsing discrete components, applying semantic understanding that encompasses the user's definitions, criteria, and methodological stance as an integrated intellectual position. Each document receives processing through the lens of the user's complete expressed intellectual stance, creating symbolic processing systems that are entirely unique to each user's authored framework and producing fundamentally different analytical outcomes even when applied to identical source materials.

Mechanical systematic application ensures absolute consistency through deterministic orchestration. The Process Engine mechanically guarantees that every document receives identical treatment regardless of scale—whether processing responses from 10 students or 100 students, analyzing 200 academic papers or 3,000 papers, every item in the corpus undergoes the same systematic processing through the user's framework. No neural judgment determines processing priority or scope; mechanical orchestration ensures complete coverage without selective attention or qualitative filtering. The user's authored framework functions as the governing symbolic system that gets systematically applied without exception, creating consistency and fairness through deterministic application rather than artificial intelligence discretion.

This architecture enables transcendent formation by synthesizing user authorship with systematic processing. Users create their own symbolic systems through natural language frameworks that express their unique intellectual positions, while neural understanding provides semantic interpretation that enables systematic application of these authored systems to large document corpora. Mechanical orchestration ensures that this processing occurs without bias, omission, or inconsistency, creating conditions where formation emerges from authentic engagement with documents that have been systematically processed through the user's own authored intellectual framework.

This transcends traditional neurosymbolic approaches by making the symbolic component user-authored and dynamically created for each journey, while maintaining mechanical systematic application through neural semantic understanding.

### IV. Data Model: Journey Projection Spaces

The data model implements a fundamental principle: **documents don't have inherent meaning—meaning emerges through projection into journey-specific intellectual spaces**.

#### The Three-Layer Architecture

*   **Raw Corpus:** This layer represents the ground truth. It consists of the original, unmodified source artifacts (e.g., PDF, text files, images) provided by the user. Documents exist here without interpretation.

*   **Journey Projection Spaces:** Each journey creates a unique projection space where documents are transformed according to the user's natural language framework:
    *   **Semantic Segmentation**: Documents are divided according to what the LLM understands from the user's natural language description of their approach
    *   **Contextualized Embedding**: Vectors are generated with the LLM's semantic understanding of the user's framework as context
    *   **Framework-Based Assessment**: Relevance and contribution are measured through the LLM's comprehension of the user's expressed criteria
    *   **Formation Accumulation**: Insights that emerge from engagement with documents projected through the user's authored framework
    *   **Cross-Journey Bridges**: Different users' natural language frameworks may reveal shared concepts through different semantic projections
    
    The same document exists differently in each journey's projection space because each user's natural language framework creates a unique semantic lens. A paper on "neural networks" is processed completely differently when one user writes "I'm investigating algorithmic robustness in deep learning architectures" versus another who writes "I'm exploring how artificial networks might model human cognitive processes."

*   **Knowledge Layer:** This is the queryable, semantic API exposed by the Knowledge Database to the Process Engine. It provides journey-scoped access to projections, enabling search and discovery within the user's intellectual space rather than generic retrieval.

#### Why Projection Spaces Matter

1. **Scale Without Summarization**: Processing 3,000 papers becomes tractable not through AI summaries but through precise projection
2. **Progressive Refinement**: Initial broad projections minimize false negatives, then iterative refinement sharpens understanding
3. **Cross-Disciplinary Discovery**: Different projections of the same documents reveal conceptual bridges between fields
4. **Intellectual Sovereignty**: The user's framework determines meaning, not the system's processing

### V. Process Model

The Process Engine executes two distinct categories of processes through a unified interface architecture.

*   **Platform Services:** These foundational capabilities ensure intellectual sovereignty:
    *   Document ingestion that preserves organizational structure
    *   Metadata extraction that respects user categorization
    *   Embedding generation that reflects conceptual space
    *   Indexing that supports search patterns
    *   Database operations that maintain journey context
    
    These services never generate insights—they prepare materials for analysis. They are triggered by user actions and serve the inquiry.

*   **Reference Processes:** The platform includes two fully-implemented processes that directly embody the foundational research:
    *   **SystematicScreeningProcess:** Direct implementation of **[LLAssist](./papers/2407.13993v3.pdf)** methodology - dual relevance/contribution assessment with user-authored research questions and definitions
    *   **ConstrainedCompositionProcess:** Direct implementation of **[EdgePrompt](./papers/3701716.3717810.pdf)** methodology - structured content creation with teacher-authored rubrics and safety constraints
    
    These reference implementations demonstrate how **[Cognitive Silicon](./papers/2504.16622v1.pdf)** principles of formation through authorship are realized through systematic processing orchestrated by the platform services.

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

The platform handles migrations and ensures data consistency across extensions. See [11-EXTENSION-GUIDE.md](./11-EXTENSION-GUIDE.md) for implementation details.

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

See [06-USER-MODEL.md](./06-USER-MODEL.md) for detailed specifications.