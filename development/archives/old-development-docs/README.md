# Archived Development Documentation

## Purpose

This directory contains historical development documentation that:
1. Represents early investigations that led to rejected patterns
2. Contains outdated patterns that violate current architectural imperatives
3. Shows the evolution of thinking but no longer reflects current stance

## Why These Files Were Archived

These documents were archived on 2025-08-11 because they contain patterns and approaches that violate the clarified architectural imperatives:
- Repository pattern references (we reject repositories - DbContext IS the data layer)
- DDD praxis implementations (we embrace DDD ontology but reject its praxis)
- Mocking of internal services (we only mock external services)
- Primary key strategies that don't support user partitioning

## Contents

### Core Documents
- **ARCHITECTURAL-DIVERGENCES.md** - Treated clarified imperatives as "divergences" 
- **CONTEXT-RECOVERY.md** - Referenced repository patterns and outdated workflow

### Phase Investigations & Completions
- **phase-02-domain-models/** - DDD investigation without clear rejection of praxis
- **phase-03-repository-pattern/** - Repository pattern investigation (led to rejection)
- **phase-04-through-10/** - Retroactive journey documenting process violations
- **phase-04-api-completion/** - Built on wrong primary key structure
- **phase-05-process-engine-completion/** - Contains references to internal mocking
- **phase-06-platform-services-completion/** - Assumes wrong database schema
- **phase-07-user-journey-completion/** - Tests with mocking violations
- **phase-11-blazor-ui/** - Planning based on flawed foundation
- **dialectical-review-phase-4-7/** - Review of flawed implementations
- **epistemic-self-review/** - Self-review documenting process violations

## Historical Value

These documents are preserved because:
1. They show the journey of discovery that led to current imperatives
2. They demonstrate why certain patterns were investigated and rejected
3. They maintain epistemic integrity - we don't hide our learning process

## Current Authoritative Sources

For current architectural guidance, refer to:
- `/docs/ARCHITECTURE.md` - Authoritative architecture with clarified imperatives
- `/docs/DESIGN-PATTERNS.md` - Correct patterns (direct DbContext, no repositories)
- `/docs/IMPLEMENTATION.md` - Implementation philosophy
- `/development/ALIGNMENT-GAPS.md` - Current critical issues to fix

## Warning

**DO NOT FOLLOW PATTERNS IN THESE ARCHIVED FILES**

These files represent historical investigation and contain patterns we explicitly reject. They are preserved for historical record only.

---

*Archived: 2025-08-11*  
*Reason: Violation of clarified architectural imperatives*  
*Status: Historical reference only - DO NOT IMPLEMENT*