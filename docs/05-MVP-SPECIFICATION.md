# Veritheia Core Specification

## 1. Overview

Veritheia is formative technology - epistemic infrastructure that makes formation scalable in the midst of information overload. The system provides journey projection spaces where documents are transformed according to user-defined intellectual frameworks, enabling formation through authorship. Users develop intellectual capacity through engagement with projected documents, not through consumption of AI-generated outputs.

> **Formation Note: The Neurosymbolic Transcendence** - The system demonstrates the revolutionary capability where users author their own symbolic systems through natural language. When a researcher writes "Papers are relevant if they provide empirical evidence," or a teacher writes "Good essays use sensory details," these natural language statements become the symbolic rules governing document processing. No programming required—the user's words ARE the symbolic system.

## 2. Core Formation Patterns

**Note**: These are core formation patterns - the same infrastructure supports any formative journey that meets the architecture's authorship constraints.

The system supports formative journeys - intellectual development through engagement with documents. These patterns demonstrate how formation scales through different types of authorship:

### 2.1 Research Formation Journey: Literature Review

**FORMATIVE GOAL**: Dr. Sarah develops research formation - the accumulated scholarly capacity to conduct systematic literature reviews. She doesn't receive AI-generated summaries but authors her own understanding through engagement with projected documents.

**CONCRETE USER STORY**: Dr. Sarah has 3,247 papers but can manually engage with only ~200. Through Veritheia, she develops the intellectual capacity to synthesize insights from the full corpus through her own authorship.

**PRECISE PROCESS**:
1. **Framework Definition** - User authors their symbolic system in natural language:
   - Research Questions: "How are LLMs being utilized for threat detection?"
   - Term Definitions: "Contextualized AI means AI systems utilizing proprietary, domain-specific knowledge"
   - Assessment Criteria: Relevance threshold 0.7, contribution scoring rubric
   - Theoretical Orientation: Post-industrial computing perspective

2. **Document Projection** - System mechanically applies user's symbolic framework:
   - **Segmentation**: Split according to semantic boundaries relevant to user's questions
   - **Embedding**: Generate vectors with user's vocabulary as context (transcended: user's words shape the vector space)
   - **Assessment**: Neural component interprets user's natural language rules to measure each segment

3. **Formation Through Authorship** - User develops scholarly capacity through:
   - Authoring inclusion/exclusion decisions (not accepting AI selections)
   - Writing synthesis that connects patterns across documents
   - Evolving research questions based on corpus engagement
   - Building personal theoretical framework through document encounter

4. **Formation Accumulation** - System captures intellectual development:
   - Decision reasoning that shows evolving judgment
   - Framework refinements that demonstrate deepening understanding
   - Insights authored through engagement (not AI-generated)
   - Accumulated capacity for future scholarly work

### 2.2 Pedagogical Formation Journey: Educational Assessment

**FORMATIVE GOAL**: Ms. Priya develops pedagogical formation - the accumulated capacity to create meaningful assignments and evaluate student growth. Students develop authentic voice through constrained composition exercises. Both teacher and students author their understanding through engagement.

**CONCRETE USER STORY**: Ms. Priya needs to evaluate 30 student essays but can only provide detailed individual feedback to ~10. Through Veritheia, she develops the capacity to support all students' formation while maintaining authentic assessment.

**PRECISE PROCESS**:
1. **Framework Definition** - Teacher defines:
   - Learning Objectives: "Student demonstrates descriptive language using sensory details"
   - Assessment Rubric: 4 points = proper length + grade-level vocabulary + 3+ sensory details
   - Safety Constraints: School-appropriate vocabulary, no inappropriate topics
   - Evaluation Criteria: 50-100 words, clear topic focus

2. **Assignment Projection** - System projects content through teacher framework:
   - **Template Processing**: Transform assignment templates through pedagogical constraints
   - **Rubric Formalization**: Project teacher criteria into assessment framework
   - **Safety Integration**: Multi-stage validation within boundary constraints

3. **Assessment Execution** - System evaluates student responses:
   - **Edge Validation**: Local LLM checks length, vocabulary, topic relevance
   - **Staged Scoring**: Sequential evaluation against rubric components
   - **Boundary Enforcement**: Filter inappropriate content, off-topic responses
   - **Teacher Review**: Flag edge cases for human review

4. **Dual Formation Process** - Both teacher and students develop:
   - **Teacher Formation**: Pedagogical capacity through pattern recognition in student growth
   - **Student Formation**: Authentic voice development through constrained composition
   - **Mutual Development**: Teacher framework evolves as students demonstrate new capabilities

## 3. Abstraction Level Hierarchy

### 3.1 HARD-CODED INFRASTRUCTURE (Cannot be changed by users)

**Level 0: Partition Architecture**
- Every query MUST begin with UserId
- Composite primary keys: (UserId, Id) for all user-owned entities
- User data isolation enforced at database level
- No cross-user queries possible

**Level 1: Journey Projection Spaces**
- Documents stored once, projected differently per journey
- Journey-specific segmentation, embedding, assessment
- Formation tracking as intellectual development
- Process execution within journey boundaries

**Level 2: Process Engine Framework**
```csharp
// IMMUTABLE INTERFACE (Pattern analogue, not DDD implementation)
public interface IAnalyticalProcess
{
    string ProcessType { get; }
    Task<ProcessResult> ExecuteAsync(ProcessContext context);
    InputDefinition GetInputDefinition();
}
```

## 4. System Architecture

The system follows a composable architectural pattern where components can be combined in different configurations to serve different deployment scenarios. This pattern enables clean separation of concerns while supporting flexible composition of functionality.

### 4.1 Composable Components

**ApiService Component**: The core business logic component provides the application programming interface for all system operations. This component contains all business logic, data access patterns, and domain services without any presentation or transport concerns. It serves as the foundation that can be composed with different interface components.

**Important**: The "API" in ApiService refers to **Application Programming Interface**, not HTTP REST API. This component is a pure business logic library that provides programming interfaces for other components to consume through direct method calls.

**Web Component**: The user interface component provides the Blazor Server presentation interface. This component imports the ApiService component and calls its programming interface directly. It handles user interaction, authentication, and session management while delegating all business operations to the ApiService component.

**ApiGateway Component**: The public interface component provides HTTP API endpoints for external system integration. This component imports the ApiService component and exposes its programming interface through HTTP protocols. It serves as the bridge between external systems and the core business logic.

**MCPGateway Component**: The AI agent interface component provides Model Context Protocol endpoints for AI agent integration. This component imports the ApiService component and exposes its programming interface through MCP protocols. It enables AI agents to access system functionality through standardized MCP interfaces.

### 4.2 Composition Patterns

Components communicate through direct method calls within the same process. The ApiService component defines the programming interface that other components consume. This pattern eliminates network overhead while maintaining clean architectural boundaries.

Different deployment scenarios can compose these components in various ways. A simple deployment might combine Web and ApiService components. A public API deployment might combine ApiGateway and ApiService components. A full deployment might include all three components.

### 4.3 Extension Points

The composable architecture provides clear extension points for additional components. New interface components can be added that import the ApiService component. The ApiService component can be extended with additional business logic without affecting interface components. This pattern supports progressive enhancement and flexible deployment scenarios.

### 3.2 CONFIGURABLE FORMATIVE ABSTRACTIONS (User-definable within formative constraints)

**Level 3: Journey Frameworks** (User-defined schemas, evolve per journey)

*Example Framework Patterns* (Not fixed formats - illustrative structures):

Framework elements might include research questions, definitions, assessment criteria, theoretical orientations, learning objectives, rubrics, safety constraints - but the specific schema is user-defined and can evolve.

Projection rules might include segmentation strategies, embedding contexts, assessment prompts, evaluation stages - but these are configured per journey type and can change as the journey develops.

*Note*: These are configurable scaffolds that accept multiple schema evolutions per journey type, not required formats.

**Level 4: Formative Process Implementations** (Extensible, must support formation through authorship)

*Known Process Implementations*:

**SystematicScreeningProcess** - Implements the LLAssist methodology for systematic literature review. Enables processing thousands of documents rather than hundreds through dual-phase assessment: Phase 1 extracts key semantics (topics, entities, keywords) from abstracts; Phase 2 evaluates each document against user research questions through relevance assessment (discusses the topic) and contribution assessment (researches the topic directly). Generates scores (0-1 scale), binary decisions, and reasoning chains. Every document receives identical treatment to prevent selective processing bias. Formation occurs through user engagement with these measurements.

**ConstrainedCompositionProcess** - Embodies EdgePrompt methodology for pedagogical formation within lesson planning journeys.

*Extension Requirements*: New processes must enable formation through authorship within journey projection spaces

**Process Input/Output Patterns**: Processes accept researcher frameworks and produce assessments for user evaluation. SystematicScreeningProcess: researchers upload CSV datasets (title, abstract, authors, venue, keywords), define research questions, optionally provide screening questions. Process generates outputs with scores (0-1), binary decisions, and reasoning chains. Enables processing thousands of documents while maintaining user control through evaluation of measurements, not consumption of insights.

**Input/Output Formats**: CSV input requires columns: title, abstract, authors, publication_venue, keywords. Research questions as plain text, one per line. JSON output contains per-article: extracted semantics (topics, entities, keywords arrays), scores (relevance_score, contribution_score as floats 0.0-1.0), binary decisions (is_relevant, is_contributing), reasoning chains, must-read determination (logical OR of decisions).

**User Interface Patterns**: The main interface uses enterprise sidebar navigation organizing around journeys, not processes. Sidebar Navigation provides primary access to journeys, personas, and system functions. Main Content Area displays journey-specific interfaces, process results, and configuration screens. Journey Dashboard shows active journeys with persona context, process types, current states, and recent activity. Users create new journeys by selecting persona and process, then configure journey-specific frameworks.

SystematicScreeningProcess requires tabular interface for thousand-document result sets within journey context. Core table displays: Title | Authors | Venue | Relevance Score | Contribution Score | Must-Read | Actions. Filters enable viewing subsets (relevant only, contributing only, must-read only, by venue, by score thresholds). Row selection enables bulk actions. Detail view shows reasoning chains and extracted semantics. Progress interface shows processing status during batch execution. Formation interface enables iterative refinement of research questions based on result patterns.

```
Sidebar Navigation (Persistent across all pages):
┌─────────────────────────────────────────────────────────────────────────────┐
│ ┌─────────────┐                                                             │
│ │ VERITHEIA   │ Dr. Sarah Chen │ [Researcher ▼] │ [Settings] │ [Logout]     │
│ └─────────────┘                                                             │
├─────────────────────────────────────────────────────────────────────────────┤
│ │📊 Dashboard │ Main Content Area (varies by selection)                     │
│ │             │                                                             │
│ │🚀 Journeys  │ ┌─────────────────────────────────────────────────────────┐ │
│ │ • ML Sec... │ │ [Selected content displays here]                        │ │
│ │ • Research  │ │                                                         │ │
│ │ • Market    │ │ Dashboard: Journey overview and recent activity         │ │
│ │             │ │ Journey: Process interface and results                  │ │
│ │👤 Personas  │ │ Personas: Context management and vocabulary             │ │
│ │ • Researcher│ │ Documents: Upload and corpus management                 │ │
│ │ • Student   │ │ Settings: User preferences and configuration            │ │
│ │ • Entrepren.│ │                                                         │ │
│ │             │ │                                                         │ │
│ │📄 Documents │ │                                                         │ │
│ │             │ │                                                         │ │
│ │⚙️  Settings │ │                                                         │ │
│ │             │ │                                                         │ │
│ │❓ Help      │ │                                                         │ │
│ └─────────────┘ └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘

Journey Dashboard (When Dashboard selected in sidebar):
┌─────────────────────────────────────────────────────────────────────────────┐
│ │📊 Dashboard │ Active Journeys (3)                     [+ New Journey]     │
│ │             │                                                             │
│ │🚀 Journeys  │ 📊 ML Security Literature Review                           │
│ │ • ML Sec... │    Researcher persona │ SystematicScreeningProcess         │
│ │ • Research  │    2,576 docs → 324 must-read │ Processing (67%)           │
│ │ • Market    │    [Continue] [View Results] [Pause]                       │
│ │             │                                                             │
│ │👤 Personas  │ 🎓 Research Methods Course                                 │
│ │ • Researcher│    Student persona │ ConstrainedCompositionProcess         │
│ │ • Student   │    Essay framework │ Active │ Last: 1 day ago              │
│ │ • Entrepren.│    [Continue] [View Submissions] [Edit Framework]          │
│ │             │                                                             │
│ │📄 Documents │ 💼 Startup Market Analysis                                 │
│ │             │    Entrepreneur persona │ SystematicScreeningProcess       │
│ │⚙️  Settings │    847 docs → 89 must-read │ Completed: 3 days ago         │
│ │             │    [View Results] [Export] [Archive]                       │
│ │❓ Help      │                                                             │
│ └─────────────┘ Recent Activity                                            │
│                 • ML Security: 156 new assessments completed               │
│                 • Research Methods: 3 new student submissions              │
│                 • Market Analysis: Results exported to PDF                 │
└─────────────────────────────────────────────────────────────────────────────┘

Journey Detail View (When specific journey selected in sidebar):
┌─────────────────────────────────────────────────────────────────────────────┐
│ │📊 Dashboard │ ML Security Literature Review │ [⚙️ Configure] [📤 Export] │
│ │             │                                                             │
│ │🚀 Journeys  │ ┌─ Process Status ─────────────────────────────────────────┐ │
│ │ • ML Sec... │ │ SystematicScreeningProcess │ Processing (67%)             │ │
│ │ • Research  │ │ Article 1,724 of 2,576 │ 4.2h elapsed │ 2.8h remaining  │ │
│ │ • Market    │ │ [Pause] [Resume] [Cancel]                                │ │
│ │             │ └─────────────────────────────────────────────────────────┘ │
│ │👤 Personas  │                                                             │
│ │ • Researcher│ ┌─ Results Summary ────────────────────────────────────────┐ │
│ │ • Student   │ │ Total: 2,576 docs │ Processed: 1,724 │ Must-Read: 243   │ │
│ │ • Entrepren.│ │ [View All Results] [View Must-Read Only] [Export CSV]    │ │
│ │             │ └─────────────────────────────────────────────────────────┘ │
│ │📄 Documents │                                                             │
│ │             │ ┌─ Framework Configuration ────────────────────────────────┐ │
│ │⚙️  Settings │ │ Research Questions (4) │ Last updated: 2 hours ago      │ │
│ │             │ │ • How are LLMs being utilized for threat detection?      │ │
│ │❓ Help      │ │ • What are the main challenges in AI security?          │ │
│ └─────────────┘ │ [Edit Questions] [Add Question] [View Full Framework]    │ │
│                 └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘

Journey Dashboard (Main Interface - Journey-Centric Organization):
┌──────────────────────────────────────────────────────────────────────────────┐
│ Veritheia │ Dr. Sarah Chen │ [Researcher ▼] │ [Settings] │ [Help] │ [Logout] │
├──────────────────────────────────────────────────────────────────────────────┤
│ Active Journeys (3)                                         [+ New Journey]  │
├──────────────────────────────────────────────────────────────────────────────┤
│ 📊 ML Security Literature Review                                            │
│    Researcher persona │ SystematicScreeningProcess │ Processing (67%)        │
│    2,576 docs → 324 must-read │ Last activity: 2 hours ago                   │
│    [Continue] [View Results] [Pause Processing]                              │
├──────────────────────────────────────────────────────────────────────────────┤
│ 🎓 Research Methods Course                                                  │
│    Student persona │ ConstrainedCompositionProcess │ Active                  │
│    Essay assignment framework │ Last activity: 1 day ago                     │
│    [Continue] [View Submissions] [Edit Framework]                            │
├──────────────────────────────────────────────────────────────────────────────┤
│ 💼 Startup Market Analysis                                                  │
│    Entrepreneur persona │ SystematicScreeningProcess │ Completed             │
│    847 docs → 89 must-read │ Completed: 3 days ago                           │
│    [View Results] [Export] [Archive]                                         │
├──────────────────────────────────────────────────────────────────────────────┤
│ Recent Activity                                                              │
│ • ML Security: 156 new assessments completed                                 │
│ • Research Methods: 3 new student submissions                                │
│ • Market Analysis: Results exported to PDF                                   │
└──────────────────────────────────────────────────────────────────────────────┘

New Journey Creation (Process Selection Interface):
┌──────────────────────────────────────────────────────────────────────────────┐
│ Create New Journey                                               [Cancel][×] │
├──────────────────────────────────────────────────────────────────────────────┤
│ Journey Name: [Cybersecurity AI Applications Review____________]             │
│ Purpose: [Systematic review of AI/ML applications in cyber defense_______]   │
├──────────────────────────────────────────────────────────────────────────────┤
│ Select Persona:                                                              │
│ ● Researcher (Domain expertise, investigation methods)                       │
│ ○ Student (Academic vocabulary, learning-focused patterns)                   │
│ ○ Entrepreneur (Business terminology, market analysis approaches)            │
├──────────────────────────────────────────────────────────────────────────────┤
│ Select Process:                                                              │
│ ● SystematicScreeningProcess                                                 │
│   📋 Literature review with dual-phase assessment (relevance + contribution) │
│   📊 Handles thousands of documents, generates must-read determinations      │
│   📝 Requires: CSV datasets, research questions                             │
│                                                                              │
│ ○ ConstrainedCompositionProcess                                              │
│   ✍️ Pedagogical formation with structured writing frameworks               │
│   👥 Student response evaluation, assignment creation                       │
│   📝 Requires: Learning objectives, rubrics, safety constraints             │
│                                                                              │
│ ○ [Custom Process] (Extension capability)                                   │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                [Create Journey] [Cancel]     │
└──────────────────────────────────────────────────────────────────────────────┘

Systematic Screening Results (2,576 documents → 324 must-read)
┌─────────────────────────────────────────────────────────────────────────────┐
│ [Must-Read Only ▼] [Year: All ▼] [Venue: Scopus ▼] [Threshold≥0.7] [Export] │
├─────────────────────────────────────────────────────────────────────────────┤
│☐ Title                        │Authors   │RQ1│RQ2│RQ3│RQ4│Must│Actions     │
├─────────────────────────────────────────────────────────────────────────────┤
│☐ LLMs for Threat Detection... │Chen, L.  │0.9│0.8│0.2│0.1│ ✓  │[View][Tag] │
│☐ AI Security Vulnerabilities..│Smith, J. │0.7│0.9│0.3│0.6│ ✓  │[View][Tag] │
│☐ Adversarial ML in Cyber...   │Brown, M. │0.3│0.4│0.9│0.2│ ✓  │[View][Tag] │
├─────────────────────────────────────────────────────────────────────────────┤
│ Showing 1-50 of 324 must-read results                    [1][2]...[7][>]    │
└─────────────────────────────────────────────────────────────────────────────┘

Detail View (when row selected):
┌─────────────────────────────────────────────────────────────────────────────┐
│ AI Security in Critical Infrastructure Systems                              │
│ Authors: Smith, J., Davis, K.  │ Venue: IEEE Security & Privacy 2024        │
├─────────────────────────────────────────────────────────────────────────────┤
│ Relevance Assessment (0.9): Discusses strategic AI implementation in        │
│ cyber defense contexts as specified in RQ1.                                 │
│                                                                             │
│ Contribution Assessment (0.8): Directly researches AI deployment            │
│ strategies with empirical evaluation as specified in RQ2.                   │
├─────────────────────────────────────────────────────────────────────────────┤
│ Extracted Semantics: [AI deployment][risk mitigation][critical systems]     │
│ Must-Read: YES (meets both relevance and contribution thresholds)           │
└─────────────────────────────────────────────────────────────────────────────┘

Processing Status (during 10+ hour batch execution):
┌─────────────────────────────────────────────────────────────────────────────┐
│ Processing: Article 1,247 of 2,576 (48%) │ Elapsed: 4.2h │ Remaining: 4.8h  │
│ Current: "Machine Learning for Intrusion Detection Systems"                 │
│ [Pause] [Resume] [Cancel]                    Last saved: 2 minutes ago      │
└─────────────────────────────────────────────────────────────────────────────┘
```

## 4. MVP Technical Architecture

### 4.1 Database Schema with User Partitions

**CRITICAL: All user-owned entities use composite primary keys (UserId, Id)**

```sql
-- CORRECT partition-enforced schema
CREATE TABLE journeys (
    user_id UUID NOT NULL,
    id UUID NOT NULL,
    PRIMARY KEY (user_id, id),
    -- other fields...
);

CREATE TABLE journey_document_segments (
    user_id UUID NOT NULL,
    id UUID NOT NULL,
    journey_id UUID NOT NULL,
    PRIMARY KEY (user_id, id),
    FOREIGN KEY (user_id, journey_id) REFERENCES journeys(user_id, id)
);

-- ALL indexes start with user_id for partition locality
CREATE INDEX idx_segments_user_journey ON journey_document_segments(user_id, journey_id);
```

### 4.2 Journey Projection Implementation

**Core Tables for Both Journey Types**:
- `journey_frameworks` - User's theoretical/pedagogical framework
- `journey_document_segments` - Documents projected into journey space
- `journey_segment_assessments` - AI measurements within journey context
- `journey_formations` - Captured insights and understanding development

**Framework Storage Pattern**:
```sql
-- Same table structure supports both journey types
CREATE TABLE journey_frameworks (
    user_id UUID NOT NULL,
    journey_id UUID NOT NULL,
    journey_type VARCHAR(100) NOT NULL, -- 'literature_review' | 'educational_assessment'
    framework_elements JSONB NOT NULL,  -- RQs + definitions OR objectives + rubrics
    projection_rules JSONB NOT NULL,    -- How to segment/embed/assess documents
    PRIMARY KEY (user_id, journey_id)
);
```

### 4.3 Neurosymbolic Process Engine Implementation

The Process Engine implements neurosymbolic architecture, transcended—the critical differentiator from all legacy systems. Traditional symbolic AI requires programmers to encode rules in formal languages. Veritheia transcends this: users author symbolic systems in natural language ("Papers must provide empirical evidence"), and neural components interpret these natural language rules. The mechanical orchestration ensures these user-authored rules apply to ALL documents identically, regardless of corpus size.

**Execution Context** (Neurosymbolic-aware):
```csharp
public class ProcessContext
{
    public Guid UserId { get; set; }          // ALWAYS required for partition
    public Guid JourneyId { get; set; }       // Journey boundary enforcement
    public string NaturalLanguageFramework { get; set; } // User's authored symbolic system
    public ProcessInputs Inputs { get; set; } // Process-specific parameters
}
```

**Neurosymbolic Process Implementation Examples**:

*Research Formation Process* (Direct implementation of LLAssist methodology):
```csharp
public class SystematicScreeningProcess : IAnalyticalProcess
{
    private readonly ICognitiveService _cognitiveService;
    
    public async Task<ProcessResult> ExecuteAsync(ProcessContext context)
    {
        // User's natural language framework becomes the symbolic system:
        // "I'm investigating how LLMs enhance cybersecurity threat detection.
        //  By 'contextualized AI' I mean systems that leverage domain-specific expertise...
        //  I need papers that directly contribute to understanding this relationship..."
        
        var documents = await GetAllDocuments(context.UserId, context.JourneyId);
        
        // MECHANICAL ORCHESTRATION: Process ALL documents identically
        foreach (var document in documents)
        {
            // Neural semantic understanding of user's authored symbolic framework
            var assessment = await _cognitiveService.ProcessWithUserFramework(
                document.Content, 
                context.NaturalLanguageFramework
            );
            
            // Systematic storage - every document gets processed and stored
            await StoreAssessment(context.UserId, document.Id, assessment);
        }
        
        // Result: All documents processed through user's authored framework
        return new ProcessResult { ProcessedCount = documents.Count };
    }
}
```

*Pedagogical Formation Process* (Direct implementation of EdgePrompt methodology):
```csharp
public class ConstrainedCompositionProcess : IAnalyticalProcess
{
    private readonly ICognitiveService _cognitiveService;
    
    public async Task<ProcessResult> ExecuteAsync(ProcessContext context)
    {
        // Teacher's natural language framework becomes the symbolic system:
        // "My Grade 5 students need to develop descriptive writing using sensory details.
        //  I want them to write 50-100 words about familiar topics, using vocabulary
        //  appropriate for their age level. No inappropriate content or violence..."
        
        var studentResponses = context.Inputs.StudentResponses;
        
        // MECHANICAL ORCHESTRATION: Process ALL student responses identically  
        foreach (var response in studentResponses)
        {
            // Neural semantic understanding of teacher's authored symbolic framework
            var evaluation = await _cognitiveService.ProcessWithUserFramework(
                response.Content, 
                context.NaturalLanguageFramework
            );
            
            // Systematic storage - every student gets identical evaluation process
            await StoreEvaluation(context.UserId, response.Id, evaluation);
        }
        
        // Result: All responses processed through teacher's authored framework
        return new ProcessResult { ProcessedCount = studentResponses.Count };
    }
}
```

## 5. Formative Success Criteria

### 5.1 Research Formation Success
- User develops capacity to engage with large document corpora (scales beyond manual limits)
- Authors synthesis through engagement with projected documents (not AI-generated summaries)
- Evolves theoretical framework through corpus encounter (formation through authorship)
- Builds accumulated scholarly capacity for future literature reviews
- Journey documentation shows intellectual development trajectory

### 5.2 Pedagogical Formation Success  
- Teacher develops capacity to support all student formation (scales beyond manual feedback limits)
- Students develop authentic voice through constrained composition exercises
- Both teacher and students author understanding through engagement
- Teacher's pedagogical framework evolves through pattern recognition in student growth
- Assessment emerges from authentic work (not standardized metrics)

### 5.3 Formative Architecture Success
- Zero cross-user data leakage (intellectual sovereignty protected)
- Journey projection spaces maintain formative integrity
- All processes support formation through authorship (no generic outputs)
- System scales formation despite information overload
- Extensions must serve intellectual development (cannot be generic document processing)

### 5.4 Performance Success
- Performance requirements to be determined based on actual implementation and user testing
- Success criteria will emerge from real usage patterns, not predetermined assumptions

## 6. Implementation Notes

**What is Hard-Coded**: User partition architecture, journey projection framework, process engine interface, database schema with composite keys

**What is Configurable**: Journey frameworks for different types of formation, projection rules for intellectual development, process implementations that support authorship within specific domains

**Extension Path**: New formative journey types must enable formation through authorship - users must develop intellectual capacity through engagement, not receive AI-generated outputs

The MVP embodies LLAssist and EdgePrompt as concrete implementations within the Veritheia platform. Both are proto-Veritheia systems that demonstrate formative journey patterns - LLAssist for research formation through literature engagement, EdgePrompt for pedagogical formation through assessment cycles. Veritheia provides the unified infrastructure that makes both possible while maintaining intellectual sovereignty through journey projection spaces and strict user partition boundaries.