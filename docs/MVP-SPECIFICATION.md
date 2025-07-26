# Veritheia MVP: Functionality Specification

This document defines the functionalities that ensure users author their own understanding. Every feature is designed to amplify human intellectual capability rather than replace it. The MVP delivers an environment where educational institutions and research groups develop their own insights through structured engagement with knowledge.

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
| 1.2.1 | Metadata Extraction | Automatic extraction of standard metadata (Title, Authors) from PDFs |
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
| 2.1.4 | Event-Triggered Execution | Automatic process triggering based on system events (e.g., document upload) |

### 2.2 Static Processes (Core Platform Services)

| ID | Feature | Description |
|---|---|---|
| 2.2.1 | Document Ingestion | Automatic pipeline triggered on file upload: PDF/text extraction, chunking, embedding generation, indexing |
| 2.2.2 | Text Extraction Service | Extract and clean text from PDFs and text files |
| 2.2.3 | Embedding Generation | Generate vector embeddings for text chunks using configured Cognitive System |
| 2.2.4 | Metadata Extraction | Extract title, authors, and other metadata from documents |
| 2.2.5 | Document Chunking | Split documents into semantic chunks for processing |

### 2.3 Dynamic Processes (Analytical Workflows)

| ID | Feature | Description |
|---|---|---|
| 2.3.1 | Systematic Screening | Literature review process where users define criteria, assess relevance, and determine contribution |
| 2.3.2 | Process Input Forms | Dynamic form generation based on process requirements |
| 2.3.3 | Result Rendering | Process-specific result visualization and interaction |

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

## IV. Cognitive System

The pluggable, LLM-based reasoning component.

### 4.1 Core Components

| ID | Feature | Description |
|---|---|---|
| 4.1.1 | Adaptor Interface | ICognitiveAdaptor with CreateEmbeddingsAsync and GenerateTextAsync |
| 4.2.1 | Local Mode | Default implementation using local inference (e.g., Ollama) |
| 4.2.2 | External API Mode | Optional implementation for cloud APIs (e.g., OpenAI) |

## V. Deployment & Administration

Functionalities required for the suite to be installed and managed by a user.

### 5.1 Installation

| ID | Feature | Description |
|---|---|---|
| 5.1.1 | Desktop Installer | Single package for Windows/macOS/Linux with all components |

### 5.2 Configuration

| ID | Feature | Description |
|---|---|---|
| 5.2.1 | Settings UI | Dedicated settings panel in desktop app |
| 5.2.2 | Database Config | Set file system path for Raw Corpus |
| 5.2.3 | Cognitive Config | Toggle Local/External mode, model selection, API credentials |
| 5.2.4 | Scope Config | Default assignment rules, mandatory scope option |