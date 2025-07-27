# Veritheia Documentation

## Overview

This directory contains the complete documentation for Veritheia—an environment where you author your own understanding rather than consume processed information.

Start with the [VISION](./VISION.md) to understand how Veritheia ensures intellectual sovereignty.


## Documentation Index

### Core Documents (Read in Order)

1. **[VISION.md](./VISION.md)** - Understanding through authorship
   - How Veritheia ensures users remain the authors
   - Why every output bears the author's intellectual fingerprint
   - The architecture of intellectual sovereignty

2. **[ARCHITECTURE.md](./ARCHITECTURE.md)** - System design supporting authorship
   - Components that amplify rather than replace thinking
   - Patterns that ensure personal understanding
   - Models that make insights non-transferable
   
3. **[MVP-SPECIFICATION.md](./MVP-SPECIFICATION.md)** - Features for formation
   - Capabilities that develop understanding
   - Interfaces that support authorship
   - Processes that ensure intellectual ownership

4. **[IMPLEMENTATION.md](./IMPLEMENTATION.md)** - Technical foundation for sovereignty
   - Technologies that preserve the journey
   - Workflows that maintain context
   - Systems that protect formation

5. **[DESIGN-PATTERNS.md](./DESIGN-PATTERNS.md)** - Imperative implementation patterns
   - Domain-Driven Design patterns
   - Repository and Specification patterns
   - CQRS and Unit of Work
   - Code examples and checklists

6. **[EXTENSION-GUIDE.md](./EXTENSION-GUIDE.md)** - Creating full-stack extensions
   - Process implementation patterns
   - Domain model integration
   - UI component development
   - Distribution and testing

7. **[API-CONTRACTS.md](./API-CONTRACTS.md)** - Interface definitions and contracts
   - Core process interfaces
   - Platform service contracts
   - Data transfer objects
   - HTTP API specifications

8. **[USER-MODEL.md](./USER-MODEL.md)** - User, journey, and journal architecture
   - User as the constant with persona and knowledge base
   - Journeys as process instances
   - Journals as narrative records
   - Future sharing patterns

9. **[CLASS-MODEL.md](./CLASS-MODEL.md)** - Core domain classes and their relationships
   - Comprehensive Mermaid class diagram
   - Aggregate boundaries and design principles
   - Process-specific domain models
   - Repository and service patterns

10. **[ENTITY-RELATIONSHIP.md](./ENTITY-RELATIONSHIP.md)** - Database schema and data model
    - PostgreSQL schema with pgvector extension
    - Comprehensive ERD using Mermaid
    - Table definitions with indexes
    - Migration strategy and security patterns

11. **[TESTING-STRATEGY.md](./TESTING-STRATEGY.md)** - Test types, coverage expectations, and behavioral specs
    - Testing philosophy aligned with formation principles
    - Unit, integration, and behavioral test patterns
    - Context assembly and journey integrity tests
    - Extension testing guidelines

12. **[PROMPT-ENGINEERING.md](./PROMPT-ENGINEERING.md)** - Prompt patterns that maintain formation boundaries
    - Role constraints for AI assistance
    - Context assembly from journey and journals
    - Validation patterns preventing insight generation
    - Anti-patterns and required patterns

### Planned Documents
- **DEPLOYMENT-GUIDE.md** - Production deployment procedures
- **SECURITY-POLICIES.md** - Security boundaries and threat model

## Documentation Standards

### Document Structure

Each document should include:

1. **Title** - Clear, descriptive title as H1
2. **Purpose Statement** - Brief paragraph explaining the document's intent
3. **Table of Contents** - For documents over 3 sections

### Formatting Conventions

- **Headings**: Use hierarchical markdown headings (H1 for title, H2 for major sections, H3 for subsections)
- **Code Examples**: Use fenced code blocks with language hints
- **Diagrams**: ASCII diagrams for architecture, Mermaid for complex flows
- **Lists**: Use numbered lists for sequences, bullet points for unordered items

### Cross-References

- Link between documents using relative paths: `[See Architecture](./ARCHITECTURE.md#section-name)`
- Maintain link integrity when renaming sections
- Use descriptive link text, not "click here"

### Language Guidelines

#### Requirement Levels (inspired by RFC 2119)

When documenting requirements, use these keywords consistently:

- **MUST/REQUIRED**: Absolute requirements
- **MUST NOT**: Absolute prohibitions  
- **SHOULD/RECOMMENDED**: Strong recommendations with valid exceptions
- **MAY/OPTIONAL**: Truly optional features

Example: "The Process Engine MUST validate all API requests"

#### Clarity Principles

- Write for developers who are new to the project
- Define domain terms on first use
- Prefer concrete examples over abstract descriptions
- Keep sentences concise and paragraphs focused

### Document Types

1. **Conceptual** (like ARCHITECTURE.md)
   - Focus on the "what" and "why"
   - Avoid implementation details
   - Use domain language

2. **Specification** (like MVP-SPECIFICATION.md)
   - Enumerate features and requirements
   - Use consistent formatting for easy scanning
   - Include acceptance criteria where applicable

3. **Technical** (like IMPLEMENTATION.md and EXTENSION-GUIDE.md)
   - Focus on the "how"
   - Include code examples
   - Reference specific technologies and versions

4. **Operational** (like planned DEPLOYMENT-GUIDE.md)
   - Step-by-step procedures
   - Prerequisites clearly stated
   - Troubleshooting sections

### Maintenance

- Review documents quarterly for accuracy
- Update cross-references when restructuring
- Archive deprecated documents with clear notices
- Track major changes in commit messages

## Contributing to Documentation

When adding new documentation:

1. Place all documentation in this directory
2. Determine the appropriate document type
3. Follow the structure and formatting conventions
4. Add entry to this README's index
5. Ensure all cross-references work

Changes will be automatically published via GitHub Pages.