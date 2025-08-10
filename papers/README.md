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

### 3. Contextualized AI for Cyber Defense: An Automated Survey Using LLMs
**File**: `2409.13524v1.pdf`  
**Authors**: Christoforus Yoga Haryanto, Anne Maria Elvira, Trung Duc Nguyen, Minh Hieu Vu, Yoshiano Hartanto, Emily Lomempow, Arathi Arakala  
**Year**: 2024  
**Reference**: 2024 17th International Conference on Security of Information and Networks (SIN), 02-04 December 2024  
**DOI**: [10.1109/SIN63213.2024.10871242](https://doi.org/10.1109/SIN63213.2024.10871242)  
**Also**: arXiv:2409.13524v1 [cs.CR] - [https://doi.org/10.48550/arXiv.2409.13524](https://doi.org/10.48550/arXiv.2409.13524)  
**Note**: 8 pages, 2 figures, 4 tables

This paper demonstrates two LLM-assisted literature survey methodologies that directly inform Veritheia's approach to document analysis. The paper's Method B (using Gemma 2:9b for screening and Claude 3.5 Sonnet for full-text analysis) provides the theoretical foundation for Veritheia's systematic screening process. The emphasis on contextualized AI—systems that utilize proprietary and domain-specific knowledge—aligns with Veritheia's journey-specific document projections.

## Relevance to Veritheia

These three papers form the theoretical and methodological foundation of Veritheia:

### Core Principles Established

1. **Epistemic Sovereignty** (EdgePrompt): The principle that intellectual work should remain under the control of its author, implemented through local-first LLM processing and rejection of cloud dependencies.

2. **Systematic Methodology** (LLAssist): Structured approaches to document analysis and knowledge synthesis, with specific scoring thresholds (0.7) for relevance determination.

3. **Contextualized Intelligence** (Cyber Defense paper): AI systems that access and utilize proprietary, domain-specific knowledge—exactly what Veritheia achieves through journey-specific document projections.

4. **Grounded Reasoning**: All insights must be traceable to source material, preventing hallucination and ensuring epistemic integrity.

### Direct Implementation Mappings

The methodologies from these papers are directly implemented in Veritheia:

- **LLAssist → BasicSystematicScreeningProcess.cs**: The paper's relevance scoring (0-1 scale with 0.7 threshold) is implemented in the systematic screening process.

- **Cyber Defense Method B → Document Analysis Pipeline**: The two-stage approach (Gemma for screening + Claude for analysis) inspired Veritheia's separation of screening and composition processes.

- **EdgePrompt → LocalLLMAdapter.cs**: The guardrail techniques and offline-first requirements directly shaped the local LLM integration design.

- **Journey Projections**: The concept of "contextualized AI" from the Cyber Defense paper manifests as journey-specific document projections where documents gain meaning through their journey context.

### Methodological Contributions

1. **LLM-Assisted Review**: Both LLAssist and the Cyber Defense paper demonstrate effective LLM-assisted literature review, which Veritheia extends to general document analysis.

2. **Prompt Engineering**: The Cyber Defense paper's structured prompting techniques (clear role assignment, output format specification) are used throughout Veritheia's analytical processes.

3. **Human-AI Collaboration**: All three papers emphasize human oversight and expertise remaining critical, reflected in Veritheia's design as an augmentation tool, not a replacement.

4. **Trust Through Transparency**: The papers' emphasis on explainability and verification directly influences Veritheia's journal system that tracks all analytical decisions.