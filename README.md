# Veritheia

*From Veritas (Latin: truth) and alētheia (Greek: truth as "uncoveredness")*

### I. What It Is

Veritheia is epistemic infrastructure—a place to think, reflect, remember, and author knowledge.

You bring your own source material—books, PDFs, journal entries, recordings—and through Veritheia you compose your own synthesis, develop your own insights, and build structured knowledge that is yours in substance and form.

It is public infrastructure for those engaged in real intellectual work: educators designing curricula, students seeking insight rather than answers, researchers mapping complex domains, and civic leaders cultivating understanding across communities.

### II. Architectural Imperatives

The environment is defined by its architecture.

Its perimeter is sovereign. Its reasoning is grounded. Its structure is open.

### III. The Purpose

The purpose of this architecture is to ensure that your intellectual work remains yours.

Through disciplined inquiry, you develop your own understanding. Through structured engagement, you author your own insights. Through formative practice, you become more capable—not more dependent.

### IV. Documentation

Comprehensive project documentation is available in the [docs](docs/) directory:

- [Documentation Index](docs/README.md) - Complete guide to all documentation
- [AI Agent Guide](docs/AI-AGENT-GUIDE.md) - Epistemic collaboration principles for AI agents
- [Architecture](docs/ARCHITECTURE.md) - System design and conceptual model
- [MVP Specification](docs/MVP-SPECIFICATION.md) - Feature requirements and functionality
- [Implementation](docs/IMPLEMENTATION.md) - Technical details and development guide
- [Foundational Papers](papers/) - Research papers informing the architecture


### V. Quick Start

```bash
# Build the solution
dotnet build

# Run with .NET Aspire
dotnet run --project veritheia.AppHost
```

### VI. Testing

#### Running Tests

```bash
# Run all tests (excluding integration)
dotnet test --filter "Category!=Integration&Category!=LLMIntegration"

# Run LLM integration tests (requires local LLM server)
dotnet test --filter "Category=LLMIntegration"

# With custom LLM server URL
LLM_URL=http://localhost:1234 dotnet test --filter "Category=LLMIntegration"
```

#### Test Categories
- **Unit Tests**: Fast, isolated tests (run in CI)
- **Integration**: Database tests using Testcontainers (local only)
- **LLMIntegration**: Tests requiring LLM server (local only)

### VII. Current Status

See [Development Progress](development/PROGRESS.md) for detailed phase implementation status.

### VIII. Technical Requirements

- .NET 9 SDK (for native UUIDv7 support)
- Docker Desktop (for PostgreSQL container)
- .NET Aspire workload

### IX. Provenance

The architecture and its methodologies are derived from the following research.

- Syah, R. A., Haryanto, C. Y., Lomempow, E., Malik, K., & Putra, I. (2025). EdgePrompt: Engineering Guardrail Techniques for Offline LLMs in K-12 Educational Settings. In *Companion Proceedings of the ACM on Web Conference 2025 (WWW '25 Companion)*. Association for Computing Machinery, New York, NY, USA, 1635–1638. Published: 23 May 2025. [https://doi.org/10.1145/3701716.3717810](https://doi.org/10.1145/3701716.3717810)

- Haryanto, C. Y., & Lomempow, E. (2025). Cognitive Silicon: An Architectural Blueprint for Post-Industrial Computing Systems. *arXiv preprint arXiv:2504.16622*. [https://doi.org/10.48550/arXiv.2504.16622](https://doi.org/10.48550/arXiv.2504.16622)

- Haryanto, C. Y. (2024). LLAssist: Simple Tools for Automating Literature Review Using Large Language Models. *arXiv preprint arXiv:2407.13993v3 [cs.DL]*. Presented at CIE51, 11 Dec 2024. [https://doi.org/10.48550/arXiv.2407.13993](https://doi.org/10.48550/arXiv.2407.13993)

- Haryanto, C. Y., Elvira, A. M., Nguyen, T. D., Vu, M. H., Hartanto, Y., Lomempow, E., & Arakala, A. (2024). Contextualized AI for Cyber Defense: An Automated Survey using LLMs. *arXiv preprint arXiv:2409.13524*. [https://doi.org/10.48550/arXiv.2409.13524](https://doi.org/10.48550/arXiv.2409.13524)

- Haryanto, C. Y. (2024). Progress: A Post-AI Manifesto. *arXiv preprint arXiv:2408.13775*. [https://doi.org/10.48550/arXiv.2408.13775](https://doi.org/10.48550/arXiv.2408.13775)

