# Veritheia

*From Veritas (Latin: truth) and alētheia (Greek: truth as "uncoveredness")*

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## The Problem

You have thousands of documents to understand—research papers, reports, course materials. AI tools promise to help by generating summaries and answers, but this creates a deeper problem: when AI reads for you, the understanding isn't yours. You become dependent on AI interpretation rather than developing your own comprehension.

## What Veritheia Does

Veritheia helps you engage with large document collections while ensuring every insight remains yours. Instead of generating summaries, it measures documents against YOUR questions, using YOUR definitions, within YOUR framework. You build understanding through engagement, not consumption.

**Veritheia is open source (MIT licensed)**, enabling institutions and individuals to run their own instances while maintaining complete control over their intellectual work.

## Who It's For

- **Researchers** conducting systematic literature reviews
- **Educators** designing curricula and assessments
- **Students** building genuine understanding, not just answers
- **Professionals** analyzing domain-specific documents
- **Anyone** who needs to understand large document sets while maintaining intellectual ownership

## How It's Different

**Traditional AI**: Reads documents → Generates summaries → You consume

**Veritheia**: You define framework → AI measures documents → You author understanding

The key innovation: You write rules in plain English ("Papers are relevant if they provide empirical evidence"), and these become the system's operating instructions. No programming required—your words literally control how documents are processed.

## Development Philosophy: Specification-First

**Veritheia follows strict specification-first development.** Complete specifications are written in `/docs` before any implementation. The implementation, which is majority AI-assisted, must follow the spec exactly—it cannot exceed or diverge from what is specified. This ensures architectural coherence and prevents feature creep.

## 🚨 CRITICAL WARNING: AI Implementation Bias

**DO NOT allow AI agents to implement code without explicit architectural debiasing.**

AI training data contains fundamentally WRONG patterns that will violate this project's clean enterprise architecture:

**AI Will Automatically Add (ALL WRONG):**
- HTTP calls between Web and ApiService layers
- DTO classes for every entity transfer  
- AutoMapper and conversion layers
- Repository pattern and unnecessary abstractions
- "Best practice" patterns that create architectural bloat

**This Project Uses CLEAN ENTERPRISE:**
- **Web → ApiService → Data** (all in-process calls)
- **NO HTTP within application boundary**
- **NO DTOs** (use Entities + ViewModels for display only)
- **NO AutoMapper, no Repository pattern**
- **Direct service-to-service communication**

**Implementation must be human-guided with explicit direction and continuous output review to override AI training bias.**

## Current Implementation Status

**Architecture**: Specification defines composable component-based system with in-process communication
- **ApiService**: Should be pure business logic library (Application Programming Interface, not HTTP REST)
- **Web**: Should import ApiService directly for in-process communication
- **ApiGateway**: HTTP API component for external integration
- **MCPGateway**: AI agent integration via Model Context Protocol

**Current State**: Core formation patterns implemented with user journey management
- ✅ User authentication and data isolation
- ✅ Journey creation and management
- ✅ Persona-based intellectual frameworks
- ✅ Database with PostgreSQL 17 + pgvector
- ⚠️ **Architectural Divergence**: Implementation uses HTTP calls between Web and ApiService
- 🎯 **Next**: Architectural refactoring to match specification, then process execution integration

**Key Principle**: Users remain the authors of their intellectual work through direct engagement with documents, not AI-generated summaries.

**Note**: The implementation currently diverges from the specification. The system is functional but uses HTTP communication instead of direct method calls. See [Development Progress](development/PROGRESS.md) for details on required architectural refactoring.

<img width="3840" height="2160" alt="Screenshot 2025-08-26 121320" src="https://github.com/user-attachments/assets/90753eb1-de9f-4374-9c30-08e5bc1fb7f7" />

## Documentation

Comprehensive specifications (written before implementation) are available in the [docs](docs/) directory:

- [Documentation Index](docs/README.md) - Complete guide to all documentation
- [Vision](docs/01-VISION.md) - Why Veritheia exists and what it enables
- [User Guide](docs/02-USER-GUIDE.md) - What you can do with Veritheia
- [Architecture](docs/03-ARCHITECTURE.md) - System design and conceptual model
- [Implementation](docs/04-IMPLEMENTATION.md) - Technical details and development guide
- [MVP Specification](docs/05-MVP-SPECIFICATION.md) - Feature requirements and functionality
- [AI Agent Guide](docs/14-AI-AGENT-GUIDE.md) - Epistemic collaboration principles for AI agents
- [Authentication System](docs/17-AUTHENTICATION-SYSTEM.md) - User identity and data isolation patterns
- [Composable Extension Patterns](docs/18-COMPOSABLE-EXTENSION-PATTERNS.md) - Timeless specification patterns
- [Project Architecture](docs/19-PROJECT-ARCHITECTURE.md) - System structure and communication patterns
- [Foundational Papers](docs/papers/) - Research papers informing the architecture


## Quick Start

```bash
# Build the solution
dotnet build

# Run with .NET Aspire
dotnet run --project veritheia.AppHost
```

## Testing

### Running Tests

```bash
# Run CI-safe tests (excludes LLM integration)
dotnet test --filter "Category!=LLMIntegration"

# Run all tests including database integration (local only)
dotnet test

# Run only LLM integration tests (requires local LLM server)
dotnet test --filter "Category=LLMIntegration"

# Run specific test categories
dotnet test --filter "Category=Unit"           # Unit tests only
dotnet test --filter "Category=Integration"    # Integration tests only
```

### Test Categories
- **Unit Tests**: Fast, isolated tests with mocks (run in CI)
- **Integration**: Database + service tests using mocks (run in CI)  
- **LLMIntegration**: Tests requiring real LLM server (local only, excluded from CI)

### Test Infrastructure
Our tests use Testcontainers to spin up PostgreSQL with pgvector automatically:
- **No configuration needed** - Works identically locally and in CI
- **Isolation** - Each test run gets a fresh database
- **Real PostgreSQL** - Tests run against actual PostgreSQL 17 with pgvector

## CI/CD Workflows

### test.yml - Quick Test Runner
- **Trigger**: Push/PR to main, master, develop branches
- **Purpose**: Fast feedback on test status
- **Features**:
  - Runs all tests using Testcontainers
  - Generates test reports
  - Uploads test results as artifacts

### ci.yml - Complete CI/CD Pipeline
- **Trigger**: Push/PR to main, master branches, and version tags
- **Purpose**: Full validation and release pipeline
- **Features**:
  - Multi-OS testing (Ubuntu, Windows, macOS)
  - Code quality checks
  - Test coverage reporting
  - Docker image building
  - Automated releases for version tags

## Current Status

See [Development Progress](development/PROGRESS.md) for detailed phase implementation status.

## Technical Requirements

- .NET 9 SDK (for native UUIDv7 support)
- Docker Desktop (for PostgreSQL container)
- .NET Aspire workload

## Research Foundation

The architecture and its methodologies are derived from the following research.

- Syah, R. A., Haryanto, C. Y., Lomempow, E., Malik, K., & Putra, I. (2025). EdgePrompt: Engineering Guardrail Techniques for Offline LLMs in K-12 Educational Settings. In *Companion Proceedings of the ACM on Web Conference 2025 (WWW '25 Companion)*. Association for Computing Machinery, New York, NY, USA, 1635–1638. Published: 23 May 2025. [https://doi.org/10.1145/3701716.3717810](https://doi.org/10.1145/3701716.3717810)

- Haryanto, C. Y., & Lomempow, E. (2025). Cognitive Silicon: An Architectural Blueprint for Post-Industrial Computing Systems. *arXiv preprint arXiv:2504.16622*. [https://doi.org/10.48550/arXiv.2504.16622](https://doi.org/10.48550/arXiv.2504.16622)

- Haryanto, C. Y. (2024). LLAssist: Simple Tools for Automating Literature Review Using Large Language Models. *arXiv preprint arXiv:2407.13993v3 [cs.DL]*. Presented at CIE51, 11 Dec 2024. [https://doi.org/10.48550/arXiv.2407.13993](https://doi.org/10.48550/arXiv.2407.13993)

- Haryanto, C. Y., Elvira, A. M., Nguyen, T. D., Vu, M. H., Hartanto, Y., Lomempow, E., & Arakala, A. (2024). Contextualized AI for Cyber Defense: An Automated Survey using LLMs. In *2024 17th International Conference on Security of Information and Networks (SIN)*, 02-04 December 2024. IEEE. DOI: [10.1109/SIN63213.2024.10871242](https://doi.org/10.1109/SIN63213.2024.10871242). Also available: *arXiv:2409.13524 [cs.CR]*. [https://doi.org/10.48550/arXiv.2409.13524](https://doi.org/10.48550/arXiv.2409.13524)

- Haryanto, C. Y. (2024). Progress: A Post-AI Manifesto. *arXiv preprint arXiv:2408.13775*. [https://doi.org/10.48550/arXiv.2408.13775](https://doi.org/10.48550/arXiv.2408.13775)


