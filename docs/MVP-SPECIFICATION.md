# Veritheia MVP: Functionality Specification

This document defines the functionalities that ensure users author their own understanding. Part A details the MVP features for immediate implementation. Part B outlines the post-MVP roadmap to guide architectural decisions.


---

# Part A: MVP Features (Detailed Specifications)

Every feature in the MVP is designed to amplify human intellectual capability rather than replace it. The MVP delivers an environment where educational institutions and research groups develop their own insights through structured engagement with knowledge.

## I. Knowledge Database

The foundation that preserves intellectual materials without imposing system interpretation.

### 1.1 Ingestion & Storage

| ID | Feature | Description |
|---|---|---|
| 1.1.1 | Source Artifact Support | Ingest and store PDF and plain text (.txt) files |
| 1.1.2 | Artifact Storage | Manage user-defined local file system directory as immutable Raw Corpus |
| 1.1.3 | Database Schema | PostgreSQL schema for all processed data and metadata |

### 1.2 Processed Representation

| ID | Feature | Description |
|---|---|---|
| 1.2.1 | Metadata Extraction | User-initiated extraction of standard metadata (Title, Authors) from PDFs |
| 1.2.2 | Full-Text Indexing | Generate and store full-text search index for all content |
| 1.2.3 | Vector Embedding Storage | Store embeddings using PostgreSQL pgvector extension |
| 1.2.4 | Data Provenance | Version all processed data with model/version metadata |

### 1.3 Knowledge Layer API

| ID | Feature | Description |
|---|---|---|
| 1.3.1 | Artifact & Metadata API | CRUD endpoints for artifacts and metadata |
| 1.3.2 | Full-Text Search API | Keyword-based search endpoint |
| 1.3.3 | Semantic Search API | Vector similarity search endpoint (cosine distance) |
| 1.3.4 | Scoped Query API | Optional scope parameter for all search/retrieval endpoints |

### 1.4 Knowledge Scoping

| ID | Feature | Description |
|---|---|---|
| 1.4.1 | Scope Management API | Create, update, delete, and list knowledge scopes |
| 1.4.2 | Scope Types | Project, Topic, Subject, and Custom scope types |
| 1.4.3 | Scope Hierarchy | Nested scopes with parent-child relationships |
| 1.4.4 | Artifact-Scope Association | Assign artifacts to scopes with bulk operation support |

## II. Process Engine

The orchestrating layer that executes all logic and workflows through a unified process architecture.

### 2.1 Process Architecture

| ID | Feature | Description |
|---|---|---|
| 2.1.1 | Process Interface | Common interface for all processes enabling uniform execution and monitoring |
| 2.1.2 | Process Registry | Service for process discovery and metadata retrieval |
| 2.1.3 | Process Execution | Runtime engine that executes processes with consistent error handling and result management |
| 2.1.4 | User-Triggered Execution | Process execution initiated by explicit user action |

### 2.2 Platform Services

| ID | Feature | Description |
|---|---|---|
| 2.2.1 | Document Ingestion | User-initiated pipeline for file processing: PDF/text extraction, chunking, embedding generation, indexing |
| 2.2.2 | Text Extraction Service | Extract and clean text from PDFs and text files |
| 2.2.3 | Embedding Generation | Generate vector embeddings for text chunks using configured Cognitive System |
| 2.2.4 | Metadata Extraction | Extract title, authors, and other metadata from documents |
| 2.2.5 | Document Chunking | Split documents into semantic chunks for processing |

### 2.3 Reference Processes

#### 2.3.1 Systematic Screening Process

| ID | Feature | Description |
|---|---|---|
| 2.3.1.1 | Author's Research Questions | User composes their research questions, which become part of their journey's conceptual framework |
| 2.3.1.2 | Personal Definitions | User defines key terms from their perspective, shaping how the system interprets documents |
| 2.3.1.3 | AI Relevance Assessment | AI acts as librarian measuring binary (T/F) + score (0-1) on relevance to user's RQs, with rationale |
| 2.3.1.4 | AI Contribution Assessment | AI acts as peer reviewer measuring if document directly answers user's RQs (higher bar than relevance) |
| 2.3.1.5 | Dual Rationale Presentation | AI provides distinct rationales: librarian perspective for relevance, peer reviewer for contribution |
| 2.3.1.6 | Interactive Results Table | Sortable/filterable table showing title, authors, relevance score/rationale, contribution score/rationale |
| 2.3.1.7 | User-Driven Corpus Triage | User interprets AI assessments to identify: core papers (high contribution), contextual papers (relevant only), papers to set aside |

#### 2.3.2 Constrained Composition Process

| ID | Feature | Description |
|---|---|---|
| 2.3.2.1 | Source Material Selection | Choose from uploaded corpus materials and specify sections/chapters |
| 2.3.2.2 | Task Type Selection | Predefined pedagogical task types (e.g., descriptive writing, analysis, reflection) |
| 2.3.2.3 | Learning Objective Input | Clear statement of what students should demonstrate |
| 2.3.2.4 | Content Constraints | Boundaries for generated prompts (topic limits, tone, age-appropriateness) |
| 2.3.2.5 | Answer Constraints | Rules for valid responses (word count, required elements, vocabulary level) |
| 2.3.2.6 | Prompt Generation | AI-generated writing prompt based on source material and constraints |
| 2.3.2.7 | Rubric Generation | Point-based grading rubric aligned with learning objectives |
| 2.3.2.8 | Assignment Management | Save, edit, and distribute assignments to students |
| 2.3.2.9 | Student Submission | Text input interface for student responses |
| 2.3.2.10 | AI Formative Assessment | Real-time evaluation against teacher-approved rubric, measuring performance against criteria |
| 2.3.2.11 | Teacher Sovereignty | Teachers review all AI assessments, can override grades, and maintain complete pedagogical control |
| 2.3.2.12 | Formation Analytics | Dashboard reveals patterns in student understanding, informing teacher's next instructional moves |

### 2.4 Process Categories

Additional processes can extend these patterns:

| Category | Description | Pattern |
|---|---|---|
| Methodological Processes | Structure inquiry through established methodologies | Analytical |
| Developmental Processes | Present progressive challenges for skill development | Compositional |
| Analytical Processes | Structure systematic examination for pattern discovery | Analytical |
| Compositional Processes | Structure creation exercises for expressive development | Compositional |
| Reflective Processes | Structure contemplation exercises for deeper understanding | Mixed |

See [EXTENSION-GUIDE.md](./EXTENSION-GUIDE.md) for detailed implementation patterns.

## III. Presentation (Desktop Web Client)

The primary user interface for the MVP, delivered as a self-contained desktop application.

### 3.1 Library Management

| ID | Feature | Description |
|---|---|---|
| 3.1.1 | Artifact Upload | UI for uploading PDF and .txt files |
| 3.1.2 | Library View | Browsable list/table with metadata, scope filters, and scope indicators |
| 3.1.3 | Artifact Detail View | Full metadata display with provenance info and scope management |
| 3.1.4 | Artifact Deletion | Remove artifact and all associated processed data |

### 3.2 Process Execution Interface

| ID | Feature | Description |
|---|---|---|
| 3.2.1 | Process Selection | List available analytical processes with descriptions |
| 3.2.2 | Dynamic Input Forms | Render process-specific input forms based on process definition |
| 3.2.3 | Execution Monitoring | Display process progress and status during execution |
| 3.2.4 | Result Display | Process-specific result rendering (tables, visualizations, reports) |

### 3.3 Search & Discovery

| ID | Feature | Description |
|---|---|---|
| 3.3.1 | Search Interface | Unified search bar for keyword/semantic search with scope selector |
| 3.3.2 | Document Viewer | Integrated PDF/text viewer for artifact content |
| 3.3.3 | Artifact Navigation | Browse between search results and artifact details |

### 3.4 Scope Management

| ID | Feature | Description |
|---|---|---|
| 3.4.1 | Scope Manager | Tree view UI for scope hierarchy with CRUD operations and statistics |
| 3.4.2 | Bulk Assignment | Multi-select artifacts for bulk scope assignment |
| 3.4.3 | Scope Navigation | "Enter" scope to constrain all operations |

### 3.5 User Contexts

| ID | Feature | Description |
|---|---|---|
| 3.5.1 | Process-Based Context | User interface adapts based on active process (researcher view vs educator view) |
| 3.5.2 | Assignment Access | Students see only assigned tasks, educators see creation and review interfaces |
| 3.5.3 | Result Visibility | Process determines what results users can access (own work vs class overview) |

## IV. User & Journey Model

The system for managing users and their intellectual journeys.

### 4.1 User Management

| ID | Feature | Description |
|---|---|---|
| 4.1.1 | User Registration | Basic user account creation with identity verification |
| 4.1.2 | Authentication | Secure login with session management |
| 4.1.3 | User Profile | Minimal profile for identification within journeys |
| 4.1.4 | Process Access | Configure which processes users can access |

### 4.2 Journey Management

| ID | Feature | Description |
|---|---|---|
| 4.2.1 | Journey Creation | Initiate new journey with selected process |
| 4.2.2 | Journey State | Track progress within process workflow |
| 4.2.3 | Journey Context | Maintain process-specific working memory |
| 4.2.4 | Journey List | View and resume active journeys |

### 4.3 Journal System

| ID | Feature | Description |
|---|---|---|
| 4.3.1 | Journal Creation | User-initiated creation of journey-specific journals |
| 4.3.2 | Journal Types | Research, Method, Decision, and Reflection journals |
| 4.3.3 | Entry Recording | Structured narrative entries at key process points |
| 4.3.4 | Context Assembly | Extract relevant entries for process context |

### 4.4 Persona Development

| ID | Feature | Description |
|---|---|---|
| 4.4.1 | Vocabulary Tracking | Build user's conceptual vocabulary from journal entries |
| 4.4.2 | Pattern Recognition | Identify user's inquiry patterns across journeys |
| 4.4.3 | Context Personalization | Adapt process interactions to user's style |

## V. Cognitive System

The pluggable, LLM-based reasoning component.

### 5.1 Core Components

| ID | Feature | Description |
|---|---|---|
| 5.1.1 | Adaptor Interface | ICognitiveAdapter with CreateEmbeddingsAsync and GenerateTextAsync |
| 5.1.2 | Local Mode | Default implementation using local inference (e.g., Ollama) |
| 5.1.3 | External API Mode | Optional implementation for cloud APIs (e.g., OpenAI) |

### 5.2 Context Management

| ID | Feature | Description |
|---|---|---|
| 5.2.1 | Context Window Handling | Manage content within available context size |
| 5.2.2 | Context Priority | Essential elements prioritized for smaller windows |
| 5.2.3 | Narrative Coherence | Maintain story flow in compressed contexts |

## VI. Deployment & Administration

Functionalities required for the suite to be installed and managed by a user.

### 6.1 Installation

| ID | Feature | Description |
|---|---|---|
| 6.1.1 | Desktop Installer | Single package for Windows/macOS/Linux with all components |

### 6.2 Configuration

| ID | Feature | Description |
|---|---|---|
| 6.2.1 | Settings UI | Dedicated settings panel in desktop app |
| 6.2.2 | Database Config | Set file system path for Raw Corpus |
| 6.2.3 | Cognitive Config | Toggle Local/External mode, model selection, API credentials |
| 6.2.4 | Scope Config | Default assignment rules, mandatory scope option |

---

# Part B: Post-MVP Roadmap (Conceptual Overview)

These features guide architectural decisions but are not part of the initial release.

## I. Collaborative Journeys

Allow multiple users to participate in shared intellectual endeavors.

### Classroom Journeys
- Teacher-initiated journeys with student participants
- Shared journals with individual contributions
- Collective knowledge building
- Real-time collaboration features

### Research Group Journeys
- Principal investigator with research team
- Distributed contribution tracking
- Synthesis across perspectives
- Milestone coordination

## II. Journey Templates

Pre-structured journeys for common use cases.

### Curriculum Templates
- Standard course structures
- Progressive skill development paths
- Assessment frameworks
- Reusable pedagogical patterns

### Methodology Templates
- Established research protocols
- Best practice workflows
- Quality assurance patterns
- Disciplinary standards

## III. Journal Sharing

Transform private journals into community resources.

### Shareable Journal Types
- Method Journals for technique sharing
- Reflection Journals for wisdom transfer
- Decision Journals for rationale transparency
- Pattern Journals for discovered insights

### Journal Libraries
- Institutional repositories
- Disciplinary collections
- Peer-reviewed journals
- Community contributions

## IV. Advanced Analytics

Deeper insights through extended context and pattern recognition.

### Cross-Journey Analysis
- Patterns across multiple journeys
- Long-term formation tracking
- Institutional learning analytics
- Research trend identification

### Formation Metrics
- Intellectual development indicators
- Capability progression
- Conceptual depth measures
- Engagement quality analysis

## V. Institutional Features

Enterprise capabilities for educational institutions.

### Multi-Tenant Architecture
- Department isolation
- Resource sharing controls
- Centralized administration
- Usage analytics

### Compliance & Governance
- Data retention policies
- Privacy controls
- Audit trails
- Export capabilities

## VI. Advanced Process Types

Sophisticated analytical capabilities.

### Meta-Analytical Processes
- Cross-study synthesis
- Pattern meta-analysis
- Theoretical integration
- Knowledge gap identification

### Longitudinal Processes
- Time-series analysis
- Development tracking
- Historical comparison
- Trend projection

## VII. Integration Ecosystem

Connect with external systems and workflows.

### Learning Management Systems
- Grade passthrough
- Assignment integration
- Roster synchronization
- Progress reporting

### Research Infrastructure
- Citation managers
- Data repositories
- Publication systems
- Collaboration platforms

## VIII. Mobile & Cloud

Extend beyond desktop deployment.

### Mobile Companions
- Journey review
- Quick captures
- Notification handling
- Offline sync

### Cloud Deployment
- Institutional hosting
- Elastic scaling
- Global accessibility
- Enhanced context windows

The architecture ensures these future capabilities can be added without fundamental restructuring.