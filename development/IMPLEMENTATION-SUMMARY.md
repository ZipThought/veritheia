# Veritheia Implementation Summary

## Quick Reference for Context Switches

### Requirements
- .NET 9 SDK (for native UUIDv7 support via Guid.CreateVersion7())
- Docker Desktop (for PostgreSQL 17 container)
- .NET Aspire workload (`dotnet workload install aspire`)

### Current Status
- **Active Phase**: Milestone 3 - Test Infrastructure (In Progress)
- **Last Update**: 2025-08-20 - Foundation Layer Complete (Milestone 0-2)
- **Next Action**: Replace InMemory database with real PostgreSQL + Respawn

### Implementation Order (Dependency Graph)
1. ✅ **Milestone 0: Database Schema** - COMPLETE (Composite primary keys)
2. ✅ **Milestone 1: Core Domain Entities** - COMPLETE (IUserOwned implementation)
3. ✅ **Milestone 2: Query Infrastructure** - COMPLETE (QueryExtensions)
4. 🔄 **Milestone 3: Test Infrastructure** - IN PROGRESS (PostgreSQL + Respawn)
5. ⏳ **Milestone 4: Platform Services** - PENDING (Journey-aware services)
6. ⏳ **Milestone 5: Cognitive Adapter** - PENDING (Journey-aware assessment)
7. ⏳ **Milestone 6: Process Engine** - PENDING (Neurosymbolic orchestration)
8. ⏳ **Milestone 7: Journey Projection Logic** - PENDING (Core innovation)
9. ⏳ **Milestone 8: Reference Processes** - PENDING (SystematicScreening, ConstrainedComposition)
10. ⏳ **Milestone 9: API Layer** - PENDING (Journey-aware endpoints)
11. ⏳ **Milestone 10: User Interface** - PENDING (Blazor Server)

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

### PDCA for Each Level
1. **PLAN**: Read relevant docs (ARCHITECTURE.md, IMPLEMENTATION.md, MVP-SPECIFICATION.md)
2. **DO**: Implement with frequent commits, update development/PROGRESS.md
3. **CHECK**: Run tests, verify against specifications
4. **ACT**: Document learnings, prepare for next level

### Critical Rules
- **ALWAYS read entire files** - no partial reads
- **Signal and stop** - confirm before proceeding (use format from DEVELOPMENT-WORKFLOW.md)
- **Commit frequently** with descriptive messages
- **Update development/PROGRESS.md** before context switches
- **Test as you go** - don't accumulate untested code
- **Follow partition boundaries** - all queries must use `.ForUser<T>()`

### File Size Limits
- Source files: < 500 lines
- Documentation: < 1000 lines
- If larger → refactor first

### Recovery After Context Switch
1. Run `./development/check-progress.sh`
2. Read development/PROGRESS.md current level
3. Check git status and recent commits
4. Continue where you left off

### Foundation Layer Achievements (Level 0-2)
- ✅ **Composite Primary Keys**: All user-owned entities use `(UserId, Id)`
- ✅ **Partition Enforcement**: QueryExtensions enforce user boundaries
- ✅ **Migration Ready**: `CompositePrimaryKeys` migration created
- ✅ **Build Success**: Main projects compile successfully

Remember: The PDCA tracking in development/PROGRESS.md is your source of truth.