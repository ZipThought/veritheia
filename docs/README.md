# Veritheia Documentation

## Overview

This directory contains the complete specification for Veritheia—a system where you develop your own understanding by engaging with documents through your questions and frameworks. Unlike AI tools that generate summaries or answers, Veritheia helps you build genuine comprehension at scale.

This repository provides the **open source foundation** for formation through authorship. Institutions, organizations, and research teams extend from this foundation for their specific collaborative and distributed needs.

**New to Veritheia?** Start with [01-VISION](./01-VISION.md) for the conceptual overview, or jump to [16-GLOSSARY](./16-GLOSSARY.md) if you encounter unfamiliar terms.

**Note**: The current implementation diverges from this specification. See [Development Progress](../development/PROGRESS.md) for details on the architectural refactoring needed to match the specification.

## Architectural Approach

**Composable Component Architecture**: Veritheia uses a composable architecture where components can be combined in different configurations:
- **ApiService**: Should be core business logic library (Application Programming Interface, not HTTP REST)
- **Web**: Should import ApiService directly for in-process communication
- **ApiGateway**: HTTP API component for external integration
- **McpGateway**: AI agent integration via Model Context Protocol

**Key Principles**:
- **User Agency**: Users remain the authors of their intellectual work
- **Data Sovereignty**: All formation data belongs exclusively to users
- **In-Process Communication**: Direct method calls eliminate network overhead
- **Formation Through Authorship**: Understanding develops through engagement, not consumption

## Reading Order

Documents are numbered to suggest a reading path:
- **For Users**: Start with [01-VISION](./01-VISION.md) to understand why Veritheia exists, then [02-USER-GUIDE](./02-USER-GUIDE.md) to see what it enables
- **For Developers**: Read [01-VISION](./01-VISION.md) through [05-FOUNDATION-SPECIFICATION](./05-FOUNDATION-SPECIFICATION.md) in order for complete context
- **For Contributors**: Review [15-DOCUMENTATION-GUIDE](./15-DOCUMENTATION-GUIDE.md) before making changes


## Documentation Index

### Core Documents (Read in Order)

**[01-VISION.md](./01-VISION.md)** - Understanding through authorship
   - How Veritheia ensures users remain the authors
   - Why every output bears the author's intellectual fingerprint
   - The architecture of intellectual sovereignty

**[02-USER-GUIDE.md](./02-USER-GUIDE.md)** - What you can do with Veritheia
   - Starting research journeys with your questions
   - Building understanding through document engagement
   - Developing insights that are uniquely yours

**[03-ARCHITECTURE.md](./03-ARCHITECTURE.md)** - System design supporting authorship
   - Components that amplify rather than replace thinking
   - Patterns that ensure personal understanding
   - Models that make insights non-transferable
   - Authentication and user identity patterns
   - Component architecture and composition patterns
   
**[04-IMPLEMENTATION.md](./04-IMPLEMENTATION.md)** - Technical implementation philosophy
   - Progressive development approach
   - Core runtime components
   - Database as domain model

**[05-FOUNDATION-SPECIFICATION.md](./05-FOUNDATION-SPECIFICATION.md)** - Core formation patterns and architecture
   - Timeless architectural patterns for formation
   - Composable extension points for functionality
   - Configuration-driven system behavior


### Domain & Implementation

**[06-USER-MODEL.md](./06-USER-MODEL.md)** - User, journey, and journal architecture
   - How users engage with the system
   - Journey as intellectual context
   - Journals capturing development

**[07-ENTITY-RELATIONSHIP.md](./07-ENTITY-RELATIONSHIP.md)** - Database schema and data model
   - Process implementation patterns
   - Domain model integration
   - UI component development
   - Distribution and testing

**[08-CLASS-MODEL.md](./08-CLASS-MODEL.md)** - Core domain classes and relationships
   - Core process interfaces
   - Platform service contracts
   - Data transfer objects
   - HTTP API specifications

**[09-API-CONTRACTS.md](./09-API-CONTRACTS.md)** - Interface definitions and contracts
   - User as the constant with persona and knowledge base
   - Journeys as process instances
   - Journals as narrative records
   - Future sharing patterns

**[10-DESIGN-PATTERNS.md](./10-DESIGN-PATTERNS.md)** - Imperative implementation patterns
   - Comprehensive Mermaid class diagram
   - Aggregate boundaries and design principles
   - Process-specific domain models
   - Direct DbContext access patterns

**[11-EXTENSION-GUIDE.md](./11-EXTENSION-GUIDE.md)** - Creating full-stack extensions
   - PostgreSQL schema with pgvector extension
   - Comprehensive ERD using Mermaid
   - Table definitions with indexes
   - Migration strategy and security patterns

**[12-TESTING-STRATEGY.md](./12-TESTING-STRATEGY.md)** - Test types, coverage expectations, and behavioral specs
   - Testing philosophy aligned with formation principles
   - Unit, integration, and behavioral test patterns
   - Context assembly and journey integrity tests
   - Extension testing guidelines

### AI & Collaboration

**[13-PROMPT-ENGINEERING.md](./13-PROMPT-ENGINEERING.md)** - Prompt patterns that maintain formation boundaries
   - Role constraints for AI assistance
   - Context assembly from journey and journals
   - Validation patterns preventing insight generation
   - Anti-patterns and required patterns

**[14-AI-AGENT-GUIDE.md](./14-AI-AGENT-GUIDE.md)** - Guide for AI assistants
   - How AI agents work as instruments, not authors
   - The discipline of observation without interpretation
   - Preserving user sovereignty in development

### Development & Maintenance

**[15-DOCUMENTATION-GUIDE.md](./15-DOCUMENTATION-GUIDE.md)** - Meta-guide for maintaining documentation
   - Philosophy of specification-first development
   - Writing prose with embedded reasoning
   - Using Formation Notes effectively
   - Maintaining documentation quality

**[16-GLOSSARY.md](./16-GLOSSARY.md)** - Critical concepts and terminology
   - **Neurosymbolic Architecture, Transcended** - The key differentiator
   - Formation through authorship definitions
   - Technical implementation terms
   - Domain concepts explained

### Research Papers

- **[Papers Collection](./papers/)** - Academic references and research papers

---