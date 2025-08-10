# Development Tools and Tracking

This folder contains all development-specific tools, scripts, and tracking documents for the Veritheia implementation.

## 🚨 CRITICAL: READ FIRST 🚨

**[ALIGNMENT-GAPS.md](./ALIGNMENT-GAPS.md)** - URGENT documentation gaps and implementation reality check. Contains epistemic scan findings showing significant discrepancies between documented claims and actual implementation status.

⚠️ **Key Finding**: Implementation is ~40-50% complete, not the claimed 85%. Test infrastructure broken. Journey projection spaces not implemented.

## Contents

### Core Workflow
- **DEVELOPMENT-WORKFLOW.md** - MANDATORY workflow: Human → Journey → Docs → Code

### Progress Tracking
- **PROGRESS.md** - Main implementation tracker with PDCA cycles for all 12 phases
- **PDCA-WORKFLOW.md** - Detailed workflow guide for Plan-Do-Check-Act methodology
- **ALIGNMENT-GAPS.md** - CRITICAL architectural violations and fixes needed

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
3. **Recover Context**: Read CONTEXT-RECOVERY.md when resuming work
4. **Follow PDCA**: Use PDCA-WORKFLOW.md for quality implementation

## Important Rules

- Always read complete files (no partial reads)
- Update PROGRESS.md before context switches
- Commit frequently with descriptive messages
- Keep source files < 500 lines, docs < 1000 lines