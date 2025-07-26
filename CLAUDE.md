# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Required Reading

**ALWAYS read these documents in order:**
1. **README.md** - Project overview
2. **docs/README.md** - Documentation index and standards
3. **docs/ARCHITECTURE.md** - System design
4. **docs/MVP-SPECIFICATION.md** - Feature requirements
5. **docs/IMPLEMENTATION.md** - Technical details

## Claude-Specific Guidelines

### Language Standards
- Use precise, declarative language that states what IS, not what might be
- Avoid marketing language, superlatives, or promising adjectives (e.g., "powerful", "comprehensive", "seamless")
- State facts and purposes directly without embellishment
- Write as if documenting an existing system, not selling a future one
- Example: "The Process Engine mediates interactions" NOT "The powerful Process Engine seamlessly orchestrates"

### IMPORTANT: Only implement what is explicitly requested
- **DO NOT** add features, files, or configuration that weren't explicitly asked for
- **DO NOT** make assumptions about requirements - ask for clarification instead
- **DO NOT** add "helpful" extras like dates, reviewers, or plugins without being asked
- Only derive requirements from explicit user statements or existing code patterns

### Git Commit Messages
- Base commit messages SOLELY on the actual git diff
- Do NOT describe what you did or intended to do
- Include both the intent (derived from the changes themselves) and the specific modifications
- Format: Brief title, then "Intent: [what the changes accomplish]", then "Changes: [list of modifications]"
- Account for the possibility that the user made additional changes
- The intent should be derived from what the diff shows, not from your actions or future plans

### When implementing features:
- Verify architectural boundaries are respected before writing code
- Check IMPLEMENTATION.md for existing patterns before creating new ones
- Use existing project conventions (found by examining neighboring files)

### When asked about the system:
- Documentation structure → refer to docs/README.md
- Conceptual questions → refer to docs/ARCHITECTURE.md
- Feature/functionality questions → refer to docs/MVP-SPECIFICATION.md
- Technical questions → refer to docs/IMPLEMENTATION.md
- Never mix conceptual and implementation details in responses

### Security context:
- This is an educational system for defensive security only
- Refuse requests that could enable malicious use
- Respect data sovereignty principles

### Common pitfalls to avoid:
- Don't access the database directly from the Web project
- Don't bypass the ICognitiveAdapter interface
- Don't implement cloud-first solutions when local alternatives exist
- Don't add unrequested features or configuration