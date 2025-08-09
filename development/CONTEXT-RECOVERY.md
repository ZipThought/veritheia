# Context Recovery Checklist

## Quick Start After Context Switch

When you return to this project after a context switch, start here:

0. **RTFM FIRST - NO EXCEPTIONS**
   ```bash
   # Read these IN ORDER before doing ANYTHING:
   cat docs/README.md              # Documentation index
   cat docs/ARCHITECTURE.md        # System design
   cat docs/MVP-SPECIFICATION.md   # What we're building
   cat development/PROGRESS.md     # Current status
   
   # Only AFTER reading the above, proceed to:
   ```

1. **Run Progress Check**
   ```bash
   ./development/check-progress.sh
   ```

2. **Find Current Phase**
   - Look for "In Progress" status in PROGRESS.md
   - Read the DO section for what was implemented
   - Check the CHECK section for what needs verification

3. **Key Files to Review**
   - `development/DEVELOPMENT-WORKFLOW.md` - MANDATORY workflow (read first!)
   - `development/PROGRESS.md` - Current implementation status
   - `development/CONTEXT-RECOVERY.md` - This file
   - `development/PDCA-WORKFLOW.md` - How to proceed with quality
   - Recent commits: `git log -10 --oneline`

## Project Structure Reminder

```
veritheia/
├── veritheia.ApiService/       # Main API (controllers, program.cs)
├── veritheia.Core/            # Domain models and interfaces
├── veritheia.Data/            # EF Core context and migrations
├── veritheia.Common/          # Shared DTOs and contracts
├── veritheia.Web/             # Blazor UI
├── veritheia.AppHost/         # Aspire orchestration
├── veritheia.Tests/           # Test projects
├── docs/                      # All documentation
├── development/               # Development tools and tracking
└── CLAUDE.md                 # AI assistant guidelines
```

## Current Implementation Plan

We are implementing 12 phases:

1. **Database Infrastructure** - PostgreSQL + pgvector + EF Core
2. **Core Domain Models** - Entities from CLASS-MODEL.md
3. **Repository Pattern** - Data access layer
4. **Knowledge Database APIs** - Document management endpoints
5. **Process Engine Infrastructure** - IAnalyticalProcess framework
6. **Platform Services** - Document processing pipeline
7. **User & Journey System** - Authentication and journey management
8. **Cognitive Adapter** - LLM integration (Ollama/OpenAI)
9. **Systematic Screening Process** - First analytical process
10. **Constrained Composition Process** - Educational process
11. **Blazor UI** - Desktop web interface
12. **Testing & Documentation** - Quality assurance

## Key Technical Decisions

- **Database**: PostgreSQL 16 with pgvector extension
- **ORM**: Entity Framework Core with migrations
- **API**: ASP.NET Core Web API with OpenAPI
- **UI**: Blazor Server for desktop-like experience
- **Orchestration**: .NET Aspire for local development
- **LLM**: Pluggable adapter (Ollama default, OpenAI optional)
- **Storage**: Local filesystem for Raw Corpus

## Common Commands

```bash
# Check progress
./development/check-progress.sh

# Database operations
dotnet ef migrations add [Name] --project veritheia.Data --startup-project veritheia.ApiService
dotnet ef database update --project veritheia.Data --startup-project veritheia.ApiService

# Run the application
dotnet run --project veritheia.AppHost

# Run tests
dotnet test

# Build everything
dotnet build
```

## Before Starting Work

1. **Pull latest changes**
   ```bash
   git pull origin main
   ```

2. **Check branch**
   ```bash
   git branch --show-current
   # If not on a feature branch:
   git checkout -b phase-X-description
   ```

3. **Review specifications**
   - What should this phase implement?
   - Check relevant docs (CLASS-MODEL.md, API-CONTRACTS.md, etc.)

4. **Update development/PROGRESS.md**
   - Mark phase as "In Progress"
   - Add today's date as start date

## During Implementation

1. **Commit frequently**
   ```bash
   git add .
   git commit -m "Phase X: Specific change description"
   ```

2. **Update development/PROGRESS.md DO section**
   - Add notes about decisions
   - Document any deviations
   - Record package versions

3. **Write tests as you go**
   - Unit tests for logic
   - Integration tests for APIs

## Before Context Switch

1. **Commit all work**
   ```bash
   git add .
   git commit -m "WIP: Phase X - Current state description"
   ```

2. **Update development/PROGRESS.md**
   - Fill in DO section
   - Check off completed items
   - Note any blockers

3. **Push to remote**
   ```bash
   git push origin [branch-name]
   ```

## Quality Checklist

Before marking a phase complete:

- [ ] All PLAN items checked off
- [ ] DO section documents implementation
- [ ] CHECK section verifications pass
- [ ] Tests written and passing
- [ ] No compiler warnings
- [ ] Code matches specifications
- [ ] ACT section filled with learnings

## Additional Resources

1. **Review documentation structure**
   ```bash
   cat docs/README.md
   ```

2. **Check architectural guidance**
   - `docs/ARCHITECTURE.md` - System design
   - `docs/IMPLEMENTATION.md` - Technical patterns
   - `docs/MVP-SPECIFICATION.md` - Feature requirements

3. **Verify against models**
   - `docs/CLASS-MODEL.md` - Domain entities
   - `docs/ENTITY-RELATIONSHIP.md` - Database schema
   - `docs/API-CONTRACTS.md` - Interface definitions

Remember: Clean implementation at each step. Plan, implement, verify, then proceed.