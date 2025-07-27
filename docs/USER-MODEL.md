# User Model

This document defines how users, journeys, and journals interact within Veritheia. The model ensures that users remain the authors of their understanding while enabling both individual and collaborative formation.

## Core Concepts

### User

The User represents an individual engaging with the system:

- **Identity**: Authentication and basic profile
- **Personas**: Multiple evolving representations for different domains (Student, Researcher, Entrepreneur)
- **Knowledge Base**: Their corpus of documents (shared across all personas and journeys)
- **Capabilities**: Which processes they can access

The User is the constant—their journeys and personas may vary, but their identity and growing understanding persist.

### Journey

A Journey represents a specific instance of a user engaging with a process:

```
Journey = User + Persona + Process + Purpose + Time
```

- **Owner**: The user who initiated the journey
- **Persona**: Which domain context is active (Student, Researcher, etc.)
- **Process**: Which standardized workflow is being followed
- **Purpose**: The driving question or goal
- **State**: Current position within the process
- **Context**: Process-specific working memory

Each journey is a unique intellectual endeavor, even when using the same process multiple times.

### Persona

A Persona represents a domain-specific intellectual context:

- **Domain**: The role or context (Student, Researcher, Entrepreneur, Professional)
- **Conceptual Vocabulary**: Domain-specific terms and their usage frequency
- **Patterns**: How this persona approaches problems
- **Preferences**: Methodological tendencies in this domain
- **Active State**: Whether currently in use

Users naturally develop different vocabularies and approaches in different contexts. A student learning statistics uses different language than when they're running their startup.

### Journal

Journals are narrative records within a journey. Each journey may have multiple journals capturing different aspects:

- **Research Journal**: Findings and discoveries
- **Method Journal**: Approaches and techniques used
- **Decision Journal**: Choices made and rationales
- **Reflection Journal**: Insights and evolving understanding

Journals are written as coherent narratives, not mere logs. They capture the story of intellectual development.

## Relationships

### One User → Many Personas

A user might have:
- **Student Persona**: Academic vocabulary, learning-focused patterns
- **Researcher Persona**: Domain expertise, investigation methods
- **Entrepreneur Persona**: Business terminology, market analysis approaches

### One User → Many Journeys

A researcher might have:
- "Systematic Review of ML Security" (Researcher persona, SystematicScreeningProcess)
- "Literature Review on Privacy" (Researcher persona, SystematicScreeningProcess)
- "Research Methods Course" (Student persona, GuidedCompositionProcess)
- "Startup Market Analysis" (Entrepreneur persona, SystematicScreeningProcess)

### One Process → Many Journeys

The SystematicScreeningProcess might be used for:
- Different research topics by the same user
- Same topic by different users
- Iterative reviews as understanding deepens

### One Journey → Multiple Journals

A systematic review journey might maintain:
- Research Journal: "Found strong evidence for..."
- Method Journal: "Adjusted inclusion criteria because..."
- Decision Journal: "Excluded Paper X due to..."
- Reflection Journal: "Beginning to see pattern..."

## Persona Evolution

The Persona is not a static profile but an evolving representation:

### Components
- **Conceptual Vocabulary**: Domain terms and how the user uses them
- **Inquiry Patterns**: How they approach problems
- **Methodological Preferences**: Techniques that work for them
- **Formation Markers**: Key insights that shaped their understanding

### Evolution Process
1. Each journal entry potentially contributes to persona
2. Patterns across journeys reveal preferences
3. Vocabulary stabilizes around core concepts
4. Methods evolve and deepen

The persona helps the system understand the user's intellectual style without prescribing it.

## Context Management

### Context Assembly

When a process needs context, it assembles from:
1. Journey purpose and current state
2. Recent relevant journal entries
3. Persona elements pertinent to the task
4. Process-specific working memory

### Context Constraints

Context must fit within processing limits:
- Essential elements prioritized
- Recent entries favored
- Narrative coherence maintained
- User's voice preserved

### Context Windows

Different deployments support different context sizes:
- Focused context: Recent journal entries and immediate task
- Extended context: Broader journal history and cross-references
- Full context: Complete journey narrative and deep patterns

The system works effectively at any context size, with richer capabilities at larger sizes.

## Journey Patterns

### Individual Journey (MVP)

The default pattern where one user owns and controls the journey:
- Private journals
- Personal context
- Individual formation
- Complete ownership

### Collaborative Journey (Future)

Multiple users contributing to shared understanding:
- **Classroom**: Teacher guides, students participate
- **Research Group**: Collaborative investigation
- **Peer Learning**: Mutual exploration

Participants contribute to shared journals while maintaining individual voices.

### Journey Templates (Future)

Structured journeys that can be instantiated:
- **Curriculum**: Pre-designed learning paths
- **Methodologies**: Proven research approaches
- **Best Practices**: Successful patterns

Templates provide structure while preserving individual journey.

### Journey Observation (Future)

Supervised journeys for mentorship:
- **Advisor-Student**: Guidance without control
- **Peer Review**: Constructive observation
- **Self-Review**: Retrospective analysis

Observers can see without modifying the journey.

## Journal Sharing (Future)

While journeys remain personal, journals can become community resources:

### Shareable Journal Types
- **Method Journals**: "How I approach systematic reviews"
- **Reflection Journals**: "Lessons from my PhD journey"
- **Decision Journals**: "Why I made these choices"

### Sharing Principles
- Journals are coherent narratives
- Attribution preserved
- Context provided
- Formation supported, not shortcut

### Journal Libraries
Collections of shared journals:
- Disciplinary methods
- Learning pathways
- Research approaches
- Pedagogical patterns

## Data Model Implications

### User Entity
```
User
- Id
- Identity (authentication)
- Personas (collection of domain contexts)
- Capabilities (process access)
```

### Journey Entity
```
Journey
- Id
- UserId (owner)
- PersonaId (active context)
- ProcessType
- Purpose
- CreatedAt
- State
- Context (process-specific)
```

### Persona Entity
```
Persona
- Id
- UserId
- Domain (Student, Researcher, etc.)
- IsActive
- ConceptualVocabulary
- Patterns
- LastEvolved
```

### Journal Entity
```
Journal
- Id
- JourneyId
- Type (Research|Method|Decision|Reflection)
- Visibility (Private|Shareable)
- Entries (narrative records)
```

### JournalEntry Entity
```
JournalEntry
- Id
- JournalId
- Timestamp
- Content (narrative text)
- Tags (for retrieval)
- Significance (for context assembly)
```

## Process Integration

Processes interact with the user model by:

1. **Reading Context**: Assembling relevant information from journals and persona
2. **Writing Journals**: Recording significant moments and decisions
3. **Updating State**: Progressing the journey
4. **Contributing to Persona**: Patterns that might inform future journeys

Each process decides what to journal and when, maintaining the narrative flow.

## Privacy and Ownership

### MVP Principles
- Users own their journeys completely
- Journals are private by default
- Persona is never shared
- Knowledge base respects document permissions

### Future Sharing
- Explicit user consent required
- Granular control (which journals, what parts)
- Attribution maintained
- Right to withdraw

## Extension Considerations

Extensions should:
- Respect journey boundaries
- Write meaningful journal entries
- Consider future sharing in journal structure
- Contribute to persona evolution appropriately
- Design for both individual and collaborative use

The user model provides the foundation for intellectual sovereignty while enabling community formation.