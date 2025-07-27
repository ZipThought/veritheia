# API Contracts

This document defines the interfaces and contracts that form the foundation of Veritheia's architecture. These contracts ensure consistent integration between components while maintaining the principle that users author their own understanding.

## Core Interfaces

### IAnalyticalProcess

The fundamental interface that all processes must implement:

```csharp
public interface IAnalyticalProcess
{
    ProcessDefinition GetDefinition();
    Task<ProcessResult> ExecuteAsync(ProcessContext context);
    IProcessResultRenderer GetResultRenderer();
}
```

### IProcessResultRenderer

Defines how process results are displayed to users:

```csharp
public interface IProcessResultRenderer
{
    Type GetComponentType();
    object PrepareViewModel(ProcessResult result);
}
```

### ICognitiveAdapter

Abstracts LLM operations for consistent cognitive system access:

```csharp
public interface ICognitiveAdapter
{
    Task<EmbeddingResult> CreateEmbeddingsAsync(string text);
    Task<string> GenerateTextAsync(string prompt, GenerationParameters parameters);
}
```

### IKnowledgeRepository

Provides unified data access respecting scope boundaries:

```csharp
public interface IKnowledgeRepository
{
    Task<IEnumerable<Document>> GetDocumentsInScopeAsync(Guid? scopeId);
    Task<Document> GetDocumentAsync(Guid documentId);
    Task<IEnumerable<ProcessedContent>> GetEmbeddingsAsync(Guid documentId);
    Task<KnowledgeScope> GetScopeAsync(Guid scopeId);
    Task SaveProcessResultAsync(ProcessResult result);
}
```

## Platform Service Interfaces

### IPlatformServices

Aggregates all platform services available to processes:

```csharp
public interface IPlatformServices
{
    IDocumentProcessor DocumentProcessor { get; }
    ITextExtractor TextExtractor { get; }
    IEmbeddingGenerator EmbeddingGenerator { get; }
    IMetadataExtractor MetadataExtractor { get; }
    IDocumentChunker DocumentChunker { get; }
}
```

#### IPlatformServices Design Note

This interface intentionally aggregates all platform services as a facade pattern. While this could be seen as violating ISP, it:
- Simplifies process implementations (single injection point)
- Guarantees service availability to all processes
- Maintains backward compatibility as services evolve

Processes only use the services they need from the facade.

### IDocumentProcessor

Manages document processing pipeline:

```csharp
public interface IDocumentProcessor
{
    Task<ProcessedDocument> ProcessDocumentAsync(Guid documentId);
    Task<bool> IsProcessedAsync(Guid documentId);
}
```

### ITextExtractor

Extracts text from various document formats:

```csharp
public interface ITextExtractor
{
    Task<string> ExtractTextAsync(Stream documentStream, string mimeType);
    bool SupportsFormat(string mimeType);
}
```

### IEmbeddingGenerator

Creates vector embeddings using the cognitive system:

```csharp
public interface IEmbeddingGenerator
{
    Task<float[]> GenerateEmbeddingAsync(string text);
    Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts);
    int GetEmbeddingDimension();
}
```

### IMetadataExtractor

Extracts structured metadata from documents:

```csharp
public interface IMetadataExtractor
{
    Task<DocumentMetadata> ExtractMetadataAsync(Stream documentStream, string mimeType);
}
```

### IDocumentChunker

Splits documents into semantic chunks:

```csharp
public interface IDocumentChunker
{
    Task<List<DocumentChunk>> ChunkDocumentAsync(string text, ChunkingStrategy strategy);
}
```

## User and Journey Interfaces

### IUserService

Manages user accounts and profiles:

```csharp
public interface IUserService
{
    Task<User> CreateUserAsync(CreateUserRequest request);
    Task<User> GetUserAsync(Guid userId);
    Task<User> GetCurrentUserAsync();
    Task UpdateUserAsync(Guid userId, UpdateUserRequest request);
    Task<IEnumerable<ProcessCapability>> GetUserCapabilitiesAsync(Guid userId);
}
```

### IJourneyService

Manages user journeys through processes:

```csharp
public interface IJourneyService
{
    Task<Journey> CreateJourneyAsync(CreateJourneyRequest request);
    Task<Journey> GetJourneyAsync(Guid journeyId);
    Task<IEnumerable<Journey>> GetUserJourneysAsync(Guid userId, JourneyFilter filter = null);
    Task UpdateJourneyStateAsync(Guid journeyId, JourneyState newState);
    Task<JourneyContext> GetJourneyContextAsync(Guid journeyId);
}
```

### IJournalService

Manages narrative records within journeys:

```csharp
public interface IJournalService
{
    Task<Journal> CreateJournalAsync(Guid journeyId, JournalType type);
    Task<JournalEntry> AddEntryAsync(Guid journalId, string content, EntryMetadata metadata = null);
    Task<IEnumerable<Journal>> GetJourneyJournalsAsync(Guid journeyId);
    Task<IEnumerable<JournalEntry>> GetRecentEntriesAsync(Guid journeyId, int count, JournalType? type = null);
    Task<string> AssembleContextAsync(Guid journeyId, ContextRequest request);
}
```

### IPersonaService

Tracks evolving user intellectual patterns:

```csharp
public interface IPersonaService
{
    Task<Persona> GetPersonaAsync(Guid userId);
    Task UpdateVocabularyAsync(Guid userId, IEnumerable<string> terms);
    Task RecordPatternAsync(Guid userId, InquiryPattern pattern);
    Task<PersonaContext> GetPersonaContextAsync(Guid userId, string domain = null);
}
```

## Process Support Interfaces

### IProcessRegistry

Manages process discovery and metadata:

```csharp
public interface IProcessRegistry
{
    Task<IEnumerable<ProcessDefinition>> GetAvailableProcessesAsync();
    Task<ProcessDefinition> GetProcessDefinitionAsync(string processType);
    Task<IAnalyticalProcess> CreateProcessInstanceAsync(string processType);
}
```

### IProcessEngine

Orchestrates process execution:

```csharp
public interface IProcessEngine
{
    Task<Guid> ExecuteAsync(string processType, Dictionary<string, object> inputs, Guid userId);
    Task<ProcessExecution> GetExecutionAsync(Guid executionId);
    Task<ProcessResult> GetResultAsync(Guid executionId);
}
```

### IAssignmentService

Manages educational assignments (for Guided Composition):

```csharp
public interface IAssignmentService
{
    Task<Assignment> CreateAssignmentAsync(Assignment assignment);
    Task<Assignment> GetAssignmentAsync(Guid assignmentId);
    Task<IEnumerable<Assignment>> GetAssignmentsForUserAsync(Guid userId);
    Task<StudentSubmission> SubmitResponseAsync(Guid assignmentId, string response, Guid studentId);
}
```

## Data Transfer Objects

### ProcessDefinition

Describes a process and its requirements:

```csharp
public class ProcessDefinition
{
    public string ProcessType { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ProcessCategory Category { get; set; }
    public ProcessTriggerType TriggerType { get; set; }
    public InputDefinition Inputs { get; set; }
}
```

### ProcessContext

Carries execution context through a process:

```csharp
public class ProcessContext
{
    public Guid ExecutionId { get; set; }
    public Guid UserId { get; set; }
    public Guid JourneyId { get; set; }
    public Guid? ScopeId { get; set; }
    public Dictionary<string, object> Inputs { get; set; }
    public IServiceProvider Services { get; set; }
    public JourneyContext JourneyContext { get; set; }
    
    public T GetInput<T>(string key);
    public T GetService<T>();
}
```

### ProcessResult

Encapsulates process execution results:

```csharp
public class ProcessResult
{
    public string ProcessType { get; set; }
    public object Data { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public DateTime ExecutedAt { get; set; }
    
    public T GetData<T>();
}
```

### InputDefinition

Fluent API for defining process inputs:

```csharp
public class InputDefinition
{
    public InputDefinition AddTextArea(string name, string description, bool required = true);
    public InputDefinition AddTextInput(string name, string description, bool required = true);
    public InputDefinition AddDropdown(string name, string description, string[] options, bool required = true);
    public InputDefinition AddScopeSelector(string name, string description, bool required = false);
    public InputDefinition AddDocumentSelector(string name, string description, bool required = true);
    public InputDefinition AddMultiSelect(string name, string description, string[] options, bool required = true);
}
```

## Base Types

### BaseEntity

Common properties for all entities:

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

## User Model Types

### User

Core user entity:

```csharp
public class User : BaseEntity
{
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public Guid PersonaId { get; set; }
    public DateTime LastActiveAt { get; set; }
}
```

### Journey

Represents a user's engagement with a process:

```csharp
public class Journey : BaseEntity
{
    public Guid UserId { get; set; }
    public string ProcessType { get; set; }
    public string Purpose { get; set; }
    public JourneyState State { get; set; }
    public Dictionary<string, object> Context { get; set; }
}
```

### Journal

Narrative record within a journey:

```csharp
public class Journal : BaseEntity
{
    public Guid JourneyId { get; set; }
    public JournalType Type { get; set; }
    public bool IsShareable { get; set; }
}
```

### JournalEntry

Individual entry in a journal:

```csharp
public class JournalEntry : BaseEntity
{
    public Guid JournalId { get; set; }
    public string Content { get; set; }
    public EntrySignificance Significance { get; set; }
    public List<string> Tags { get; set; }
}
```

## Enumerations

### ProcessCategory

Categorizes processes by their intellectual purpose:

```csharp
public enum ProcessCategory
{
    Methodological,  // Research methodologies
    Developmental,   // Skill progression
    Analytical,      // Pattern discovery
    Compositional,   // Creative work
    Reflective       // Contemplative practices
}
```

### ProcessTriggerType

Defines how processes are initiated:

```csharp
public enum ProcessTriggerType
{
    Manual,        // User-initiated
    Automatic,     // Event-triggered
    Scheduled      // Time-based
}
```

### ChunkingStrategy

Strategies for document chunking:

```csharp
public enum ChunkingStrategy
{
    Semantic,      // Preserve meaning units
    FixedSize,     // Consistent token count
    Paragraph,     // Natural breaks
    Sliding        // Overlapping windows
}
```

### JournalType

Types of journals within a journey:

```csharp
public enum JournalType
{
    Research,      // Findings and discoveries
    Method,        // Approaches and techniques
    Decision,      // Choices and rationales
    Reflection     // Insights and understanding
}
```

### JourneyState

Current state of a journey:

```csharp
public enum JourneyState
{
    Active,        // In progress
    Paused,        // Temporarily stopped
    Completed,     // Finished successfully
    Abandoned      // Discontinued
}
```

### EntrySignificance

Importance level for journal entries:

```csharp
public enum EntrySignificance
{
    Routine,       // Regular progress
    Notable,       // Worth highlighting
    Critical,      // Key decision or insight
    Milestone      // Major achievement
}
```

## Extension Contracts

### Service Registration

Extensions register services using this pattern:

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProcess<TProcess>(this IServiceCollection services)
        where TProcess : class, IAnalyticalProcess;
}
```

### Data Model Requirements

Extension entities must:
- Inherit from `BaseEntity`
- Relate to `ProcessExecution` when storing process-specific data
- Use proper Entity Framework relationships

### UI Component Requirements

Extension components must:
- Inherit from appropriate base components
- Accept view models from result renderers
- Follow platform UI patterns

## HTTP API Contracts

### Process Endpoints

```
GET  /api/processes
     Returns: ProcessDefinition[]

GET  /api/processes/{processType}
     Returns: ProcessDefinition

POST /api/processes/{processType}/execute
     Body: { inputs: { ... } }
     Returns: { executionId: Guid }

GET  /api/executions/{executionId}
     Returns: ProcessExecution

GET  /api/executions/{executionId}/result
     Returns: ProcessResult
```

### Knowledge Endpoints

```
GET  /api/documents
     Query: scopeId={guid}
     Returns: Document[]

GET  /api/documents/{documentId}
     Returns: Document

POST /api/documents
     Body: multipart/form-data
     Returns: Document

GET  /api/scopes
     Returns: KnowledgeScope[]

POST /api/scopes
     Body: KnowledgeScope
     Returns: KnowledgeScope
```

### Search Endpoints

```
POST /api/search/keyword
     Body: { query: string, scopeId?: Guid }
     Returns: SearchResult[]

POST /api/search/semantic
     Body: { query: string, scopeId?: Guid, threshold?: number }
     Returns: SemanticSearchResult[]
```

## Response Formats

### Success Response

```json
{
  "success": true,
  "data": { ... },
  "metadata": {
    "timestamp": "2024-01-01T00:00:00Z",
    "version": "v1"
  }
}
```

### Error Response

```json
{
  "success": false,
  "error": {
    "code": "PROCESS_NOT_FOUND",
    "message": "Process type 'InvalidProcess' is not registered",
    "details": { ... }
  }
}
```

### Pagination Response

```json
{
  "success": true,
  "data": [ ... ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalItems": 145,
    "totalPages": 8
  }
}
```

## Versioning

All contracts follow semantic versioning:
- Breaking changes require major version increment
- New optional features allow minor version increment
- Bug fixes use patch version increment

Backward compatibility is maintained within major versions.