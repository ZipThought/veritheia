# Entity-Relationship Model

This document defines the database schema that enables Veritheia's neurosymbolic transcended architecture through user-partitioned journey projection spaces. The schema enforces intellectual sovereignty through composite primary keys and partition boundaries while supporting the mechanical orchestration of user-authored symbolic frameworks through neural semantic understanding.

## Architectural Foundation: User Partition Sovereignty

> **Formation Note:** The composite primary keys (UserId, Id) aren't just partitioning—they're sovereignty boundaries ensuring your intellectual work remains yours. When PostgreSQL rejects a Journey without a Persona, it's protecting a truth we've discovered: every inquiry requires a perspective.

Veritheia's database schema implements the core principle that **users own their intellectual work through partition boundaries enforced at the database level**. This manifests through:

1. **Composite Primary Keys**: All user-owned entities use `(UserId, Id)` as primary key, ensuring natural partitioning
2. **Journey Projection Spaces**: Documents are transformed according to each journey's user-authored framework, not processed generically
3. **Neurosymbolic Storage**: User's natural language frameworks become queryable symbolic systems
4. **Formation Accumulation**: Insights authored through engagement are preserved as intellectual development

As demonstrated in the foundational research that Veritheia embodies:
- **[LLAssist](./papers/2407.13993v3.pdf)** processed 2,576 papers through researcher-authored frameworks with identical systematic treatment
- **[EdgePrompt](./papers/3701716.3717810.pdf)** applied teacher-authored rubrics to ALL student responses with mechanical fairness
- **[Contextualized AI](./papers/2409.13524v1.pdf)** Method B processed large document sets through consistent user-defined frameworks

### Database Infrastructure Decisions

Based on dialectical investigation documented in [Phase 01 Database Journey](../development/phases/phase-01-database/JOURNEY.md):

1. **Primary Keys**: Composite `(UserId, Id)` using UUIDv7 via `Guid.CreateVersion7()` for partition enforcement and temporal ordering
2. **Vector Indexes**: HNSW indexes partitioned by user for journey-specific semantic search
3. **Data Access**: Entity Framework Core with partition-aware query extensions
4. **Journey-Specific Projections**: Same document projected differently per journey through user-authored symbolic frameworks
5. **Neurosymbolic Storage**: Natural language frameworks stored as JSONB with semantic search capabilities
6. **Formation Tracking**: User-authored insights accumulated through systematic engagement
7. **Partition Locality**: All indexes begin with `user_id` for optimal partition performance

## Database Technology: PostgreSQL as Neurosymbolic Foundation

Veritheia leverages PostgreSQL 17 with pgvector extension as the unified foundation for both relational data and vector embeddings. This architectural decision enables neurosymbolic transcendence by storing user-authored natural language frameworks alongside the systematic processing results they generate.

### Naming Conventions

Classes use singular names (User, Document) while database tables use plural (users, documents). All user-owned entities implement composite keys `(user_id, id)` to enforce partition boundaries.

## Core Platform Schema

These tables are required for all Veritheia deployments and cannot be modified by extensions.

### Neurosymbolic Journey Projection Spaces

The database schema enables neurosymbolic transcended architecture through journey projection spaces where user-authored natural language frameworks become dynamic symbolic systems. Documents don't have universal meaning—meaning emerges through projection into user-specific intellectual spaces.

**The Neurosymbolic Process:**
1. **User-Authored Symbolic Framework**: User expresses their intellectual framework in natural language (research questions, theoretical orientation, assessment criteria) stored as JSONB
2. **Neural Semantic Understanding**: LLM comprehends the user's natural language framework and applies semantic understanding to each document
3. **Mechanical Systematic Application**: Process Engine applies identical treatment to EVERY document through the user's framework without exception
4. **Journey-Specific Projection**: Same document transformed differently per journey based on user's authored symbolic system
5. **Formation Accumulation**: User develops understanding through engagement with systematically processed documents

This transcends traditional neurosymbolic approaches because:
- **Symbolic Component**: User-authored natural language frameworks (not hardcoded rules)
- **Neural Component**: Semantic understanding of user's intellectual stance (not mechanical extraction)
- **Systematic Processing**: Mechanical orchestration ensures fairness and completeness across ALL documents
- **Formation Outcome**: User authorship through engagement (not AI-generated insights)

### Core Platform ERD

**Implementation Priority: P0-Foundation**  
The core platform schema must be created first. All tables with user partition keys must exist before any journey can begin. This includes users, personas, journeys, and documents tables which form the foundational structure.

```mermaid
erDiagram
    %% User and Identity Tables (Core) - User Partition Sovereignty
    users {
        uuid id PK "Global user identity"
        varchar email UK
        varchar display_name
        timestamp last_active_at
        timestamp created_at
        timestamp updated_at
    }

    personas {
        uuid user_id PK,FK "Partition key - always first"
        uuid id PK "UUIDv7 for temporal ordering"
        varchar domain
        boolean is_active
        jsonb conceptual_vocabulary "User's symbolic vocabulary"
        jsonb patterns "Recurring intellectual structures"
        jsonb methodological_preferences "User's approaches"
        jsonb markers "Formation milestones"
        timestamp last_evolved
        timestamp created_at
        timestamp updated_at
    }

    process_capabilities {
        uuid user_id PK,FK "Partition key - user sovereignty"
        uuid id PK "UUIDv7 identifier"
        varchar process_type
        boolean is_enabled
        timestamp granted_at
        timestamp created_at
    }

    %% Journey and Journal Tables (Core) - Neurosymbolic Projection Spaces
    journeys {
        uuid user_id PK,FK "Partition key - intellectual sovereignty"
        uuid id PK "UUIDv7 journey identifier"
        uuid persona_id FK "Within same user partition"
        varchar process_type "Which neurosymbolic process"
        text purpose "User's authored intention"
        varchar state
        jsonb context "Journey-specific parameters"
        timestamp created_at
        timestamp updated_at
    }

    journey_frameworks {
        uuid user_id PK,FK "Partition key - framework ownership"
        uuid id PK "UUIDv7 framework identifier"
        uuid journey_id FK UK "Within same user partition"
        varchar journey_type "Type of formative journey"
        jsonb framework_elements "User's natural language symbolic system"
        jsonb projection_rules "How to transform documents systematically"
        timestamp created_at
        timestamp updated_at
    }

    journals {
        uuid user_id PK,FK "Partition key - narrative ownership"
        uuid id PK "UUIDv7 journal identifier"
        uuid journey_id FK "Within same user partition"
        varchar type
        boolean is_shareable
        timestamp created_at
        timestamp updated_at
    }

    journal_entries {
        uuid user_id PK,FK "Partition key - entry ownership"
        uuid id PK "UUIDv7 entry identifier"
        uuid journal_id FK "Within same user partition"
        text content "User's authored narrative"
        varchar significance
        text[] tags
        jsonb metadata "Formation markers"
        timestamp created_at
    }

    %% Knowledge Tables (Core) - Raw Corpus with User Ownership
    documents {
        uuid user_id PK,FK "Partition key - document ownership"
        uuid id PK "UUIDv7 document identifier"
        varchar file_name
        varchar mime_type
        varchar file_path
        bigint file_size
        timestamp uploaded_at
        uuid scope_id FK "Within same user partition"
        timestamp created_at
        timestamp updated_at
    }

    document_metadata {
        uuid user_id PK,FK "Partition key - metadata ownership"
        uuid id PK "UUIDv7 metadata identifier"
        uuid document_id FK UK "Within same user partition"
        varchar title
        text[] authors
        date publication_date
        jsonb extended_metadata "Extracted document properties"
        timestamp created_at
        timestamp updated_at
    }

    journey_document_segments {
        uuid user_id PK,FK "Partition key - projection ownership"
        uuid id PK "UUIDv7 segment identifier"
        uuid journey_id FK "Within same user partition"
        uuid document_id FK "Within same user partition"
        text segment_content "Content shaped by user's framework"
        varchar segment_type "Type determined by projection rules"
        text segment_purpose "Why this exists for this user's journey"
        jsonb structural_path "Position in original document"
        int sequence_index
        int4range byte_range
        varchar created_by_rule "Which user rule created this"
        varchar created_for_question "Which user question drove this"
        timestamp created_at
    }

    search_indexes {
        uuid user_id PK,FK "Partition key - search ownership"
        uuid id PK "UUIDv7 index identifier"
        uuid segment_id FK "Within same user partition"
        varchar vector_model "Which embedding model used"
        int vector_dimension "Dimension for polymorphic storage"
        timestamp indexed_at
    }

    search_vectors_1536 {
        uuid user_id PK,FK "Partition key - vector ownership"
        uuid index_id PK FK "Within same user partition"
        vector embedding "Journey-contextualized embedding"
    }

    search_vectors_768 {
        uuid user_id PK,FK "Partition key - vector ownership"
        uuid index_id PK FK "Within same user partition"
        vector embedding "Journey-contextualized embedding"
    }

    journey_segment_assessments {
        uuid user_id PK,FK "Partition key - assessment ownership"
        uuid id PK "UUIDv7 assessment identifier"
        uuid segment_id FK "Within same user partition"
        varchar assessment_type "Neural understanding type"
        int research_question_id "Which user question"
        float relevance_score "Neural semantic assessment"
        float contribution_score "Neural understanding of contribution"
        jsonb rubric_scores "For educational frameworks"
        text assessment_reasoning "LLM's understanding of user framework"
        jsonb reasoning_chain "Chain-of-thought through user's system"
        varchar assessed_by_model "Which neural system provided understanding"
        timestamp assessed_at
    }

    journey_formations {
        uuid user_id PK,FK "Partition key - formation ownership"
        uuid id PK "UUIDv7 formation identifier"
        uuid journey_id FK "Within same user partition"
        varchar insight_type "Type of user-authored insight"
        text insight_content "User's authored understanding"
        jsonb formed_from_segments "Which systematically processed segments"
        jsonb formed_through_questions "Which user questions enabled formation"
        text formation_reasoning "User's reasoning through engagement"
        text formation_marker "Milestone in intellectual development"
        timestamp formed_at
    }

    knowledge_scopes {
        uuid user_id PK,FK "Partition key - scope ownership"
        uuid id PK "UUIDv7 scope identifier"
        varchar name "User's organizational structure"
        text description
        varchar type
        uuid parent_scope_id FK "Within same user partition"
        timestamp created_at
        timestamp updated_at
    }

    %% Process Tables (Core) - Neurosymbolic Process Infrastructure
    process_definitions {
        uuid id PK "Global process definition (not user-specific)"
        varchar process_type UK "Unique process type identifier"
        varchar name "User-readable process name"
        text description "What this neurosymbolic process enables"
        varchar category "Type of formative process"
        varchar trigger_type "How process initiates"
        jsonb inputs "Expected user framework structure"
        jsonb configuration "Process-specific parameters"
        timestamp created_at
        timestamp updated_at
    }

    process_executions {
        uuid user_id PK,FK "Partition key - execution ownership"
        uuid id PK "UUIDv7 execution identifier"
        uuid journey_id FK "Within same user partition"
        varchar process_type "Which neurosymbolic process"
        varchar state "Current execution state"
        jsonb inputs "User's authored framework for this execution"
        timestamp started_at
        timestamp completed_at
        text error_message
        timestamp created_at
        timestamp updated_at
    }

    process_results {
        uuid user_id PK,FK "Partition key - result ownership"
        uuid id PK "UUIDv7 result identifier"
        uuid execution_id FK UK "Within same user partition"
        varchar process_type "Which neurosymbolic process produced this"
        jsonb data "Systematic processing results for user engagement"
        jsonb metadata "Process execution details"
        timestamp executed_at
        timestamp created_at
    }

    %% Core Relationships - User Partition Sovereignty
    users ||--o{ personas : "owns intellectual personas"
    users ||--o{ process_capabilities : "granted neurosymbolic processes"
    users ||--o{ journeys : "owns formative journeys"
    personas ||--o{ journeys : "enables projection through"

    journeys ||--o{ journals : "contains"
    journeys ||--o{ process_executions : "tracks"

    journals ||--o{ journal_entries : "records"

    documents ||--|| document_metadata : "has"
    documents ||--o{ journey_document_segments : "projected into"
    documents }o--o| knowledge_scopes : "organized by"
    
    journeys ||--|| journey_frameworks : "defines"
    journeys ||--o{ journey_document_segments : "creates"
    journeys ||--o{ journey_formations : "accumulates"
    
    journey_document_segments ||--o{ search_indexes : "indexed by"
    journey_document_segments ||--o{ journey_segment_assessments : "assessed"
    
    search_indexes ||--|| search_vectors_1536 : "stores in"
    search_indexes ||--|| search_vectors_768 : "stores in"

    knowledge_scopes ||--o{ knowledge_scopes : "contains"

    process_executions ||--o| process_results : "produces"
```

### Core Table Definitions

#### User Domain Tables

**Implementation Priority: P0-Foundation**  
User tables must exist before any other user-owned entities. They establish the partition boundaries that ensure sovereignty.

##### users
Primary table for user accounts:
```sql
CREATE EXTENSION IF NOT EXISTS vector;

CREATE TABLE users (
    id UUID PRIMARY KEY, -- UUIDv7 generated by application via Guid.CreateVersion7()
    email VARCHAR(255) UNIQUE NOT NULL,
    display_name VARCHAR(255) NOT NULL,
    last_active_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE
);

CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_last_active ON users(last_active_at);
```

##### personas
Evolving representation of user's intellectual style:
```sql
CREATE TABLE personas (
    id UUID PRIMARY KEY, -- UUIDv7 generated by application
    user_id UUID NOT NULL,
    domain VARCHAR(100) NOT NULL,
    is_active BOOLEAN DEFAULT true,
    conceptual_vocabulary JSONB DEFAULT '{}',
    patterns JSONB DEFAULT '[]',
    methodological_preferences JSONB DEFAULT '[]',
    markers JSONB DEFAULT '[]',
    last_evolved TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    CONSTRAINT fk_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT uq_user_domain UNIQUE (user_id, domain)
);

CREATE INDEX idx_personas_user ON personas(user_id);
CREATE INDEX idx_personas_active ON personas(user_id, is_active);
```

##### process_capabilities
Tracks which processes users can access:
```sql
CREATE TABLE process_capabilities (
    id UUID PRIMARY KEY, -- UUIDv7 generated by application
    user_id UUID NOT NULL,
    process_type VARCHAR(255) NOT NULL,
    is_enabled BOOLEAN DEFAULT true,
    granted_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT uq_user_process UNIQUE (user_id, process_type)
);

CREATE INDEX idx_capabilities_user ON process_capabilities(user_id);
```

#### Journey Domain Tables

**Implementation Priority: P1-Core**  
Journey tables depend on user tables but must exist before any processing can occur. They enable the projection spaces that make formation possible.

##### journeys
Represents user engagement with processes:
```sql
CREATE TABLE journeys (
    id UUID PRIMARY KEY, -- UUIDv7 generated by application
    user_id UUID NOT NULL,
    persona_id UUID NOT NULL,
    process_type VARCHAR(255) NOT NULL,
    purpose TEXT NOT NULL,
    state VARCHAR(50) NOT NULL DEFAULT 'Active',
    context JSONB DEFAULT '{}',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    CONSTRAINT fk_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_persona FOREIGN KEY (persona_id) REFERENCES personas(id),
    CONSTRAINT chk_state CHECK (state IN ('Active', 'Paused', 'Completed', 'Abandoned'))
);

CREATE INDEX idx_journeys_user ON journeys(user_id);
CREATE INDEX idx_journeys_state ON journeys(state);
CREATE INDEX idx_journeys_process ON journeys(process_type);
```

##### journey_frameworks
User-authored natural language frameworks that become symbolic systems - core of neurosymbolic transcendence:
```sql
CREATE TABLE journey_frameworks (
    id UUID PRIMARY KEY, -- UUIDv7 generated by application
    journey_id UUID NOT NULL UNIQUE,
    journey_type VARCHAR(100) NOT NULL, -- 'systematic_review', 'educational', 'research_formation'
    
    -- The intellectual framework that shapes projections
    framework_elements JSONB NOT NULL, -- {
                                       --   "research_questions": [...],
                                       --   "conceptual_vocabulary": {...},
                                       --   "assessment_criteria": {...},
                                       --   "theoretical_orientation": "..."
                                       -- }
    
    -- Rules for transforming documents in this journey's space
    projection_rules JSONB NOT NULL, -- {
                                     --   "segmentation": {"strategy": "...", "rules": [...]},
                                     --   "embedding_context": {...},
                                     --   "assessment_prompts": [...],
                                     --   "discovery_parameters": {...}
                                     -- }
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    CONSTRAINT fk_journey FOREIGN KEY (journey_id) REFERENCES journeys(id) ON DELETE CASCADE
);

CREATE INDEX idx_frameworks_journey ON journey_frameworks(journey_id);
CREATE INDEX idx_frameworks_type ON journey_frameworks(journey_type);
```

##### journals
Narrative records within journeys:
```sql
CREATE TABLE journals (
    id UUID PRIMARY KEY, -- UUIDv7 generated by application
    journey_id UUID NOT NULL,
    type VARCHAR(50) NOT NULL,
    is_shareable BOOLEAN DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    CONSTRAINT fk_journey FOREIGN KEY (journey_id) REFERENCES journeys(id) ON DELETE CASCADE,
    CONSTRAINT chk_type CHECK (type IN ('Research', 'Method', 'Decision', 'Reflection'))
);

CREATE INDEX idx_journals_journey ON journals(journey_id);
CREATE INDEX idx_journals_type ON journals(type);
```

##### journal_entries
Individual narrative entries:
```sql
CREATE TABLE journal_entries (
    id UUID PRIMARY KEY, -- UUIDv7 generated by application
    journal_id UUID NOT NULL,
    content TEXT NOT NULL,
    significance VARCHAR(50) NOT NULL DEFAULT 'Routine',
    tags TEXT[] DEFAULT '{}',
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_journal FOREIGN KEY (journal_id) REFERENCES journals(id) ON DELETE CASCADE,
    CONSTRAINT chk_significance CHECK (significance IN ('Routine', 'Notable', 'Critical', 'Milestone'))
);

CREATE INDEX idx_entries_journal ON journal_entries(journal_id);
CREATE INDEX idx_entries_significance ON journal_entries(significance);
CREATE INDEX idx_entries_tags ON journal_entries USING GIN(tags);
CREATE INDEX idx_entries_created ON journal_entries(created_at DESC);
```

#### Knowledge Domain Tables

**Implementation Priority: P1-Core**  
Document tables enable corpus storage and must exist before documents can be projected into journeys. They maintain the raw corpus that gets transformed through user frameworks.

##### documents
Source materials in the knowledge base:
```sql
CREATE TABLE documents (
    id UUID PRIMARY KEY, -- UUIDv7 generated by application
    file_name VARCHAR(500) NOT NULL,
    mime_type VARCHAR(100) NOT NULL,
    file_path VARCHAR(1000) NOT NULL,
    file_size BIGINT NOT NULL,
    uploaded_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    scope_id UUID,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    CONSTRAINT fk_scope FOREIGN KEY (scope_id) REFERENCES knowledge_scopes(id) ON DELETE SET NULL
);

CREATE INDEX idx_documents_scope ON documents(scope_id);
CREATE INDEX idx_documents_uploaded ON documents(uploaded_at DESC);
```

##### document_metadata
Extracted document properties:
```sql
CREATE TABLE document_metadata (
    id UUID PRIMARY KEY, -- UUIDv7 generated by application
    document_id UUID UNIQUE NOT NULL,
    title VARCHAR(1000),
    authors TEXT[],
    publication_date DATE,
    extended_metadata JSONB DEFAULT '{}',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    CONSTRAINT fk_document FOREIGN KEY (document_id) REFERENCES documents(id) ON DELETE CASCADE
);

CREATE INDEX idx_metadata_title ON document_metadata(title);
CREATE INDEX idx_metadata_authors ON document_metadata USING GIN(authors);
```

##### journey_document_segments
Documents projected into journey-specific segments:
```sql
CREATE TABLE journey_document_segments (
    id UUID PRIMARY KEY, -- UUIDv7 generated by application
    journey_id UUID NOT NULL,
    document_id UUID NOT NULL,
    
    -- Content shaped by journey's projection rules
    segment_content TEXT NOT NULL,
    segment_type VARCHAR(50), -- 'abstract', 'methodology', 'paragraph', etc.
    segment_purpose TEXT, -- Why this segment exists for this journey
    
    -- Structure and position
    structural_path JSONB, -- {"path": ["section-2", "subsection-3", "paragraph-5"]}
    sequence_index INTEGER NOT NULL,
    byte_range INT4RANGE, -- Original position in document
    
    -- Projection metadata
    created_by_rule VARCHAR(255), -- Which segmentation rule created this
    created_for_question VARCHAR(255), -- Which research question drove this
    
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_journey FOREIGN KEY (journey_id) REFERENCES journeys(id) ON DELETE CASCADE,
    CONSTRAINT fk_document FOREIGN KEY (document_id) REFERENCES documents(id) ON DELETE CASCADE,
    CONSTRAINT uq_journey_doc_seq UNIQUE(journey_id, document_id, sequence_index)
);

CREATE INDEX idx_segments_journey ON journey_document_segments(journey_id);
CREATE INDEX idx_segments_document ON journey_document_segments(document_id);
CREATE INDEX idx_segments_type ON journey_document_segments(segment_type);
```

##### search_indexes
Metadata for segment embeddings:
```sql
CREATE TABLE search_indexes (
    id UUID PRIMARY KEY, -- UUIDv7 generated by application
    segment_id UUID NOT NULL,
    vector_model VARCHAR(100) NOT NULL,
    vector_dimension INTEGER NOT NULL,
    indexed_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_segment FOREIGN KEY (segment_id) REFERENCES journey_document_segments(id) ON DELETE CASCADE,
    CONSTRAINT uq_segment_model UNIQUE(segment_id, vector_model)
);

CREATE INDEX idx_search_segment ON search_indexes(segment_id);
CREATE INDEX idx_search_model ON search_indexes(vector_model);
```

##### search_vectors_1536, search_vectors_768, search_vectors_384
Polymorphic vector storage by dimension:
```sql
-- 1536-dimensional vectors (OpenAI, Cohere)
CREATE TABLE search_vectors_1536 (
    index_id UUID PRIMARY KEY REFERENCES search_indexes(id) ON DELETE CASCADE,
    embedding vector(1536) NOT NULL
);

CREATE INDEX idx_vectors_1536_hnsw ON search_vectors_1536 
    USING hnsw (embedding vector_cosine_ops)
    WITH (m = 16, ef_construction = 64);

-- 768-dimensional vectors (E5, BGE)
CREATE TABLE search_vectors_768 (
    index_id UUID PRIMARY KEY REFERENCES search_indexes(id) ON DELETE CASCADE,
    embedding vector(768) NOT NULL
);

CREATE INDEX idx_vectors_768_hnsw ON search_vectors_768 
    USING hnsw (embedding vector_cosine_ops)
    WITH (m = 16, ef_construction = 64);

-- 384-dimensional vectors (lightweight models)
CREATE TABLE search_vectors_384 (
    index_id UUID PRIMARY KEY REFERENCES search_indexes(id) ON DELETE CASCADE,
    embedding vector(384) NOT NULL
);

CREATE INDEX idx_vectors_384_hnsw ON search_vectors_384 
    USING hnsw (embedding vector_cosine_ops)
    WITH (m = 16, ef_construction = 64);
```

##### journey_segment_assessments
Journey-specific assessment of segments:
```sql
CREATE TABLE journey_segment_assessments (
    id UUID PRIMARY KEY, -- UUIDv7 generated by application
    segment_id UUID NOT NULL,
    
    -- Assessment details
    assessment_type VARCHAR(50) NOT NULL, -- 'relevance', 'contribution', 'rubric_match'
    research_question_id INTEGER, -- Which RQ this assesses
    
    -- Scores based on journey type
    relevance_score FLOAT,
    contribution_score FLOAT,
    rubric_scores JSONB, -- For educational journeys
    
    -- Reasoning preservation
    assessment_reasoning TEXT,
    reasoning_chain JSONB, -- Chain-of-thought steps
    
    -- Model tracking
    assessed_by_model VARCHAR(100),
    assessed_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_segment FOREIGN KEY (segment_id) REFERENCES journey_document_segments(id) ON DELETE CASCADE
);

CREATE INDEX idx_assessments_segment ON journey_segment_assessments(segment_id);
CREATE INDEX idx_assessments_type ON journey_segment_assessments(assessment_type);
CREATE INDEX idx_assessments_scores ON journey_segment_assessments(relevance_score, contribution_score);
```

##### journey_formations
Accumulated insights from journeys:
```sql
CREATE TABLE journey_formations (
    id UUID PRIMARY KEY, -- UUIDv7 generated by application
    journey_id UUID NOT NULL,
    
    -- What was formed
    insight_type VARCHAR(50) NOT NULL, -- 'conceptual', 'methodological', 'theoretical'
    insight_content TEXT NOT NULL,
    
    -- How it was formed
    formed_from_segments JSONB, -- {"segments": [uuid1, uuid2, ...]}
    formed_through_questions JSONB, -- {"questions": ["RQ1", "RQ2", ...]}
    formation_reasoning TEXT,
    
    -- When in the journey
    formation_marker TEXT, -- Milestone or marker reached
    formed_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_journey FOREIGN KEY (journey_id) REFERENCES journeys(id) ON DELETE CASCADE
);

CREATE INDEX idx_formations_journey ON journey_formations(journey_id);
CREATE INDEX idx_formations_type ON journey_formations(insight_type);
CREATE INDEX idx_formations_formed ON journey_formations(formed_at DESC);
```

##### knowledge_scopes
Organizational boundaries for documents:
```sql
CREATE TABLE knowledge_scopes (
    id UUID PRIMARY KEY, -- UUIDv7 generated by application
    name VARCHAR(255) NOT NULL,
    description TEXT,
    type VARCHAR(50) NOT NULL,
    parent_scope_id UUID,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    CONSTRAINT fk_parent FOREIGN KEY (parent_scope_id) REFERENCES knowledge_scopes(id) ON DELETE CASCADE,
    CONSTRAINT chk_type CHECK (type IN ('Project', 'Topic', 'Subject', 'Custom'))
);

CREATE INDEX idx_scopes_parent ON knowledge_scopes(parent_scope_id);
CREATE INDEX idx_scopes_type ON knowledge_scopes(type);
```

#### Process Infrastructure Tables

**Implementation Priority: P2-MVP**  
Process tables enable the execution tracking and result storage necessary for the MVP demonstration. They depend on journey infrastructure being in place.

##### process_definitions
Metadata for available processes:
```sql
CREATE TABLE process_definitions (
    id UUID PRIMARY KEY, -- UUIDv7 generated by application
    process_type VARCHAR(255) UNIQUE NOT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    category VARCHAR(50) NOT NULL,
    trigger_type VARCHAR(50) NOT NULL,
    inputs JSONB NOT NULL,
    configuration JSONB DEFAULT '{}',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    CONSTRAINT chk_category CHECK (category IN ('Methodological', 'Developmental', 'Analytical', 'Compositional', 'Reflective')),
    CONSTRAINT chk_trigger CHECK (trigger_type IN ('Manual', 'UserInitiated'))
);
```

##### process_executions
Tracks process runs:
```sql
CREATE TABLE process_executions (
    id UUID PRIMARY KEY, -- UUIDv7 generated by application
    journey_id UUID NOT NULL,
    process_type VARCHAR(255) NOT NULL,
    state VARCHAR(50) NOT NULL DEFAULT 'Pending',
    inputs JSONB NOT NULL,
    started_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP WITH TIME ZONE,
    error_message TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    CONSTRAINT fk_journey FOREIGN KEY (journey_id) REFERENCES journeys(id) ON DELETE CASCADE,
    CONSTRAINT chk_state CHECK (state IN ('Pending', 'Running', 'Completed', 'Failed', 'Cancelled'))
);

CREATE INDEX idx_executions_journey ON process_executions(journey_id);
CREATE INDEX idx_executions_state ON process_executions(state);
CREATE INDEX idx_executions_started ON process_executions(started_at DESC);
```

##### process_results
Stores process outputs:
```sql
CREATE TABLE process_results (
    id UUID PRIMARY KEY, -- UUIDv7 generated by application
    execution_id UUID UNIQUE NOT NULL,
    process_type VARCHAR(255) NOT NULL,
    data JSONB NOT NULL,
    metadata JSONB DEFAULT '{}',
    executed_at TIMESTAMP WITH TIME ZONE NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_execution FOREIGN KEY (execution_id) REFERENCES process_executions(id) ON DELETE CASCADE
);
```

## Extension Schemas

These tables demonstrate how processes extend the platform. New processes can follow either pattern.

### Systematic Screening Extension

The Systematic Screening process stores all its data in ProcessResult.data as JSONB - no additional tables needed.

#### Storage Pattern
```sql
-- Example of screening results stored in process_results.data:
{
    "results": [
        {
            "documentId": "uuid",
            "isRelevant": true,
            "relevanceScore": 0.85,
            "relevanceRationale": "...",
            "contributesToRQ": true,
            "contributionScore": 0.92,
            "contributionRationale": "...",
            "addressedQuestions": ["RQ1", "RQ2"]
        }
    ],
    "researchQuestions": "RQ1: ..., RQ2: ...",
    "definitions": { "term": "definition" }
}
```

#### Query Examples
```sql
-- Find all relevant documents from a screening
SELECT 
    pr.data->>'documentId' as document_id,
    pr.data->>'relevanceScore' as score
FROM process_results pr
WHERE pr.process_type = 'SystematicScreening'
    AND pr.execution_id = 'specific-execution-id'
    AND (pr.data->>'isRelevant')::boolean = true;

-- Get high-contribution documents across all screenings
SELECT DISTINCT
    result->>'documentId' as document_id,
    MAX((result->>'contributionScore')::decimal) as max_score
FROM process_results pr,
    jsonb_array_elements(pr.data->'results') as result
WHERE pr.process_type = 'SystematicScreening'
    AND (result->>'contributesToRQ')::boolean = true
GROUP BY result->>'documentId'
HAVING MAX((result->>'contributionScore')::decimal) > 0.8;
```

### Guided Composition Extension

The Guided Composition process uses dedicated tables for complex educational workflows.

#### Extension ERD

```mermaid
erDiagram
    %% Guided Composition Extension Tables
    assignments {
        uuid id PK
        varchar title
        text prompt
        text source_material
        jsonb constraints
        jsonb rubric
        uuid teacher_id FK
        boolean is_active
        timestamp created_at
        timestamp updated_at
    }

    student_submissions {
        uuid id PK
        uuid assignment_id FK
        uuid student_id FK
        text response
        timestamp submitted_at
        timestamp created_at
    }

    evaluation_results {
        uuid id PK
        uuid submission_id FK UK
        decimal score
        decimal max_score
        jsonb category_scores
        text[] feedback
        boolean is_overridden
        text override_justification
        timestamp created_at
    }

    %% Extension Relationships
    assignments ||--o{ student_submissions : "receives"
    student_submissions ||--|| evaluation_results : "generates"
    users ||--o{ assignments : "creates as teacher"
    users ||--o{ student_submissions : "creates as student"
```

#### Extension Table Definitions

##### assignments
Educational assignments for Guided Composition:
```sql
CREATE TABLE assignments (
    id UUID PRIMARY KEY, -- UUIDv7 generated by application
    title VARCHAR(500) NOT NULL,
    prompt TEXT NOT NULL,
    source_material TEXT,
    constraints JSONB NOT NULL,
    rubric JSONB NOT NULL,
    teacher_id UUID NOT NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    CONSTRAINT fk_teacher FOREIGN KEY (teacher_id) REFERENCES users(id)
);

CREATE INDEX idx_assignments_teacher ON assignments(teacher_id);
CREATE INDEX idx_assignments_active ON assignments(is_active);
```

##### student_submissions
Responses to assignments:
```sql
CREATE TABLE student_submissions (
    id UUID PRIMARY KEY, -- UUIDv7 generated by application
    assignment_id UUID NOT NULL,
    student_id UUID NOT NULL,
    response TEXT NOT NULL,
    submitted_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_assignment FOREIGN KEY (assignment_id) REFERENCES assignments(id) ON DELETE CASCADE,
    CONSTRAINT fk_student FOREIGN KEY (student_id) REFERENCES users(id),
    CONSTRAINT uq_assignment_student UNIQUE (assignment_id, student_id)
);

CREATE INDEX idx_submissions_assignment ON student_submissions(assignment_id);
CREATE INDEX idx_submissions_student ON student_submissions(student_id);
```

##### evaluation_results
Grading results with override capability:
```sql
CREATE TABLE evaluation_results (
    id UUID PRIMARY KEY, -- UUIDv7 generated by application
    submission_id UUID UNIQUE NOT NULL,
    score DECIMAL(5,2) NOT NULL,
    max_score DECIMAL(5,2) NOT NULL,
    category_scores JSONB NOT NULL,
    feedback TEXT[],
    is_overridden BOOLEAN DEFAULT false,
    override_justification TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_submission FOREIGN KEY (submission_id) REFERENCES student_submissions(id) ON DELETE CASCADE
);
```

## Extension Guidelines

### When to Use ProcessResult.data (JSONB)

Choose JSONB storage when:
- Results are read-mostly after creation
- Don't need complex relational queries
- Want to avoid schema migrations
- Data is naturally document-oriented
- Results are tightly coupled to single execution

Benefits:
- No schema changes needed
- Flexible data structure
- Fast to implement
- Good for analytical results

### When to Use Dedicated Tables

Choose dedicated tables when:
- Need referential integrity (foreign keys)
- Require complex queries or joins
- Have ongoing state management
- Need efficient updates to specific fields
- Data lives beyond single execution

Benefits:
- Full relational capabilities
- Better query performance
- Data integrity guarantees
- Supports complex workflows

## Design Trade-offs

### Array Types
We use PostgreSQL arrays for:
- `tags TEXT[]` in journal_entries
- `authors TEXT[]` in document_metadata

Rationale:
- Avoids join complexity for read-heavy operations
- PostgreSQL GIN indexes provide efficient array queries
- These arrays are bounded (few tags per entry, few authors per document)
- Simplifies the domain model

Future normalization could be added if:
- Need for author deduplication across documents
- Complex tag hierarchies or synonyms
- Cross-journal tag analytics

### JSONB Usage
Extensive use of JSONB for:
- Flexible metadata storage
- Process-specific data
- Evolution without migration

Trade-offs accepted:
- Less strict schema validation
- Potential for data inconsistency
- Compensated by application-level validation

## Indexes and Performance

### Vector Search Indexes
```sql
-- HNSW indexes are created per dimension table (see search_vectors_* tables above)
-- Each index uses optimal parameters for vector similarity search
-- Parameters: m = 16 (connections per node), ef_construction = 64 (build quality)

-- Ensure proper statistics for query planning on vector tables
ALTER TABLE search_vectors_1536 SET (autovacuum_vacuum_scale_factor = 0.02);
ALTER TABLE search_vectors_768 SET (autovacuum_vacuum_scale_factor = 0.02);
ALTER TABLE search_vectors_384 SET (autovacuum_vacuum_scale_factor = 0.02);
```

### Full-Text Search
```sql
-- Full-text search on journey-specific segments
ALTER TABLE journey_document_segments ADD COLUMN content_tsv tsvector;
UPDATE journey_document_segments SET content_tsv = to_tsvector('english', segment_content);
CREATE INDEX idx_segment_fts ON journey_document_segments USING GIN(content_tsv);

-- Trigger to maintain tsvector
CREATE TRIGGER tsvector_update BEFORE INSERT OR UPDATE ON journey_document_segments
FOR EACH ROW EXECUTE FUNCTION tsvector_update_trigger(content_tsv, 'pg_catalog.english', segment_content);
```

### JSONB Indexes
```sql
-- GIN indexes for JSONB queries
CREATE INDEX idx_personas_vocabulary ON personas USING GIN(conceptual_vocabulary);
CREATE INDEX idx_contexts ON journeys USING GIN(context);
CREATE INDEX idx_results_data ON process_results USING GIN(data);
```

## Cascade Delete Strategy

The schema implements careful cascade strategies to maintain data integrity while respecting user ownership:

### User Deletion Cascades
When a user is deleted:
- **CASCADE**: personas, journeys, process_capabilities → Complete removal of user's intellectual work
- **CASCADE**: All downstream entities (journals, journal_entries, process_executions)
- **RESTRICT**: assignments (teacher_id) → Cannot delete teachers with active assignments

### Journey Deletion Cascades  
When a journey is deleted:
- **CASCADE**: journals, process_executions → Remove all journey-specific data
- **CASCADE**: journal_entries, process_results → Complete cleanup

### Document Deletion Cascades
When a document is deleted:
- **CASCADE**: document_metadata, journey_document_segments → Remove all projections
- **CASCADE**: All downstream search_indexes, search_vectors_*, assessments
- **SET NULL**: References from scopes → Documents can exist without scopes

### Scope Deletion Cascades
When a knowledge_scope is deleted:
- **CASCADE**: child scopes → Recursive deletion of scope hierarchy
- **SET NULL**: document references → Documents persist without scope

### Extension-Specific Cascades
- **CASCADE**: assignment → student_submissions → evaluation_results
- **RESTRICT**: Cannot delete users who have submitted work (student_id)

This strategy ensures:
1. User sovereignty - deleting a user removes all their data
2. Journey integrity - journey deletion is complete
3. Document persistence - documents survive scope changes
4. Educational integrity - submitted work is preserved

## Migration Strategy

### Initial Schema Creation
1. Create core tables first (in dependency order)
2. Create extension tables per process
3. Add all foreign key constraints
4. Create indexes
5. Set up triggers and functions

### Version Management
```sql
CREATE TABLE schema_migrations (
    version VARCHAR(255) PRIMARY KEY,
    applied_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
```

### Adding New Extensions
1. Create extension tables in separate migration
2. Use extension-specific schema or table prefix
3. Reference only core tables, never other extensions
4. Document storage pattern choice

## Security Considerations

### Row-Level Security
```sql
-- Example: Users can only see their own journeys
ALTER TABLE journeys ENABLE ROW LEVEL SECURITY;

CREATE POLICY journeys_owner_policy ON journeys
    FOR ALL
    TO application_role
    USING (user_id = current_setting('app.current_user_id')::UUID);
```

### Extension Isolation
- Extensions cannot modify core tables
- Extensions cannot query other extensions' tables
- All extension data must relate to core entities
- Process isolation enforced at application layer

### Audit Trails
```sql
-- Generic audit trigger function
CREATE OR REPLACE FUNCTION audit_trigger_function()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO audit_log (
        table_name,
        operation,
        user_id,
        changed_data,
        created_at
    ) VALUES (
        TG_TABLE_NAME,
        TG_OP,
        current_setting('app.current_user_id')::UUID,
        to_jsonb(NEW),
        CURRENT_TIMESTAMP
    );
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;
```

## Backup and Recovery

### Backup Strategy
- Daily full backups of entire database
- Continuous archiving of WAL files
- Point-in-time recovery capability
- Separate backup of Raw Corpus files

### Data Retention
- Core platform data: Indefinite
- Journal entries: Indefinite (core to formation)
- Process executions: 1 year minimum
- Extension data: Per process requirements
- Audit logs: 7 years

## Platform Evolution

### Core Schema Stability
Core tables have strict backward compatibility:
- New columns must be nullable or have defaults
- Existing columns cannot change type
- Relationships cannot be broken
- Indexes can be added but not removed

### Extension Flexibility
Extensions can evolve freely:
- Add/modify tables as needed
- Change storage patterns
- Migrate between JSONB and tables
- Must maintain core table references

The schema maintains data integrity while implementing the core principle that users author their own understanding through persistent, traceable intellectual journeys, with boundaries between platform and extensions.