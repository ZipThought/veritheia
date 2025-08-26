# Extension Development Guide

This guide explains how to extend Veritheia with new processes, data models, and user interfaces. Extensions integrate cleanly with the core platform while maintaining their own domain logic and ensuring users remain the authors of their understanding.

## Understanding the Extension Model

Veritheia's architecture supports full-stack extensions through a process-based model. Each extension typically includes:

- **Process Implementation**: Business logic that orchestrates the user's journey
- **Data Models**: Domain-specific entities that capture process state
- **User Interface**: Components that enable user interaction
- **Result Rendering**: Visualization of process outcomes

> **Formation Note:** Extensions aren't features added to Veritheia—they're new domains where formation through authorship can occur. Every extension must preserve the neurosymbolic transcendence: users author frameworks in natural language, the system mechanically applies them, formation emerges from engagement. An extension that generates insights or makes decisions violates the core architecture. Extensions amplify user capability, never replace user judgment.

## Process Categories

Extensions typically fall into one of these categories, each supporting different patterns of intellectual work:

### Methodological Processes
Guide users through established research methodologies. These processes:
- Structure inquiry according to proven frameworks
- Ensure systematic coverage of methodology requirements
- Maintain research rigor while preserving personal interpretation
- Examples: Systematic review protocols, grounded theory analysis, case study methodology

### Developmental Processes
Support skill progression through scaffolded challenges. These processes:
- Adapt to user's current capability level
- Provide structured practice opportunities
- Track progress while maintaining personal growth paths
- Examples: Writing development sequences, critical thinking exercises, research skill building

### Analytical Processes
Enable domain-specific analysis patterns. These processes:
- Apply specialized analytical frameworks
- Reveal patterns through user's interpretive lens
- Support deep examination of specific phenomena
- Examples: Statistical analysis workflows, thematic analysis, network analysis

### Compositional Processes
Facilitate creative and expressive work. These processes:
- Guide structured creation while preserving voice
- Provide constraints that enhance creativity
- Support iterative development of original work
- Examples: Academic writing support, argument construction, synthesis development

### Reflective Processes
Guide contemplative and evaluative practices. These processes:
- Encourage deep engagement with material
- Support metacognitive development
- Foster personal understanding through structured reflection
- Examples: Learning reflection protocols, research diary processes, peer review workflows

## Extension Architecture

### Core Concepts

Every extension builds on these foundational concepts:

1. **Process Definition**: Describes what the process does and what inputs it requires
2. **Process Context**: Carries user journey and execution state throughout the process
3. **Platform Services**: Guaranteed capabilities provided by the core platform
4. **Result Persistence**: Ensures all outputs are tied to their generative journey

### Integration Points

Extensions integrate with the platform through several well-defined interfaces:

#### Process Interface
The primary integration point where extensions:
- Define their metadata and input requirements
- Execute their core logic with platform service access
- Specify how results should be rendered

#### Data Model Integration
Extensions can define domain-specific entities that:
- Relate to process executions for full traceability
- Extend the core schema without modifying it
- Maintain referential integrity with platform entities

#### User Interface Components
Extensions provide UI components that:
- Collect process-specific inputs through dynamic forms
- Display results in domain-appropriate ways
- Maintain consistent user experience patterns

#### Service Registration
Extensions register their services to:
- Enable dependency injection throughout the system
- Allow discovery by the process registry
- Integrate with platform infrastructure

## Design Principles

### Maintain Personal Context
Every process execution is inherently tied to a specific user's journey. Extensions should:
- Always operate within the provided process context
- Never generate outputs that could be meaningful outside that context
- Ensure results reflect the user's specific questions and framework

### Respect Process Boundaries
Extensions should be self-contained and respectful of other processes:
- Access only data explicitly provided through context
- Not directly query other processes' results
- Use platform services for all cross-cutting concerns

### Enable Composition
Design extensions to work well with others:
- Structure output data for potential downstream use
- Document the conceptual model clearly
- Follow platform conventions for data formats

### Preserve Intellectual Sovereignty
Ensure users remain the authors of their understanding:
- Never make decisions on behalf of users
- Always require active engagement for meaningful results
- Design interactions that develop capability, not dependency

## Development Workflow

### 1. Define Process Concept
Before coding, clearly articulate:
- What intellectual work the process supports
- What category it falls into and why
- What inputs are required from users
- What outputs will be produced

### 2. Design the User Journey
Map out how users will:
- Discover and understand the process
- Provide necessary inputs
- Engage with the process execution
- Interpret and use the results

### 3. Model Domain

### 3.4 Prohibited Extension Patterns

Extensions must not implement fallback data generation. When your extension cannot access required services, fail immediately with clear exceptions. Do not generate substitute data. Do not provide degraded functionality. Do not attempt graceful degradation.

Extensions must not catch and suppress platform exceptions. When platform services throw exceptions, let them propagate. Do not log and continue. Do not transform into default returns. Do not wrap in generic errors. Platform exceptions contain formation context that users need.

Extensions must not partially process document sets. When your extension processes multiple documents, use transaction semantics. Either process all documents successfully or rollback entirely. Do not commit partial results. Do not skip failed documents. Do not continue after errors.

Extensions must not provide degraded operation modes. When required services are unavailable, fail completely. Do not switch to "offline mode." Do not use "cached approximations." Do not offer "basic functionality." Your extension either operates with full capability or reports inability to proceed.
Identify domain-specific concepts that need representation:
- What entities capture process state
- How they relate to platform entities
- What data needs persistence

### 4. Implement the Process
Create process implementation following platform patterns:
- Define process metadata and inputs
- Implement execution logic using platform services
- Design result structure for domain
- Create result rendering components

### 5. Test Extension
Verify extension:
- Works correctly in isolation
- Integrates properly with platform services
- Maintains personal context throughout execution
- Produces meaningful, journey-specific results

## Platform Services

Extensions rely on these guaranteed platform services:

### Document Processing
Handles the complexity of extracting and preparing textual content:
- Supports multiple file formats (PDF, text, etc.)
- Maintains relationship to source documents
- Provides consistent text extraction quality

### Embedding Generation
Creates vector representations for semantic operations:
- Uses configured cognitive adapter
- Maintains embedding versioning
- Supports batch operations for efficiency

### Metadata Extraction
Identifies and extracts document properties:
- Recognizes common metadata patterns
- Preserves source attribution
- Enables structured queries

### Document Chunking
Splits documents into processable segments:
- Maintains semantic coherence
- Supports different chunking strategies
- Preserves chunk-to-document relationships

### Knowledge Repository
Provides unified data access:
- Respects scope boundaries
- Maintains query efficiency
- Ensures data consistency

## Journal Integration

### Writing Meaningful Entries

Processes should record journal entries at key moments:
- Decision points with rationales
- Pattern discoveries
- Method adjustments
- Reflective insights

Write entries as narratives, not logs:
```
"After reviewing inclusion criteria against Paper Y, decided to include 
despite methodological differences. The contrasting approach provides 
valuable perspective on [topic]."
```

### Designing for Future Sharing

Structure journals to be potentially shareable:
- Self-contained narratives
- Sufficient context for external readers
- Clear decision rationales
- Tagged for categorization

### Supporting Context Windows

Design for varying context availability:
- **Minimal (4K)**: Recent critical entries only
- **Standard (32K)**: Recent narrative with key decisions  
- **Extended (100K+)**: Full journey narrative with cross-references

The platform assembles appropriate context based on available capacity.

## Result Patterns

### Analytical Results
For processes that analyze and assess:
- Present findings with clear rationales
- Enable filtering and sorting
- Show confidence or relevance scores
- Maintain connection to source materials

### Compositional Results
For processes that create content:
- Display generated content prominently
- Show constraints and how they were met
- Provide evaluation or feedback
- Enable iteration and refinement

### Developmental Results
For processes that track progress:
- Visualize growth over time
- Highlight achievements and challenges
- Suggest next steps
- Maintain historical context

### Reflective Results
For processes that deepen understanding:
- Present insights and realizations
- Show evolution of thinking
- Connect to broader patterns
- Encourage further reflection

## Best Practices

### Process Design
- Start with user needs, not technical capabilities
- Design for meaningful engagement, not efficiency
- Ensure every step requires user judgment
- Make the journey as important as the destination

### Data Modeling
- Model only what's essential for your domain
- Use platform entities for common concepts
- Design for extensibility within your domain
- Document your schema clearly

### User Interface
- Follow platform UI patterns for consistency
- Design forms that guide without constraining
- Provide clear feedback during processing
- Make results explorable and interactive

### Performance Considerations
- Use platform services efficiently
- Batch operations where possible
- Cache appropriately within process execution
- Design for incremental processing

## Distribution Considerations

### Package Structure
Organize your extension for clarity:
- Separate concerns into appropriate projects
- Include complete documentation
- Provide example usage scenarios
- Include test coverage

### Versioning
Maintain compatibility through careful versioning:
- Follow semantic versioning principles
- Document breaking changes clearly
- Maintain backward compatibility where possible
- Test against multiple platform versions

### Documentation
Help users understand your extension:
- Explain the intellectual work it supports
- Provide conceptual overview before technical details
- Include real-world usage examples
- Document prerequisites and assumptions

## Troubleshooting

### Common Integration Issues

**Process Not Discovered**
- Verify service registration is correct
- Check that process interface is properly implemented
- Ensure assembly is being scanned

**Data Access Problems**
- Confirm entities are properly related
- Check that migrations have been applied
- Verify scope permissions

**UI Components Not Rendering**
- Ensure components are registered
- Check that view models match component expectations
- Verify Blazor routing configuration

### Debugging Techniques
- Use platform logging infrastructure
- Leverage development-time diagnostics
- Test components in isolation
- Verify against reference implementations

## Future Considerations

As you develop extensions, consider:
- How your extension might compose with others
- What new patterns you're establishing
- How to maintain quality as complexity grows
- Ways to contribute patterns back to the community
- How journals could become community resources
- What collaborative patterns might emerge

### Preparing for Evolution

Design with future capabilities in mind:
- **Collaboration**: Store attribution even in single-user mode
- **Templates**: Structure journeys for potential reuse
- **Sharing**: Write journals as teachable narratives
- **Scale**: Consider larger context windows

The architecture supports these futures without requiring them.

Remember: The goal is not just to add functionality, but to expand the ways users can develop their own understanding. Every extension should make users more capable, not more dependent on the system.