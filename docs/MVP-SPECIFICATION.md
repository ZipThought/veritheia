# Veritheia MVP: Functionality Specification

This document defines the complete set of functionalities required for the Veritheia Minimum Viable Product (MVP). It focuses on delivering core value for educational institutions and research groups who need to manage and analyze document collections.

## I. Knowledge Database

The core data layer providing passive storage and semantic access.

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

The orchestrating layer that executes all logic and workflows.

### 2.1 Static Processes (Core Services)

| ID | Feature | Description |
|---|---|---|
| 2.1.1 | Ingestion Pipeline | Background process for new artifacts: PDF→text, metadata extraction, chunking, embedding generation, storage, auto-scope assignment |
| 2.1.2 | Summary Generation | On-demand summary generation for artifacts via Cognitive System |

### 2.2 Dynamic Processes (Analytical Workflows)

| ID | Feature | Description |
|---|---|---|
| 2.2.1 | Systematic Screening | Core SLR workflow: accepts research question + optional scope, calculates semantic similarity, returns ranked artifacts |

## III. Presentation (Desktop Web Client)

The primary user interface for the MVP, delivered as a self-contained desktop application.

### 3.1 Library Management

| ID | Feature | Description |
|---|---|---|
| 3.1.1 | Artifact Upload | UI for uploading PDF and .txt files |
| 3.1.2 | Library View | Browsable list/table with metadata, scope filters, and scope indicators |
| 3.1.3 | Artifact Detail View | Full metadata display with provenance info and scope management |
| 3.1.4 | Artifact Deletion | Remove artifact and all associated processed data |

### 3.2 Inquiry & Interaction

| ID | Feature | Description |
|---|---|---|
| 3.2.1 | Search Interface | Unified search bar for keyword/semantic search with scope selector |
| 3.2.2 | Screening Interface | Research question input with scope constraints |
| 3.2.3 | Results Display | Ranked results with links to artifact details |
| 3.2.4 | Document Viewer | Integrated PDF/text viewer |

### 3.3 Scope Management

| ID | Feature | Description |
|---|---|---|
| 3.3.1 | Scope Manager | Tree view UI for scope hierarchy with CRUD operations and statistics |
| 3.3.2 | Bulk Assignment | Multi-select artifacts for bulk scope assignment |
| 3.3.3 | Scope Navigation | "Enter" scope to constrain all operations |

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