# Foundational Papers

This directory contains the research papers that inform Veritheia's architecture and methodology.

## Papers

### 1. LLAssist: Simple Tools for Automating Literature Review Using Large Language Models
**File**: `2407.13993v3.pdf`  
**Authors**: Christoforus Yoga Haryanto  
**Year**: 2024  
**Reference**: arXiv:2407.13993v3 [cs.DL]  
**DOI**: https://doi.org/10.48550/arXiv.2407.13993  
**Note**: 10 pages, 3 figures, 1 table, presented at the 51st International Conference on Computers and Industrial Engineering (CIE51), 11 Dec 2024  
**Subjects**: Digital Libraries (cs.DL); Artificial Intelligence (cs.AI)  
**Latest version**: v3, 20 Dec 2024

This paper introduces LLAssist, an open-source tool designed to streamline literature reviews by leveraging Large Language Models and NLP techniques. The methodologies for systematic screening and constrained composition in Veritheia are directly derived from the patterns established in LLAssist.

### 2. EdgePrompt: Engineering Guardrail Techniques for Offline LLMs in K-12 Educational Settings
**File**: `3701716.3717810.pdf`  
**Authors**: Riza Alaudin Syah, Christoforus Yoga Haryanto, Emily Lomempow, Krishna Malik, Irvan Putra  
**Year**: 2025  
**Reference**: WWW '25 Companion Proceedings, Pages 1635-1638  
**DOI**: https://doi.org/10.1145/3701716.3717810  
**Published**: 23 May 2025

This paper establishes the epistemic sovereignty principles and offline-first architecture that underpin Veritheia's design. The guardrail techniques and local processing requirements directly influence the system's use of local LLMs and rejection of cloud-first approaches.

## Relevance to Veritheia

These papers collectively establish:

1. **Epistemic Sovereignty**: The principle that intellectual work should remain under the control of its author
2. **Systematic Methodology**: Structured approaches to document analysis and knowledge synthesis
3. **Local Processing**: Preference for local LLMs and offline-capable architectures
4. **Grounded Reasoning**: All insights must be traceable to source material

The methodologies from these papers are implemented in:
- `/veritheia.Data/Processes/BasicSystematicScreeningProcess.cs` - Document screening methodology
- `/veritheia.Data/Processes/BasicConstrainedCompositionProcess.cs` - Structured document generation
- `/veritheia.Data/Services/LocalLLMAdapter.cs` - Local LLM integration for sovereignty