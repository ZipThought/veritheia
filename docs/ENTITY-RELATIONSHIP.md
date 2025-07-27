# Entity-Relationship Model

This document defines the database schema that persists Veritheia's domain model. The schema clearly separates core platform tables (required for all deployments) from extension tables (process-specific additions).

## Database Overview

Veritheia uses PostgreSQL 16 with the pgvector extension for unified storage of relational data and vector embeddings. The schema is organized into core platform areas and extension areas.

## Naming Conventions

Classes use singular names (User, Document) while database tables use plural (users, documents). This follows standard conventions for each domain.

## Core Platform Schema

These tables are required for all Veritheia deployments and cannot be modified by extensions.

### Core Platform ERD

```mermaid
erDiagram
    %% User and Identity Tables (Core)
    users {
        uuid id PK
        varchar email UK
        varchar display_name
        timestamp last_active_at
        timestamp created_at
        timestamp updated_at
    }

    personas {
        uuid id PK
        uuid user_id FK
        varchar domain
        boolean is_active
        jsonb conceptual_vocabulary
        jsonb patterns
        jsonb methodological_preferences
        jsonb markers
        timestamp last_evolved
        timestamp created_at
        timestamp updated_at
    }

    process_capabilities {
        uuid id PK
        uuid user_id FK
        varchar process_type
        boolean is_enabled
        timestamp granted_at
        timestamp created_at
    }

    %% Journey and Journal Tables (Core)
    journeys {
        uuid id PK
        uuid user_id FK
        uuid persona_id FK
        varchar process_type
        text purpose
        varchar state
        jsonb context
        timestamp created_at
        timestamp updated_at
    }

    journals {
        uuid id PK
        uuid journey_id FK
        varchar type
        boolean is_shareable
        timestamp created_at
        timestamp updated_at
    }

    journal_entries {
        uuid id PK
        uuid journal_id FK
        text content
        varchar significance
        text[] tags
        jsonb metadata
        timestamp created_at
    }

    %% Knowledge Tables (Core)
    documents {
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

    document_metadata {
        uuid id PK
        uuid document_id FK UK
        varchar title
        text[] authors
        date publication_date
        jsonb extended_metadata
        timestamp created_at
        timestamp updated_at
    }

    processed_contents {
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

    knowledge_scopes {
        uuid id PK
        varchar name
        text description
        varchar type
        uuid parent_scope_id FK
        timestamp created_at
        timestamp updated_at
    }

    %% Process Tables (Core)
    process_definitions {
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

    process_executions {
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

    process_results {
        uuid id PK
        uuid execution_id FK UK
        varchar process_type
        jsonb data
        jsonb metadata
        timestamp executed_at
        timestamp created_at
    }

    %% Core Relationships
    users ||--o{ personas : "has"
    users ||--o{ process_capabilities : "granted"
    users ||--o{ journeys : "owns"
    personas ||--o{ journeys : "used by"

    journeys ||--o{ journals : "contains"
    journeys ||--o{ process_executions : "tracks"

    journals ||--o{ journal_entries : "records"

    documents ||--|| document_metadata : "has"
    documents ||--o{ processed_contents : "generates"
    documents }o--o| knowledge_scopes : "organized by"

    knowledge_scopes ||--o{ knowledge_scopes : "contains"

    process_executions ||--o| process_results : "produces"
```

### Core Table Definitions

#### User Domain Tables

##### users
Primary table for user accounts:
```sql
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
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
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
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

#### Journey Domain Tables

##### journeys
Represents user engagement with processes:
```sql
CREATE TABLE journeys (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
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

##### journals
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

##### journal_entries
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

#### Knowledge Domain Tables

##### documents
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

##### document_metadata
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

##### processed_contents
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

##### knowledge_scopes
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

#### Process Infrastructure Tables

##### process_definitions
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
    CONSTRAINT chk_trigger CHECK (trigger_type IN ('Manual', 'UserInitiated'))
);
```

##### process_executions
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

##### process_results
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

##### student_submissions
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

##### evaluation_results
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
- **CASCADE**: document_metadata, processed_contents → Remove all derived data
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