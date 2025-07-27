# Entity-Relationship Model

This document defines the database schema that persists Veritheia's domain model. The schema ensures data integrity while supporting the principle that users author their own understanding.

## Database Overview

Veritheia uses PostgreSQL 16 with the pgvector extension for unified storage of relational data and vector embeddings. The schema is organized into logical areas that reflect the domain aggregates.

## Entity-Relationship Diagram

```mermaid
erDiagram
    %% User and Identity Tables
    Users {
        uuid id PK
        varchar email UK
        varchar display_name
        uuid persona_id FK
        timestamp last_active_at
        timestamp created_at
        timestamp updated_at
    }

    Personas {
        uuid id PK
        uuid user_id FK UK
        jsonb conceptual_vocabulary
        jsonb patterns
        jsonb methodological_preferences
        jsonb markers
        timestamp last_evolved
        timestamp created_at
        timestamp updated_at
    }

    ProcessCapabilities {
        uuid id PK
        uuid user_id FK
        varchar process_type
        boolean is_enabled
        timestamp granted_at
        timestamp created_at
    }

    %% Journey and Journal Tables
    Journeys {
        uuid id PK
        uuid user_id FK
        varchar process_type
        text purpose
        varchar state
        jsonb context
        timestamp created_at
        timestamp updated_at
    }

    Journals {
        uuid id PK
        uuid journey_id FK
        varchar type
        boolean is_shareable
        timestamp created_at
        timestamp updated_at
    }

    JournalEntries {
        uuid id PK
        uuid journal_id FK
        text content
        varchar significance
        text[] tags
        jsonb metadata
        timestamp created_at
    }

    %% Knowledge Tables
    Documents {
        uuid id PK
        varchar file_name
        varchar mime_type
        varchar file_path
        bigint file_size
        timestamp uploaded_at
        uuid scope_id FK
        timestamp created_at
        timestamp updated_at
    }

    DocumentMetadata {
        uuid id PK
        uuid document_id FK UK
        varchar title
        text[] authors
        date publication_date
        jsonb extended_metadata
        timestamp created_at
        timestamp updated_at
    }

    ProcessedContents {
        uuid id PK
        uuid document_id FK
        text content
        vector embedding
        int chunk_index
        int start_position
        int end_position
        varchar processing_model
        varchar processing_version
        timestamp created_at
    }

    KnowledgeScopes {
        uuid id PK
        varchar name
        text description
        varchar type
        uuid parent_scope_id FK
        timestamp created_at
        timestamp updated_at
    }

    %% Process Tables
    ProcessDefinitions {
        uuid id PK
        varchar process_type UK
        varchar name
        text description
        varchar category
        varchar trigger_type
        jsonb inputs
        jsonb configuration
        timestamp created_at
        timestamp updated_at
    }

    ProcessExecutions {
        uuid id PK
        uuid journey_id FK
        varchar process_type
        varchar state
        jsonb inputs
        timestamp started_at
        timestamp completed_at
        text error_message
        timestamp created_at
        timestamp updated_at
    }

    ProcessResults {
        uuid id PK
        uuid execution_id FK UK
        varchar process_type
        jsonb data
        jsonb metadata
        timestamp executed_at
        timestamp created_at
    }

    %% Process-Specific Tables
    Assignments {
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

    StudentSubmissions {
        uuid id PK
        uuid assignment_id FK
        uuid student_id FK
        text response
        timestamp submitted_at
        timestamp created_at
    }

    EvaluationResults {
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

    %% Relationships
    Users ||--|| Personas : "has"
    Users ||--o{ ProcessCapabilities : "granted"
    Users ||--o{ Journeys : "owns"
    Users ||--o{ Assignments : "creates"

    Journeys ||--o{ Journals : "contains"
    Journeys ||--o{ ProcessExecutions : "tracks"

    Journals ||--o{ JournalEntries : "records"

    Documents ||--|| DocumentMetadata : "has"
    Documents ||--o{ ProcessedContents : "generates"
    Documents }o--o| KnowledgeScopes : "organized by"

    KnowledgeScopes ||--o{ KnowledgeScopes : "contains"

    ProcessExecutions ||--o| ProcessResults : "produces"

    Assignments ||--o{ StudentSubmissions : "receives"
    StudentSubmissions ||--|| EvaluationResults : "generates"
```

## Table Definitions

### User Domain Tables

#### Users
Primary table for user accounts:
```sql
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) UNIQUE NOT NULL,
    display_name VARCHAR(255) NOT NULL,
    persona_id UUID,
    last_active_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    CONSTRAINT fk_persona FOREIGN KEY (persona_id) REFERENCES personas(id)
);

CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_last_active ON users(last_active_at);
```

#### Personas
Evolving representation of user's intellectual style:
```sql
CREATE TABLE personas (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID UNIQUE NOT NULL,
    conceptual_vocabulary JSONB DEFAULT '{}',
    patterns JSONB DEFAULT '[]',
    methodological_preferences JSONB DEFAULT '[]',
    markers JSONB DEFAULT '[]',
    last_evolved TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    CONSTRAINT fk_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);
```

#### ProcessCapabilities
Tracks which processes users can access:
```sql
CREATE TABLE process_capabilities (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
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

### Journey Domain Tables

#### Journeys
Represents user engagement with processes:
```sql
CREATE TABLE journeys (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    process_type VARCHAR(255) NOT NULL,
    purpose TEXT NOT NULL,
    state VARCHAR(50) NOT NULL DEFAULT 'Active',
    context JSONB DEFAULT '{}',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    CONSTRAINT fk_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT chk_state CHECK (state IN ('Active', 'Paused', 'Completed', 'Abandoned'))
);

CREATE INDEX idx_journeys_user ON journeys(user_id);
CREATE INDEX idx_journeys_state ON journeys(state);
CREATE INDEX idx_journeys_process ON journeys(process_type);
```

#### Journals
Narrative records within journeys:
```sql
CREATE TABLE journals (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
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

#### JournalEntries
Individual narrative entries:
```sql
CREATE TABLE journal_entries (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
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

### Knowledge Domain Tables

#### Documents
Source materials in the knowledge base:
```sql
CREATE TABLE documents (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
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

#### DocumentMetadata
Extracted document properties:
```sql
CREATE TABLE document_metadata (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
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

#### ProcessedContents
Chunked and embedded document content:
```sql
CREATE TABLE processed_contents (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    document_id UUID NOT NULL,
    content TEXT NOT NULL,
    embedding vector(1536),
    chunk_index INTEGER NOT NULL,
    start_position INTEGER NOT NULL,
    end_position INTEGER NOT NULL,
    processing_model VARCHAR(255) NOT NULL,
    processing_version VARCHAR(50) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_document FOREIGN KEY (document_id) REFERENCES documents(id) ON DELETE CASCADE
);

CREATE INDEX idx_processed_document ON processed_contents(document_id);
CREATE INDEX idx_processed_chunk ON processed_contents(document_id, chunk_index);
CREATE INDEX idx_embeddings ON processed_contents USING ivfflat (embedding vector_cosine_ops);
```

#### KnowledgeScopes
Organizational boundaries for documents:
```sql
CREATE TABLE knowledge_scopes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
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

### Process Domain Tables

#### ProcessDefinitions
Metadata for available processes:
```sql
CREATE TABLE process_definitions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
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
    CONSTRAINT chk_trigger CHECK (trigger_type IN ('Manual', 'Automatic', 'Scheduled'))
);
```

#### ProcessExecutions
Tracks process runs:
```sql
CREATE TABLE process_executions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
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

#### ProcessResults
Stores process outputs:
```sql
CREATE TABLE process_results (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    execution_id UUID UNIQUE NOT NULL,
    process_type VARCHAR(255) NOT NULL,
    data JSONB NOT NULL,
    metadata JSONB DEFAULT '{}',
    executed_at TIMESTAMP WITH TIME ZONE NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_execution FOREIGN KEY (execution_id) REFERENCES process_executions(id) ON DELETE CASCADE
);
```

### Process-Specific Tables

#### Assignments
Educational assignments for Guided Composition:
```sql
CREATE TABLE assignments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
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

#### StudentSubmissions
Responses to assignments:
```sql
CREATE TABLE student_submissions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
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

#### EvaluationResults
Grading results with override capability:
```sql
CREATE TABLE evaluation_results (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
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

## Indexes and Performance

### Vector Search Indexes
```sql
-- IVFFlat index for approximate nearest neighbor search
CREATE INDEX idx_embeddings ON processed_contents USING ivfflat (embedding vector_cosine_ops)
WITH (lists = 100);

-- Ensure proper statistics for query planning
ALTER TABLE processed_contents SET (autovacuum_vacuum_scale_factor = 0.02);
```

### Full-Text Search
```sql
-- Full-text search on document content
ALTER TABLE processed_contents ADD COLUMN content_tsv tsvector;
UPDATE processed_contents SET content_tsv = to_tsvector('english', content);
CREATE INDEX idx_content_fts ON processed_contents USING GIN(content_tsv);

-- Trigger to maintain tsvector
CREATE TRIGGER tsvector_update BEFORE INSERT OR UPDATE ON processed_contents
FOR EACH ROW EXECUTE FUNCTION tsvector_update_trigger(content_tsv, 'pg_catalog.english', content);
```

### JSONB Indexes
```sql
-- GIN indexes for JSONB queries
CREATE INDEX idx_personas_vocabulary ON personas USING GIN(conceptual_vocabulary);
CREATE INDEX idx_contexts ON journeys USING GIN(context);
CREATE INDEX idx_results_data ON process_results USING GIN(data);
```

## Migration Strategy

### Initial Schema Creation
1. Create tables in dependency order
2. Add foreign key constraints
3. Create indexes
4. Set up triggers and functions

### Version Management
```sql
CREATE TABLE schema_migrations (
    version VARCHAR(255) PRIMARY KEY,
    applied_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
```

### Data Migration Patterns
- Use transactions for atomic changes
- Create new columns as nullable first
- Backfill data in batches
- Add constraints after data migration
- Drop old columns in separate migration

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
- Journal entries: Indefinite (core to formation)
- Process executions: 1 year minimum
- Processed content: While document exists
- Audit logs: 7 years

The schema ensures data integrity while supporting the core principle that users author their own understanding through persistent, traceable intellectual journeys.