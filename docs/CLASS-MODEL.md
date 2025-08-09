# Class Model

This document defines the core domain classes and their relationships within Veritheia. The model clearly separates the core platform (which all deployments require) from process-specific extensions (which demonstrate extensibility patterns).

## Overview Diagram

```mermaid
classDiagram
    namespace CorePlatform {
        class BaseEntity
        class User
        class Journey
        class Document
        class ProcessExecution
    }
    
    namespace SystematicScreeningExtension {
        class ScreeningResult
    }
    
    namespace GuidedCompositionExtension {
        class Assignment
        class StudentSubmission
        class EvaluationResult
    }
    
    ProcessResult ..> ScreeningResult : stores in data
    User ..> Assignment : creates
```

## Core Platform Classes

These classes form the foundation that all processes depend on. They cannot be modified by extensions.

### Core Platform Class Diagram

```mermaid
classDiagram
    %% Base Classes
    class BaseEntity {
        <<abstract>>
        +Guid Id
        +DateTime CreatedAt
        +DateTime? UpdatedAt
    }

    %% User and Journey Domain (Core)
    class User {
        +string Email
        +string DisplayName
        +DateTime LastActiveAt
        +ICollection~Journey~ Journeys
        +ICollection~Persona~ Personas
        +ICollection~ProcessCapability~ Capabilities
    }

    class Persona {
        +Guid UserId
        +string Domain
        +bool IsActive
        +Dictionary~string,int~ ConceptualVocabulary
        +List~InquiryPattern~ Patterns
        +List~string~ MethodologicalPreferences
        +List~FormationMarker~ Markers
        +DateTime LastEvolved
    }

    class Journey {
        +Guid UserId
        +Guid PersonaId
        +string ProcessType
        +string Purpose
        +JourneyState State
        +Dictionary~string,object~ Context
        +User User
        +Persona Persona
        +ICollection~Journal~ Journals
        +ICollection~ProcessExecution~ Executions
    }

    class Journal {
        +Guid JourneyId
        +JournalType Type
        +bool IsShareable
        +Journey Journey
        +ICollection~JournalEntry~ Entries
    }

    class JournalEntry {
        +Guid JournalId
        +string Content
        +EntrySignificance Significance
        +List~string~ Tags
        +Dictionary~string,object~ Metadata
        +Journal Journal
    }

    class ProcessCapability {
        +Guid UserId
        +string ProcessType
        +bool IsEnabled
        +DateTime GrantedAt
        +User User
    }

    %% Knowledge Domain (Core)
    class Document {
        +string FileName
        +string MimeType
        +string FilePath
        +long FileSize
        +DateTime UploadedAt
        +Guid EncounteredByJourneyId
        +string EncounterContext
        +DateTime EncounteredAt
        +Journey EncounteredByJourney
        +ICollection~ProcessedContent~ ProcessedContents
        +DocumentMetadata Metadata
        +ICollection~DocumentEncounter~ Encounters
    }

    class DocumentMetadata {
        +Guid DocumentId
        +string Title
        +List~string~ Authors
        +DateTime? PublicationDate
        +Dictionary~string,object~ ExtendedMetadata
        +Document Document
    }

    class ProcessedContent {
        +Guid DocumentId
        +string Content
        +float[]? Embedding1536
        +float[]? Embedding768
        +float[]? Embedding384
        +int ChunkIndex
        +int StartPosition
        +int EndPosition
        +string ProcessingModel
        +string ProcessingVersion
        +Document Document
    }

    class DocumentEncounter {
        +Guid DocumentId
        +Guid JourneyId
        +DateTime EncounteredAt
        +string SearchQuery
        +string WhyRelevant
        +float RelevanceScore
        +Document Document
        +Journey Journey
        +JournalEntry ReflectionEntry
    }

    class KnowledgeScope {
        +string Name
        +string Description
        +ScopeType Type
        +Guid? ParentScopeId
        +KnowledgeScope ParentScope
        +ICollection~KnowledgeScope~ ChildScopes
        +ICollection~Document~ Documents
    }

    %% Process Infrastructure (Core)
    class ProcessDefinition {
        +string ProcessType
        +string Name
        +string Description
        +ProcessCategory Category
        +ProcessTriggerType TriggerType
        +InputDefinition Inputs
        +Dictionary~string,object~ Configuration
    }

    class ProcessExecution {
        +Guid JourneyId
        +string ProcessType
        +ProcessState State
        +Dictionary~string,object~ Inputs
        +DateTime StartedAt
        +DateTime? CompletedAt
        +string? ErrorMessage
        +Journey Journey
        +ProcessResult Result
    }

    class ProcessResult {
        +Guid ExecutionId
        +string ProcessType
        +object Data
        +Dictionary~string,object~ Metadata
        +DateTime ExecutedAt
        +ProcessExecution Execution
    }

    %% Core Relationships
    BaseEntity <|-- User
    BaseEntity <|-- Persona
    BaseEntity <|-- Journey
    BaseEntity <|-- Journal
    BaseEntity <|-- JournalEntry
    BaseEntity <|-- Document
    BaseEntity <|-- DocumentMetadata
    BaseEntity <|-- ProcessedContent
    BaseEntity <|-- KnowledgeScope
    BaseEntity <|-- ProcessDefinition
    BaseEntity <|-- ProcessExecution
    BaseEntity <|-- ProcessResult
    BaseEntity <|-- ProcessCapability

    User "1" --> "*" Persona : has
    User "1" --> "*" Journey : owns
    User "1" --> "*" ProcessCapability : granted
    Persona "*" --> "1" User : belongs to

    Journey "1" --> "*" Journal : contains
    Journey "1" --> "*" ProcessExecution : tracks
    Journey "*" --> "1" User : belongs to
    Journey "*" --> "1" Persona : uses

    Journal "1" --> "*" JournalEntry : records
    Journal "*" --> "1" Journey : documents

    Document "1" --> "1" DocumentMetadata : has
    Document "1" --> "*" ProcessedContent : generates
    Document "*" --> "0..1" KnowledgeScope : organized by

    KnowledgeScope "1" --> "*" KnowledgeScope : contains
    KnowledgeScope "*" --> "0..1" KnowledgeScope : child of

    ProcessExecution "1" --> "0..1" ProcessResult : produces
    ProcessExecution "*" --> "1" Journey : part of
```

### Core Enumerations

```mermaid
classDiagram
    class JourneyState {
        <<enumeration>>
        Active
        Paused
        Completed
        Abandoned
    }

    class JournalType {
        <<enumeration>>
        Research
        Method
        Decision
        Reflection
    }

    class EntrySignificance {
        <<enumeration>>
        Routine
        Notable
        Critical
        Milestone
    }

    class ProcessCategory {
        <<enumeration>>
        Methodological
        Developmental
        Analytical
        Compositional
        Reflective
    }

    class ProcessTriggerType {
        <<enumeration>>
        Manual
        Automatic
        Scheduled
    }

    class ProcessState {
        <<enumeration>>
        Pending
        Running
        Completed
        Failed
        Cancelled
    }

    class ScopeType {
        <<enumeration>>
        Project
        Topic
        Subject
        Custom
    }
```

### Core Value Objects

These are transient or stored as JSONB within core entities:

```mermaid
classDiagram
    class InputDefinition {
        <<value object>>
        +List~InputField~ Fields
        +AddTextArea()
        +AddTextInput()
        +AddDropdown()
        +AddScopeSelector()
        +AddDocumentSelector()
    }

    class ProcessContext {
        <<value object>>
        +Guid ExecutionId
        +Guid UserId
        +Guid JourneyId
        +Guid? ScopeId
        +Dictionary~string,object~ Inputs
        +JourneyContext JourneyContext
        +GetInput~T~()
        +GetService~T~()
    }

    class JourneyContext {
        <<value object>>
        +string Purpose
        +Dictionary~string,object~ State
        +List~JournalEntry~ RecentEntries
        +PersonaContext PersonaContext
    }

    class PersonaContext {
        <<value object>>
        +List~string~ RelevantVocabulary
        +List~InquiryPattern~ ActivePatterns
        +string? DomainFocus
    }

    class InquiryPattern {
        <<value object>>
        +string PatternType
        +string Description
        +int OccurrenceCount
        +DateTime LastObserved
    }

    class FormationMarker {
        <<value object>>
        +DateTime OccurredAt
        +string InsightDescription
        +Guid JourneyId
        +string Context
    }
```

## Extension Classes (Process-Specific)

These classes demonstrate how processes extend the platform. New processes follow these patterns.

### Systematic Screening Extension

This extension stores its results entirely within ProcessResult.Data:

```mermaid
classDiagram
    class ScreeningResult {
        <<extension-value-object>>
        +Guid DocumentId
        +bool IsRelevant
        +decimal RelevanceScore
        +string RelevanceRationale
        +bool ContributesToRQ
        +decimal ContributionScore
        +string ContributionRationale
        +List~string~ AddressedQuestions
    }

    class ScreeningProcessResult {
        <<stored-as-jsonb>>
        +List~ScreeningResult~ Results
        +string ResearchQuestions
        +Dictionary~string,string~ Definitions
    }

    ProcessResult ..> ScreeningProcessResult : data contains
    ScreeningProcessResult "1" --> "*" ScreeningResult : contains
```

### Guided Composition Extension

This extension uses dedicated tables for complex educational workflows:

```mermaid
classDiagram
    class Assignment {
        <<extension-entity>>
        +string Title
        +string Prompt
        +string SourceMaterial
        +Dictionary~string,object~ Constraints
        +Dictionary~string,object~ Rubric
        +Guid TeacherId
        +bool IsActive
        +ICollection~StudentSubmission~ Submissions
    }

    class StudentSubmission {
        <<extension-entity>>
        +Guid AssignmentId
        +Guid StudentId
        +string Response
        +DateTime SubmittedAt
        +EvaluationResult Evaluation
        +Assignment Assignment
    }

    class EvaluationResult {
        <<extension-entity>>
        +Guid SubmissionId
        +decimal Score
        +decimal MaxScore
        +Dictionary~string,decimal~ CategoryScores
        +List~string~ Feedback
        +bool IsOverridden
        +string? OverrideJustification
        +StudentSubmission Submission
    }

    BaseEntity <|-- Assignment
    BaseEntity <|-- StudentSubmission
    BaseEntity <|-- EvaluationResult

    Assignment "1" --> "*" StudentSubmission : receives
    StudentSubmission "1" --> "1" EvaluationResult : generates
    User ..> Assignment : creates as teacher
    User ..> StudentSubmission : creates as student
```

## Platform Boundaries

### What Core Platform Provides

- **User and Journey Management**: Identity, personas, journeys, journals
- **Knowledge Storage**: Documents, metadata, embeddings, scopes
- **Process Infrastructure**: Definitions, executions, results
- **Platform Services**: Document processing, embedding generation, context assembly

### What Extensions Provide

- **Process-Specific Logic**: How to analyze, compose, or reflect
- **Domain Entities**: Assignment, submission, evaluation, etc.
- **Result Structures**: ScreeningResult, CompositionResult, etc.
- **UI Components**: Process-specific interfaces
- **Additional Tables**: When complex queries needed

### What Extensions MUST NOT Do

- Modify core platform tables
- Bypass journey/journal system
- Access other processes' data directly
- Create users outside platform
- Store data outside ProcessResult without proper relationships

## Storage Patterns for Extensions

### Pattern 1: JSONB in ProcessResult.Data

Use when:
- Results are read-mostly
- Don't need complex relational queries
- Want to avoid schema migrations
- Data is naturally document-oriented

Example: SystematicScreeningProcess stores List<ScreeningResult> as JSONB

### Pattern 2: Dedicated Extension Tables

Use when:
- Need referential integrity (foreign keys)
- Require complex queries or joins
- Have ongoing state management
- Need efficient updates to specific fields

Example: GuidedCompositionProcess uses assignments, student_submissions, evaluation_results tables

## Value Objects vs Entities

### Stored as JSONB (Value Objects)
- InquiryPattern (in Persona.patterns)
- FormationMarker (in Persona.markers)
- InputDefinition (in ProcessDefinition.inputs)
- ScreeningResult (in ProcessResult.data)
- ProcessContext (transient, not persisted)
- JourneyContext (assembled at runtime)
- PersonaContext (assembled at runtime)

### Persisted as Tables (Entities)
- All classes inheriting from BaseEntity
- Core platform classes always have tables
- Extension entities may have tables (like Assignment)

## Repository Patterns

### Core Repositories (Always Present)
- `IUserRepository`
- `IPersonaRepository` 
- `IJourneyRepository`
- `IDocumentRepository`
- `IKnowledgeScopeRepository`
- `IProcessExecutionRepository`

### Extension Repositories (Process-Specific)
- `IAssignmentRepository` (GuidedComposition)
- Future extensions add their own

### Repository Access Patterns

```csharp
// Accessing user with all personas
var user = await userRepository.GetAsync(userId);
var personas = await personaRepository.GetByUserIdAsync(userId);

// Get specific persona by domain
var studentPersona = await personaRepository.GetByUserAndDomainAsync(userId, "Student");

// Create journey with specific persona
var journey = await journeyService.CreateJourneyAsync(new CreateJourneyRequest
{
    UserId = userId,
    PersonaId = studentPersona.Id,
    ProcessType = "SystematicScreening",
    Purpose = "Literature review for thesis"
});

// Extensions access core data through services
var documents = await knowledgeRepository.GetDocumentsInScopeAsync(scopeId);
```

## Design Principles

### Core Platform Principles
- **Aggregate Boundaries**: Each aggregate maintains internal consistency
- **Entity Identity**: All entities use GUID primary keys
- **Journey Context**: Every process execution tied to user journey
- **Intellectual Sovereignty**: Data structures ensure personal authorship

### Extension Principles
- **Process Isolation**: Extensions cannot access other processes' data
- **Platform Integration**: Must use platform services for core operations
- **Result Flexibility**: Choose appropriate storage pattern
- **User Attribution**: All data traceable to authoring user

### SOLID Compliance
- **Single Responsibility**: Each class has one reason to change
- **Open/Closed**: Platform open for extension, closed for modification
- **Liskov Substitution**: All processes are substitutable IAnalyticalProcess
- **Interface Segregation**: Interfaces focused on specific capabilities
- **Dependency Inversion**: Extensions depend on abstractions, not concretions

## Future Considerations

### Event Sourcing Preparation
- Journal entries form natural event stream
- Process executions track state transitions
- Persona evolution captures changes over time

### Multi-Tenancy Ready
- All entities include user/journey ownership
- Scopes provide logical isolation
- Extensions respect boundaries

The class model ensures that technical structure serves the core principle: users author their own understanding through structured engagement with knowledge, while enabling rich extensions for different analytical patterns.