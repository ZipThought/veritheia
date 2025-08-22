# Development Tools and Tracking

This folder contains all development-specific tools, scripts, and tracking documents for the Veritheia implementation.

## 🚨 CRITICAL: READ FIRST 🚨

### Foundation Layer Complete (2025-08-20)

The **Foundation Layer (Milestone 0-2)** has been successfully implemented:

**✅ Milestone 0: Database Schema**
- Composite primary keys `(UserId, Id)` for all user-owned entities
- Partition-aware indexes for locality
- Migration `CompositePrimaryKeys` ready for deployment

**✅ Milestone 1: Core Domain Entities**
- All entities implement `IUserOwned` interface
- UserId property added to all user-owned entities
- Partition boundaries enforced at entity level

**✅ Milestone 2: Query Infrastructure**
- `QueryExtensions` class with `.ForUser<T>()` methods
- Journey-scoped operations (`.ForJourney()`, `.WithAssessments()`, etc.)
- Partition enforcement at query level

**🔄 CURRENT: Milestone 3 - Test Infrastructure**
- Replacing InMemory database with real PostgreSQL + Respawn
- Updating test files to use partition-aware patterns
- Removing references to deleted properties

### Previous Alignment Gaps (ARCHIVED)

**[archives/old-development-docs/ALIGNMENT-GAPS.md](./archives/old-development-docs/ALIGNMENT-GAPS.md)** - Contains previous epistemic scan findings showing discrepancies between documented claims and actual implementation status.

⚠️ **Previous Finding**: Implementation was ~40-50% complete, not the claimed 85%. Test infrastructure broken. Journey projection spaces not implemented. **These gaps have been addressed through foundation layer implementation.**

## Contents

### Core Workflow
- **DEVELOPMENT-WORKFLOW.md** - MANDATORY workflow: Human → Journey → Docs → Code

### Progress Tracking
- **PROGRESS.md** - Main implementation tracker with PDCA cycles for all dependency levels
- **PDCA-WORKFLOW.md** - Detailed workflow guide for Plan-Do-Check-Act methodology
- **IMPLEMENTATION-SUMMARY.md** - One-page summary of implementation plan

### Implementation Planning
- **IMPLEMENTATION-SUMMARY.md** - One-page summary of implementation plan

### Archives
- **archives/** - Historical documents with outdated patterns (DO NOT FOLLOW)

### Scripts
- **check-progress.sh** - Automated progress checker script

## Quick Start

After a context switch:
```bash
cd development
./check-progress.sh
# Then read docs/README.md for documentation index
cat IMPLEMENTATION-SUMMARY.md
```

## Usage

1. **Track Progress**: Update PROGRESS.md as you complete work
2. **Check Status**: Run `./check-progress.sh` for quick overview
3. **Recover Context**: Read IMPLEMENTATION-SUMMARY.md when resuming work
4. **Follow PDCA**: Use PDCA-WORKFLOW.md for quality implementation

## Important Rules

- Always read complete files (no partial reads)
- Update PROGRESS.md before context switches
- Commit frequently with descriptive messages
- Keep source files < 500 lines, docs < 1000 lines
- **Follow partition boundaries** - all queries must use `.ForUser<T>()`