# Documentation Guide for Veritheia

## Purpose of Documentation

Documentation for Veritheia isn't just technical reference—it's part of the epistemic infrastructure itself. Clear documentation ensures that contributors understand not just what to build but why each piece matters for user sovereignty. Every word written should strengthen the principle that Veritheia enables formation through authorship, where users develop understanding through structured engagement with their documents, not through consumption of AI-generated outputs.

## Philosophy of Specification-First Development

Veritheia embraces specification-first development where complete documentation precedes implementation. This isn't bureaucratic overhead—it's architectural discipline. The specifications in `/docs` define the system's philosophical commitments and technical contracts. Implementation follows specification, never the reverse.

Why this matters: When specifications are complete before code is written, architectural drift becomes impossible. The system can evolve incrementally while maintaining coherence with its founding principles. Every implementation decision traces back to documented reasoning. Every technical choice serves the documented purpose of enabling formation through authorship.

## Writing Prose with Embedded Reasoning

Documentation should read as prose with embedded reasoning, not bullet-pointed specifications. When you explain a technical concept, show the thinking that led to it.

For instance, rather than listing "PostgreSQL with pgvector" as a choice, explain that PostgreSQL with pgvector provides unified storage for documents and embeddings within the same ACID boundary, eliminating synchronization complexity while enabling semantic search as a first-class domain operation. This isn't verbose—it's transparent reasoning.

### Example of Prose with Reasoning

**Poor**: 
- Database: PostgreSQL
- Vector storage: pgvector
- Benefits: unified storage

**Better**:
"The system employs PostgreSQL as an Object-Relational Database Management System (ORDBMS), leveraging its full capabilities rather than treating it as a simple data store. PostgreSQL's pgvector extension provides semantic search through high-dimensional vector operations, indexed using Hierarchical Navigable Small World (HNSW) graphs for logarithmic query complexity even at scale. This unified approach eliminates the synchronization complexity that would arise from separating relational, document, and vector stores into distinct systems."

The second version explains not just what but why, embedding the reasoning directly in the prose.

## Using Formation Notes

Formation Notes connect technical features to user authorship. These aren't decorative additions but essential bridges between mechanism and purpose. Format them as blockquotes beginning with "> **Formation Note:**"

### When to Use Formation Notes

Add Formation Notes when:
- Introducing a technical constraint that preserves user sovereignty
- Explaining how a feature enables formation through authorship
- Connecting database design to intellectual ownership
- Showing how architectural decisions support user agency

### Example Formation Note

> **Formation Note:** The composite primary keys (UserId, Id) aren't just partitioning—they're sovereignty boundaries ensuring your intellectual work remains yours. When PostgreSQL rejects a Journey without a Persona, it's protecting a truth we've discovered: every inquiry requires a perspective.

Formation Notes should appear immediately after technical explanations to ground them in purpose.

## Maintaining Balance Between Technical Precision and Accessible Explanation

Every document serves multiple audiences: developers implementing features, users understanding capabilities, and contributors grasping philosophy. Balance these needs through layered explanation.

Start with purpose (why this matters for formation), then explain the concept (what it does for users), then provide technical detail (how it's implemented). This progression serves all audiences without sacrificing precision.

### Example of Balanced Explanation

"Journey projection spaces enable users to engage with thousands of documents through their own intellectual lens. When documents enter a journey, they're not stored generically but projected into that journey's intellectual space—a systematic review projects documents through methodology sections and research questions, while an educational journey projects the same documents through learning objectives and rubrics. Technically, this is implemented through the `journey_document_segments` table where documents are segmented according to journey-specific rules stored in `journey_frameworks`, with embeddings generated using the journey's conceptual vocabulary as context."

## Connecting Every Technical Decision to Formation Through Authorship

No technical decision exists in isolation. Every choice—from database schema to API design—must trace back to enabling formation through authorship.

When documenting technical decisions:
1. State what the decision is
2. Explain how it enables user authorship
3. Show what it prevents (usually AI overreach)
4. Connect it to the larger pattern of sovereignty

### Example Connection

"The Process Engine implements neurosymbolic architecture, transcended, by mechanically orchestrating the systematic application of user-authored symbolic frameworks through neural semantic understanding. This ensures that ALL documents receive identical treatment—the engine mechanically processes every document without LLM judgment about which documents are 'worth' processing. This mechanical fairness is essential for formation: users must engage with systematically processed documents, not AI-curated selections."

## Guidelines for Public vs Development Documentation

### Public Documentation (`/docs`)

Public documentation defines the complete specification of Veritheia. These documents:
- Explain what the system is and why it exists
- Define technical architecture and contracts
- Specify features and capabilities
- Establish patterns and principles
- Are displayed at veritheia.com

Public documentation should be complete, coherent, and compelling. It presents Veritheia as fully-conceived epistemic infrastructure, even when implementation is ongoing.

### Development Documentation (`/development`)

Development documentation tracks implementation progress and journeys. These documents:
- Record what has been built vs specified
- Capture implementation decisions and trade-offs
- Document dialectical journeys through problems
- Track progress through phases
- Maintain alignment between specification and implementation

Development documentation can be messier, showing work-in-progress and recording dead ends that informed better approaches.

### Cross-Referencing

When development docs need to reference specifications, link to `/docs` rather than duplicating content. When public docs need to acknowledge implementation status, use priority markers rather than detailed progress reports.

## Document Structure Guidelines

### Essential Sections

Every major document should include:

1. **Opening Purpose Statement**: One paragraph explaining why this document exists and what it enables for user sovereignty
2. **Core Content**: The main substance, written as prose with embedded reasoning
3. **Formation Notes**: Strategic placement to connect technical content to user authorship
4. **Implementation Priorities** (when applicable): P0-Foundation through P3-Enhanced markers
5. **Cross-References**: Links to related documents using the numbered structure

### Avoiding Common Pitfalls

**Don't**:
- Write mechanical lists without explanation
- Add diagrams that don't genuinely clarify
- Create tables except for true tabular data
- Separate reasoning from description
- Use generic AI/ML terminology without grounding in formation

**Do**:
- Write flowing prose that carries reasoning
- Use formatting to enhance, not replace, clear writing
- Embed Formation Notes at moments of connection
- Ground all technical terms in user authorship
- Show thinking, not just conclusions

## The Numbered Documentation Structure

Documents are numbered to enforce proper reading order:

- **00-04**: Foundation (Vision through Implementation)
- **05-09**: Core Specifications (MVP through API Contracts)
- **10-14**: Patterns and Practices (Design through Development)
- **15**: Meta-documentation (This guide)

Numbers create a suggested path while allowing selective reading. A developer implementing a feature might read 01-VISION for context, jump to 07-ENTITY-RELATIONSHIP for schema, then 10-DESIGN-PATTERNS for implementation patterns.

## Writing for Different Audiences

### For Users

Emphasize what they can do and how it preserves their intellectual sovereignty. Avoid implementation details. Focus on capability and agency.

Example: "Your research questions shape what documents mean within your journey. The same paper that seems irrelevant to a broad query becomes essential when viewed through your specific theoretical lens."

### For Developers

Provide precise technical detail while maintaining connection to purpose. Include code examples and specific patterns.

Example: "Implement composite primary keys (UserId, Id) using UUIDv7 via Guid.CreateVersion7() for partition enforcement and temporal ordering. This ensures queries naturally scope to user partitions while maintaining temporal sequence without external sequence management."

### For Contributors

Explain both philosophy and mechanism. Show how architectural decisions embody philosophical commitments.

Example: "We reject the Repository pattern not from ignorance but from recognition that PostgreSQL IS our domain model. The schema embodies business rules through constraints. To abstract it would be to deny its participation in domain modeling."

## Maintaining Documentation Quality

### Before Writing

1. Read VISION.md to ground yourself in purpose
2. Check if similar content exists elsewhere
3. Identify your primary audience
4. Determine where this fits in the numbered structure

### While Writing

1. Start with purpose, not features
2. Embed reasoning in prose
3. Add Formation Notes at connection points
4. Use concrete examples over abstract descriptions
5. Link technical decisions to user sovereignty

### After Writing

1. Verify all cross-references work
2. Ensure Formation Notes connect to purpose
3. Check that technical terms are explained
4. Confirm the document strengthens the vision
5. Add appropriate priority markers

## Recursive Review Process

After making documentation updates, perform this recursive review:

### Terminology Consistency Check
- [ ] User-facing terms are explained when technical terms are used
- [ ] Same concepts use same terms throughout
- [ ] Technical jargon includes accessible explanation
- [ ] Domain terms link to their definitions

### Formation Thread Verification
- [ ] Every document connects back to formation through authorship
- [ ] Technical features show how they enable user sovereignty
- [ ] Constraints explain what they prevent (AI overreach)
- [ ] Architecture supports, never replaces, user understanding

### Cross-Reference Integrity
- [ ] All internal links use the numbered structure
- [ ] Links point to correct sections
- [ ] No broken references
- [ ] Bidirectional links where appropriate

### Priority Coherence
- [ ] P0 items have no dependencies
- [ ] P1 items only depend on P0
- [ ] P2 items only depend on P0 and P1
- [ ] P3 items can depend on any lower priority

### Vision Alignment
- [ ] Each document strengthens rather than obscures the core vision
- [ ] Technical complexity doesn't hide user purpose
- [ ] Implementation details serve formation through authorship
- [ ] Nothing contradicts intellectual sovereignty

## Common Documentation Patterns

### Introducing a New Concept

1. Start with what problem it solves for users
2. Explain the concept in user terms
3. Provide technical implementation detail
4. Add Formation Note connecting to sovereignty
5. Show example in context

### Documenting a Technical Decision

1. State the decision clearly
2. Explain the reasoning that led to it
3. Show what alternatives were considered
4. Connect to formation through authorship
5. Acknowledge any trade-offs

### Creating a Process Description

1. Begin with what formation it enables
2. Describe user journey through the process
3. Detail technical implementation
4. Show how it maintains sovereignty
5. Provide concrete example

## Living Documentation

Documentation for Veritheia is living documentation—it evolves with understanding while maintaining philosophical coherence. When you discover better ways to explain concepts, update the documentation. When implementation reveals new insights, reflect them in specifications. When user feedback clarifies purpose, strengthen the vision.

The documentation is never "done" but rather continuously refined to better serve its purpose: ensuring that everyone who encounters Veritheia understands that it enables formation through authorship, where users develop understanding through structured engagement with their documents, guided by their questions, shaped by their framework.

## Final Reminder

Every word you write in Veritheia's documentation should serve the principle that users author their own understanding. If a sentence doesn't ultimately connect to user sovereignty, intellectual formation, or epistemic infrastructure, it probably doesn't belong.

The documentation isn't just describing software; it's preserving the possibility of genuine user understanding in an age of AI-generated content. Write accordingly.

---

*This guide itself demonstrates the principles it espouses: prose with embedded reasoning, Formation Notes at key moments, and constant connection back to the purpose of enabling formation through authorship.*