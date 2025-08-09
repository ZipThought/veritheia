# PDCA Workflow for Veritheia Implementation

This document defines the PDCA (Plan-Do-Check-Act) workflow for implementation across context switches.

## Core PDCA Principles

### QUALITY FIRST - Track All Incomplete Work
- Every commit must leave the system in a working state
- Temporary solutions and scaffolding are acceptable BUT must be:
  - Explicitly marked with TODO comments
  - Tracked in PROGRESS.md
  - Retained until properly implemented
- Each phase must be complete and verified before proceeding
- Error propagation is unacceptable - fix issues immediately

### PLAN - Define Clear Outcomes
- Each phase has 3-5 specific, verifiable deliverables
- Deliverables are binary (done/not done)
- Dependencies are explicitly stated
- Success criteria are measurable
- All files must be readable in full (< 500 lines for code, < 1000 for docs)

### DO - Document While Implementing
- Write notes immediately after key decisions
- Commit frequently with descriptive messages
- Update PROGRESS.md before context switches
- Use TODO comments for all incomplete work, temporary solutions, and scaffolding

### CHECK - Verify Systematically
- Automated tests where possible
- Manual verification checklist
- Cross-reference with documentation
- Verify no regressions

### ACT - Continuous Improvement
- Document learnings immediately
- Update approach for next phase
- Refactor if needed before moving on
- Update documentation if reality differs

## Implementation Workflow

### Starting a Phase

1. **MANDATORY: Follow Development Workflow**
   ```bash
   # STOP! Read the development workflow FIRST:
   cat development/DEVELOPMENT-WORKFLOW.md
   
   # Then read the documentation:
   
   # Step 1: Always start with docs index
   cat docs/README.md
   
   # Step 2: Check phase-specific documentation link
   grep -A 5 "Phase X:" PROGRESS.md | grep "Docs:"
   # Click the documentation link provided for the phase
   
   # Step 3: Read all referenced sections
   # Each PLAN item has a doc link - READ IT FIRST
   
   # WORKFLOW VIOLATIONS:
   # - Skipping human decision = VIOLATION
   # - Not signaling gaps = VIOLATION
   # - Coding without docs = VIOLATION  
   # - Assuming instead of asking = VIOLATION
   
   # The workflow ensures human sovereignty
   ```

2. **Create Feature Branch**
   ```bash
   git checkout -b phase-X-description
   ```

3. **Update PROGRESS.md**
   - Mark phase as "In Progress"
   - Add start date
   - Review PLAN section

### During Implementation

1. **Constant Documentation Reference**
   ```bash
   # Before implementing ANYTHING, verify against docs:
   # - Is this pattern in IMPLEMENTATION.md?
   # - Does this match the spec in MVP-SPECIFICATION.md?
   # - Am I following the architecture in ARCHITECTURE.md?
   
   # Example: Before creating a repository
   grep -n "Repository" docs/IMPLEMENTATION.md
   # Read the EXACT pattern specified
   ```

2. **Marking Incomplete Work**
   ```csharp
   // TODO: Implement actual validation logic
   public bool Validate() => true;  // Temporary implementation
   
   // TODO: Replace with real database connection
   private readonly List<User> _users = new();  // Scaffolding for testing
   
   // TODO: Add error handling
   public async Task<Document> ProcessAsync(string path)
   {
       // Simplified implementation for now
       return new Document { FilePath = path };
   }
   ```

2. **Frequent Commits**
   ```bash
   # Commit pattern: "Phase X: Specific change"
   git add .
   git commit -m "Phase 1: Add EF Core packages and create DbContext"
   ```

2. **Update DO Section**
   ```markdown
   #### DO (Implementation Notes)
   - Note: Using Npgsql.EntityFrameworkCore.PostgreSQL v8.0
   - Decision: Store Raw Corpus in ~/veritheia-data/corpus
   - Change: Added connection string to appsettings.Development.json
   ```

3. **Write Tests Immediately**
   ```csharp
   // After implementing a feature, write its test
   [Fact]
   public async Task Repository_CanSaveAndRetrieveUser()
   {
       // Test implementation
   }
   ```

### Checking Progress

1. **Run Verification Commands**
   ```bash
   # For database phases
   dotnet ef migrations list
   psql -c "SELECT * FROM pg_extension WHERE extname = 'vector';"
   
   # For code phases
   dotnet build
   dotnet test
   ```

2. **Manual Verification**
   - Open Swagger UI and test endpoints
   - Check database tables exist
   - Verify file storage locations

3. **Update CHECK Section**
   ```markdown
   #### CHECK (Verification)
   - [x] Database connects successfully
   - [x] pgvector extension installed
   - [ ] Migration runs without errors  # Still pending
   ```

### Acting on Results

1. **Document Learnings**
   ```markdown
   #### ACT (Next Steps)
   - Learning: pgvector requires PostgreSQL 15+
   - Improvement: Add version check to setup script
   - Dependency: Phase 2 can start after migration succeeds
   ```

2. **Create Issues for Problems**
   ```bash
   # If blocked, document it
   echo "| $(date +%Y-%m-%d) | Phase 1 | PostgreSQL version too old | Upgrade to v16 | - |" >> PROGRESS.md
   ```

3. **Refactor if Needed**
   ```bash
   # If approach needs change
   git checkout -b refactor-phase-X
   # Make improvements
   git commit -m "Phase X: Refactor based on learnings"
   ```

## Context Switch Protocol

### Before Switching Context

1. **Commit All Work**
   ```bash
   git add .
   git commit -m "WIP: Phase X - [describe current state]"
   ```

2. **Update PROGRESS.md**
   - Fill in DO section with current state
   - Check off completed items
   - Add any blockers to Issues table

3. **Write Context Note**
   ```markdown
   ## Context Switch Note [DATE]
   Currently working on: Phase X - Database Infrastructure
   Next step: Create initial migration after fixing connection string
   Blocked by: Need to install PostgreSQL locally
   Branch: phase-1-database
   ```

### After Context Switch

1. **Read Context Recovery**
   ```bash
   # Start here
   cat PROGRESS.md | head -50
   cat "Context Switch Note"
   git status
   git log -5 --oneline
   ```

2. **Verify System State**
   ```bash
   # Check what's running
   docker ps  # If using Docker
   dotnet ef migrations list
   dotnet test
   ```

3. **Resume PDCA Cycle**
   - If in DO: Continue implementation
   - If in CHECK: Run verifications
   - If in ACT: Document and move to next phase

## Quality Gates

Each phase must pass these gates before marking complete:

### Documentation Compliance
- [ ] All implementations match documented patterns
- [ ] No deviations from specifications without documented rationale
- [ ] All design patterns from IMPLEMENTATION.md followed
- [ ] Entity models match CLASS-MODEL.md exactly
- [ ] API contracts match API-CONTRACTS.md

### Code Quality
- [ ] All code compiles without warnings
- [ ] Code follows C# conventions
- [ ] All TODO comments resolved (no temporary solutions remain)
- [ ] Documentation comments on public APIs

### Testing
- [ ] Unit tests for business logic
- [ ] Integration tests for APIs
- [ ] All tests pass
- [ ] No skipped tests without justification

### Documentation
- [ ] PROGRESS.md updated completely
- [ ] API changes reflected in Swagger
- [ ] Any new patterns documented
- [ ] README updated if needed

### Review
- [ ] Self-review completed
- [ ] Matches specification documents
- [ ] No security issues
- [ ] Performance acceptable

## Automation Helpers

### Pre-commit Hook
```bash
#!/bin/bash
# .git/hooks/pre-commit

# Ensure PROGRESS.md is updated
if git diff --cached --name-only | grep -q "\.cs$"; then
    echo "Remember to update PROGRESS.md!"
    echo "Have you updated the DO section? (y/n)"
    read -r response
    if [[ "$response" != "y" ]]; then
        exit 1
    fi
fi
```

### Progress Check Script
```bash
#!/bin/bash
# scripts/check-progress.sh

echo "=== Veritheia Progress Check ==="
echo ""
echo "Current Phase Status:"
grep -E "^### Phase [0-9]+:|^\*\*Status\*\*:" PROGRESS.md | paste - - | column -t
echo ""
echo "Recent Commits:"
git log -5 --oneline
echo ""
echo "Test Status:"
dotnet test --no-build --nologo -v q
```

## Emergency Recovery

If completely lost:

1. **Check Git State**
   ```bash
   git status
   git stash list
   git branch -a
   ```

2. **Review All Progress**
   ```bash
   find . -name "*.md" -newer .git/FETCH_HEAD -exec echo {} \; -exec head -20 {} \;
   ```

3. **Run All Tests**
   ```bash
   dotnet clean
   dotnet build
   dotnet test
   ```

4. **Start Fresh from Last Known Good**
   ```bash
   git checkout main
   git pull
   # Review PROGRESS.md and continue
   ```

Remember: Clean implementation is mandatory. Temporary solutions must be explicitly marked and tracked. Unmarked technical debt is not acceptable.