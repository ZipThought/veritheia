# Veritheia Implementation Summary

## Quick Reference for Context Switches

### Requirements
- .NET 9 SDK (for native UUIDv7 support via Guid.CreateVersion7())
- Docker Desktop (for PostgreSQL 17 container)
- .NET Aspire workload (`dotnet workload install aspire`)

### Current Status
- **Active Phase**: Phase 1 - Database Infrastructure (In Progress)
- **Last Update**: 2025-08-09 - Tables created, repositories not yet implemented
- **Next Action**: Implement repository pattern for CRUD operations

### Implementation Order
1. Database → 2. Models → 3. Repositories → 4. APIs → 5. Process Engine → 6. Services → 7. User System → 8. Cognitive → 9. Screening → 10. Composition → 11. UI → 12. Testing

### Key Commands
```bash
# Check progress
./development/check-progress.sh

# Start PostgreSQL 17 with pgvector (requires Docker)
dotnet run --project veritheia.AppHost

# Database work 
cd veritheia.Data
dotnet ef migrations add <MigrationName>
dotnet ef database update

# Access PostgreSQL (port 57233 - may vary)
docker exec <container-name> sh -c "PGPASSWORD='<password>' psql -U postgres -d veritheiadb"
```

### Development Workflow (MANDATORY)
1. **READ FIRST**: development/DEVELOPMENT-WORKFLOW.md
2. **Signal gaps** before making changes
3. **Wait for human decisions** on ambiguities
4. **Follow the flow**: Human → [Journey?] → Docs → Code

### PDCA for Each Phase
1. **PLAN**: Read relevant docs (CLASS-MODEL, ENTITY-RELATIONSHIP, API-CONTRACTS)
2. **DO**: Implement with frequent commits, update development/PROGRESS.md
3. **CHECK**: Run tests, verify against specifications
4. **ACT**: Document learnings, prepare for next phase

### Critical Rules
- **ALWAYS read entire files** - no partial reads
- **Signal and stop** - confirm before proceeding (use format from DEVELOPMENT-WORKFLOW.md)
- **Commit frequently** with descriptive messages
- **Update development/PROGRESS.md** before context switches
- **Test as you go** - don't accumulate untested code

### File Size Limits
- Source files: < 500 lines
- Documentation: < 1000 lines
- If larger → refactor first

### Recovery After Context Switch
1. Run `./development/check-progress.sh`
2. Read development/PROGRESS.md current phase
3. Check git status and recent commits
4. Continue where you left off

Remember: The PDCA tracking in development/PROGRESS.md is your source of truth.