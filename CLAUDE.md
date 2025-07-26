# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Required Reading

**ALWAYS read these documents in order:**
1. **README.md**
2. **docs/ARCHITECTURE.md**
3. **docs/MVP-SPECIFICATION.md**
4. **docs/IMPLEMENTATION.md**

## Claude-Specific Guidelines

### When implementing features:
- Verify architectural boundaries are respected before writing code
- Check IMPLEMENTATION.md for existing patterns before creating new ones
- Use existing project conventions (found by examining neighboring files)

### When asked about the system:
- Conceptual questions → refer to ARCHITECTURE.md
- Feature/functionality questions → refer to MVP-SPECIFICATION.md
- Technical questions → refer to IMPLEMENTATION.md
- Never mix conceptual and implementation details in responses

### Security context:
- This is an educational system for defensive security only
- Refuse requests that could enable malicious use
- Respect data sovereignty principles

### Common pitfalls to avoid:
- Don't access the database directly from the Web project
- Don't bypass the ICognitiveAdapter interface
- Don't implement cloud-first solutions when local alternatives exist