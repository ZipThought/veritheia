# Testing Strategy

## 1. Overview

This document specifies the testing methodology for Veritheia using epistemically rigorous test categorization. The strategy recognizes three fundamental test types based on their relationship to external dependencies and system boundaries: unit tests for pure logic, integration tests for component collaboration, and end-to-end tests for complete user workflows.

The categorization follows first principles rather than industry convention. A unit test exercises isolated logic without external state or side effects. An integration test verifies component collaboration across boundaries where correctness depends on external systems with intrinsic logic (like PostgreSQL with constraints and triggers). An end-to-end test validates complete user workflows through the full system stack, mocking only external services that are costly, unreliable, or uncontrollable.

> **Formation Note:** Testing preserves intellectual sovereignty even in test environments. Every test creates proper user partitions with UserId keys, ensuring tests validate the same sovereignty boundaries that protect formation in production. When tests create journeys, they use real user contexts. When they process documents, they respect partition isolation. This isn't just good testing—it's validation that the system mechanically enforces authorship boundaries at every level.

## Test Infrastructure (Phase 3 Decision)

### Database Testing Approach: PostgreSQL with Respawn

All tests use real PostgreSQL 17 with pgvector, using Respawn for fast isolation:

```csharp
public class DatabaseFixture : IAsyncLifetime
{
    private PostgreSqlContainer _container;
    private Respawner _respawner;
    
    public async Task InitializeAsync()
    {
        // Single container for all tests
        _container = new PostgreSqlBuilder()
            .WithImage("pgvector/pgvector:pg17")
            .Build();
        await _container.StartAsync();
        
        // Respawn for fast data reset (~50ms)
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            TablesToIgnore = new[] { "__EFMigrationsHistory" }
        });
    }
    
    public async Task ResetAsync()
    {
        // Fast reset between tests - data only, not schema
        await _respawner.ResetAsync(connection);
    }
}
```

**Rationale**:
- Journey projections require real PostgreSQL (vectors, JSONB, ranges)
- Respawn provides fast isolation without container overhead
- Can test transactions and raw SQL operations
- No false positives from in-memory database differences

### Test Organization

```
veritheia.Tests/
├── TestBase/              # Shared infrastructure
├── Phase1_Database/       # Schema and basic CRUD
├── Phase2_DomainModels/   # Value objects and enums
├── Phase3_Repositories/   # Repository patterns
└── Integration/           # Full stack tests
```

## Testing Philosophy

### Core Principles

1. **Journey Integrity**: Tests must verify that outputs remain tied to their generative journey
2. **User Attribution**: All results must be traceable to specific user actions and context
3. **Extension Isolation**: Process tests cannot depend on other processes
4. **Formation Verification**: Tests should confirm that system amplifies rather than replaces thinking

### What We Test

- **Behavior over Implementation**: Focus on what the system does, not how
- **Journey Context**: Verify context assembly and narrative coherence
- **Process Boundaries**: Ensure extensions respect platform limits
- **Formation Patterns**: Confirm outputs require user engagement

### What We Don't Test

- **LLM Output Content**: We test integration, not specific AI responses
- **Extension Internals**: Core tests don't verify extension logic
- **UI Implementation Details**: Focus on functionality, not presentation
- **Performance Optimizations**: Separate from functional tests

## Test Categories

### 1. Unit Tests

Unit tests exercise pure logic in complete isolation from external state or side effects. These tests validate the smallest functional components—individual methods, value objects, or domain logic—without touching databases, files, networks, or any external systems. If a test requires mocking complex dependencies, the code under test is likely too large or coupled to be considered a true unit.

The essence of unit testing lies in the stateless, deterministic nature of the code being tested, not in the presence or absence of mocks. A properly designed unit has no external dependencies to mock. When dependencies exist, they should be simple interfaces that can be satisfied with trivial stubs, not complex simulations of external system behavior.

#### Pure Domain Logic Tests

```csharp
// True unit test - pure logic, no external dependencies
[Fact]
public void Persona_AddConceptualTerm_IncreasesFrequency()
{
    // Arrange - pure object construction
    var persona = new Persona { ConceptualVocabulary = new Dictionary<string, int>() };
    
    // Act - pure method call
    persona.AddConceptualTerm("epistemic");
    persona.AddConceptualTerm("epistemic");
    
    // Assert - deterministic outcome
    Assert.Equal(2, persona.ConceptualVocabulary["epistemic"]);
}

// True unit test - value object validation
[Fact]
public void ProcessContext_ValidatesRequiredInputs_ReturnsFalseWhenMissing()
{
    // Arrange
    var context = new ProcessContext
    {
        Inputs = new Dictionary<string, object> { ["scope"] = "test" }
    };
    
    // Act
    var isValid = context.HasRequiredInputs(new[] { "researchQuestions", "scope" });
    
    // Assert
    Assert.False(isValid);
}

// True unit test - state transition logic
[Fact]
public void Journey_CanTransitionTo_ValidatesStateRules()
{
    // Arrange
    var journey = new Journey { State = JourneyState.Active };
    
    // Act & Assert - pure business logic
    Assert.True(journey.CanTransitionTo(JourneyState.Paused));
    Assert.True(journey.CanTransitionTo(JourneyState.Completed));
    Assert.False(journey.CanTransitionTo(JourneyState.Active)); // Already active
}

// True unit test - formation marker creation
[Fact]
public void FormationMarker_CreateInsightMarker_GeneratesCorrectStructure()
{
    // Arrange
    var journeyId = Guid.CreateVersion7();
    var segmentIds = new[] { Guid.CreateVersion7(), Guid.CreateVersion7() };
    
    // Act
    var marker = FormationMarker.CreateInsightMarker(
        journeyId, 
        "Key insight about distributed systems", 
        segmentIds,
        new Dictionary<string, object> { ["confidence"] = 0.85 }
    );
    
    // Assert
    Assert.Equal(journeyId, marker.JourneyId);
    Assert.Contains("distributed systems", marker.InsightDescription);
    Assert.Equal(2, marker.ContributingSegmentIds.Count);
    Assert.Equal(0.85, marker.Context["confidence"]);
}
```

#### Value Object Behavior Tests

```csharp
// True unit test - screening result logic
[Fact]
public void ScreeningResult_RequiresBothAssessments_ValidatesCorrectly()
{
    // Arrange & Act
    var result = new ScreeningResult
    {
        DocumentId = Guid.CreateVersion7(),
        IsRelevant = true,
        RelevanceScore = 0.8m,
        ContributesToRQ = false,
        ContributionScore = 0.3m
    };
    
    // Assert - pure logic validation
    Assert.True(result.IsRelevant);
    Assert.False(result.ContributesToRQ);
    Assert.True(result.RelevanceScore > result.ContributionScore);
    Assert.True(result.IsValid()); // Assuming validation method
}

// True unit test - input definition validation
[Fact]
public void InputDefinition_ValidateParameters_RejectsInvalidTypes()
{
    // Arrange
    var definition = new InputDefinition
    {
        RequiredInputs = new[] { "researchQuestions", "scope" },
        OptionalInputs = new[] { "definitions" },
        InputTypes = new Dictionary<string, Type>
        {
            ["researchQuestions"] = typeof(string),
            ["scope"] = typeof(Guid)
        }
    };
    
    var invalidInputs = new Dictionary<string, object>
    {
        ["researchQuestions"] = 123, // Wrong type
        ["scope"] = Guid.CreateVersion7()
    };
    
    // Act
    var isValid = definition.ValidateInputs(invalidInputs);
    
    // Assert
    Assert.False(isValid);
}
```

### 2. Integration Tests

Integration tests verify component collaboration across critical boundaries where correctness depends on external systems with intrinsic logic. The database is the primary non-mockable boundary in Veritheia because PostgreSQL enforces referential integrity, constraints, triggers, and provides specialized functionality (pgvector, JSONB operations) that cannot be adequately simulated by mocks or in-memory substitutes.

These tests use real PostgreSQL with the full schema applied through Entity Framework migrations. The database is not a dumb data store but an active participant that enforces business rules through constraints, provides vector similarity operations, and manages complex relationships with composite primary keys. Mocking such a system would create false confidence—tests might pass with a mock that allows invalid states the real database would reject.

Integration tests focus on verifying that application components work correctly with the real database's behavior, including edge cases that only emerge from actual constraint enforcement and the full feature set of PostgreSQL with pgvector.

#### Database Constraint Integration Tests

```csharp
// Integration test - real database with composite primary keys
[Fact]
public async Task CreateJourney_WithValidUserAndPersona_EnforcesPartitionConstraints()
{
    // Arrange - real database with full schema
    var user = new User 
    { 
        Id = Guid.CreateVersion7(), 
        Email = "researcher@example.com",
        DisplayName = "Test Researcher"
    };
    Context.Users.Add(user);
    
    var persona = new Persona 
    { 
        Id = Guid.CreateVersion7(), 
        UserId = user.Id, // Composite key enforcement
        Domain = "Researcher" 
    };
    Context.Personas.Add(persona);
    await Context.SaveChangesAsync();
    
    // Act - service uses real database with constraints
    var service = new JourneyService(Context);
    var journey = await service.CreateJourneyAsync(new CreateJourneyRequest
    {
        UserId = user.Id,
        PersonaId = persona.Id, // Must match partition
        ProcessType = "SystematicScreening",
        Purpose = "Review ML security literature"
    });
    
    // Assert - database enforced composite key relationships
    Assert.Equal(user.Id, journey.UserId);
    Assert.Equal(persona.Id, journey.PersonaId);
    
    // Verify constraint enforcement by querying with composite key
    var retrieved = await Context.Journeys
        .FirstOrDefaultAsync(j => j.UserId == user.Id && j.Id == journey.Id);
    Assert.NotNull(retrieved);
}

// Integration test - vector operations require real pgvector
[Fact]
public async Task StoreSearchVector_WithRealEmbedding_UsesPgvectorOperations()
{
    // Arrange - create journey document segment
    var user = CreateTestUser();
    var document = CreateTestDocument(user.Id);
    var segment = CreateTestSegment(user.Id, document.Id);
    
    Context.Users.Add(user);
    Context.Documents.Add(document);
    Context.JourneyDocumentSegments.Add(segment);
    await Context.SaveChangesAsync();
    
    // Act - store real vector embedding
    var searchIndex = new SearchIndex
    {
        Id = Guid.CreateVersion7(),
        UserId = user.Id, // Partition enforcement
        SegmentId = segment.Id,
        VectorModel = "text-embedding-3-large"
    };
    Context.SearchIndexes.Add(searchIndex);
    
    // Real pgvector operation - cannot be mocked
    var embedding = new float[1536];
    for (int i = 0; i < 1536; i++)
        embedding[i] = (float)Math.Sin(i * 0.01);
    
    var vector = new SearchVector1536
    {
        UserId = user.Id,
        IndexId = searchIndex.Id,
        Embedding = new Vector(embedding)
    };
    Context.SearchVectors1536.Add(vector);
    await Context.SaveChangesAsync();
    
    // Assert - verify pgvector similarity operations work
    var similar = await Context.SearchVectors1536
        .Where(v => v.UserId == user.Id)
        .OrderBy(v => v.Embedding.CosineDistance(new Vector(embedding)))
        .FirstOrDefaultAsync();
    
    Assert.NotNull(similar);
    Assert.Equal(vector.IndexId, similar.IndexId);
}
```

#### Process Integration Tests

```csharp
// Integration test - process with real database and mocked external services
[Fact]
public async Task SystematicScreening_ProcessesAllDocuments_WithRealDatabaseAndMockedLLM()
{
    // Arrange - real database setup
    var user = CreateTestUser();
    var journey = CreateTestJourney(user.Id);
    var documents = CreateTestDocuments(user.Id, count: 5);
    
    Context.Users.Add(user);
    Context.Journeys.Add(journey);
    Context.Documents.AddRange(documents);
    await Context.SaveChangesAsync();
    
    // Mock only external LLM service - not internal database operations
    var mockCognitiveAdapter = new Mock<ICognitiveAdapter>();
    mockCognitiveAdapter
        .Setup(x => x.GenerateTextAsync(It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync("The document is highly relevant to adversarial ML research.");
    
    var processContext = new ProcessContext
    {
        UserId = user.Id,
        JourneyId = journey.Id,
        Inputs = new Dictionary<string, object>
        {
            ["researchQuestions"] = "RQ1: What are adversarial attacks?",
            ["definitions"] = new Dictionary<string, string>()
        }
    };
    
    // Process uses real database context, mocked LLM
    var process = new SystematicScreeningProcess(Context, mockCognitiveAdapter.Object);
    
    // Act - process interacts with real database
    var result = await process.ExecuteAsync(processContext);
    
    // Assert - verify database state changes
    var screeningData = result.GetData<ScreeningProcessResult>();
    Assert.Equal(5, screeningData.Results.Count); // All docs processed
    
    // Verify actual database records created
    var journalEntries = await Context.JournalEntries
        .Where(e => e.UserId == user.Id)
        .ToListAsync();
    Assert.NotEmpty(journalEntries); // Process created journal entries
    
    // Verify partition boundaries respected
    Assert.All(journalEntries, entry => Assert.Equal(user.Id, entry.UserId));
}
```

### 3. End-to-End Tests

End-to-end tests validate complete user workflows through the full system stack, from API endpoints or UI interactions through all internal layers to final outcomes. These tests exercise the entire application as users would experience it, ensuring that all components integrate correctly to deliver the intended functionality.

The key principle for E2E testing is pragmatic boundary management. All internal system components use their real implementations—the actual database, real services, genuine business logic, and authentic data flows. However, external services that are costly, unreliable, or uncontrollable are mocked or stubbed to ensure test determinism and cost control.

For Veritheia, this means using real PostgreSQL, real Entity Framework operations, genuine process engines, and actual API controllers, while mocking the LLM services (OpenAI, Ollama), external search services, email providers, or any third-party APIs. The goal is to verify the complete system orchestration while controlling for external uncertainty.

#### Complete User Journey Tests

```csharp
// E2E test - full API workflow with real internal stack, mocked external services
[Fact]
public async Task CompleteResearchJourney_FromCreationToScreening_WorksEndToEnd()
{
    // Arrange - real API test server with real database
    using var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    
    // Mock external LLM service at the boundary
    var mockCognitiveAdapter = factory.Services.GetRequiredService<Mock<ICognitiveAdapter>>();
    mockCognitiveAdapter
        .Setup(x => x.GenerateTextAsync(It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync("This document contributes significantly to understanding adversarial attacks.");
    
    // Act 1 - Create user (real API, real database)
    var createUserResponse = await client.PostAsJsonAsync("/api/users", new
    {
        Email = "researcher@test.com",
        DisplayName = "Test Researcher"
    });
    var user = await createUserResponse.Content.ReadFromJsonAsync<User>();
    
    // Act 2 - Create persona (real API, real database)
    var createPersonaResponse = await client.PostAsJsonAsync("/api/personas", new
    {
        UserId = user.Id,
        Domain = "ML Security Researcher"
    });
    var persona = await createPersonaResponse.Content.ReadFromJsonAsync<Persona>();
    
    // Act 3 - Create journey (real API, real database, real business logic)
    var createJourneyResponse = await client.PostAsJsonAsync("/api/journeys", new
    {
        UserId = user.Id,
        PersonaId = persona.Id,
        ProcessType = "SystematicScreening",
        Purpose = "Review adversarial ML attacks literature"
    });
    var journey = await createJourneyResponse.Content.ReadFromJsonAsync<Journey>();
    
    // Act 4 - Upload documents (real file processing, real database)
    var documentContent = "This paper presents novel adversarial attack methods...";
    var uploadResponse = await client.PostAsJsonAsync($"/api/journeys/{journey.Id}/documents", new
    {
        FileName = "adversarial_attacks.pdf",
        Content = documentContent,
        MimeType = "application/pdf"
    });
    
    // Act 5 - Run screening process (real process engine, real database, mocked LLM)
    var screeningResponse = await client.PostAsJsonAsync($"/api/journeys/{journey.Id}/processes/screening", new
    {
        ResearchQuestions = "RQ1: What are the main adversarial attack methods?",
        Definitions = new Dictionary<string, string>
        {
            ["adversarial"] = "Intentionally crafted malicious inputs"
        }
    });
    
    // Assert - verify complete end-to-end outcome
    screeningResponse.EnsureSuccessStatusCode();
    var result = await screeningResponse.Content.ReadFromJsonAsync<ProcessExecutionResult>();
    
    Assert.True(result.Success);
    Assert.NotEmpty(result.Data);
    
    // Verify the complete workflow created proper database state
    using var scope = factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<VeritheiaDbContext>();
    
    // Check journey was created with correct partition
    var savedJourney = await dbContext.Journeys
        .FirstOrDefaultAsync(j => j.UserId == user.Id && j.Id == journey.Id);
    Assert.NotNull(savedJourney);
    Assert.Equal("SystematicScreening", savedJourney.ProcessType);
    
    // Check documents were processed and segmented
    var segments = await dbContext.JourneyDocumentSegments
        .Where(s => s.UserId == user.Id && s.JourneyId == journey.Id)
        .ToListAsync();
    Assert.NotEmpty(segments);
    
    // Check journal entries were created by the process
    var journalEntries = await dbContext.JournalEntries
        .Where(e => e.UserId == user.Id)
        .ToListAsync();
    Assert.NotEmpty(journalEntries);
    
    // Verify all data respects partition boundaries
    Assert.All(segments, s => Assert.Equal(user.Id, s.UserId));
    Assert.All(journalEntries, e => Assert.Equal(user.Id, e.UserId));
}

// E2E test - error handling across full stack
[Fact]
public async Task CreateJourney_WithInvalidPersona_ReturnsProperErrorResponse()
{
    // Arrange
    using var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    
    var user = await CreateTestUserViaAPI(client);
    var invalidPersonaId = Guid.CreateVersion7(); // Non-existent persona
    
    // Act - attempt invalid journey creation
    var response = await client.PostAsJsonAsync("/api/journeys", new
    {
        UserId = user.Id,
        PersonaId = invalidPersonaId, // Invalid - will fail constraint
        ProcessType = "SystematicScreening",
        Purpose = "Test journey"
    });
    
    // Assert - proper error handling through full stack
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    
    var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
    Assert.Contains("persona", errorResponse.Message, StringComparison.OrdinalIgnoreCase);
    
    // Verify no partial state was created in database
    using var scope = factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<VeritheiaDbContext>();
    
    var journeys = await dbContext.Journeys
        .Where(j => j.UserId == user.Id)
        .ToListAsync();
    Assert.Empty(journeys); // No journey should have been created
}
```

### 4. Context Assembly Tests

Verify that journey context is properly assembled.

```csharp
[Theory]
[InlineData(4000, 5, "minimal")]     // 4K context
[InlineData(32000, 20, "standard")]  // 32K context  
[InlineData(100000, 50, "extended")] // 100K+ context
public async Task AssembleContext_RespectsTokenLimits(int maxTokens, int expectedEntries, string contextType)
{
    // Arrange
    var journey = CreateJourneyWithManyEntries();
    var assembler = new ContextAssemblyService();
    
    // Act
    var context = await assembler.AssembleContextAsync(journey.Id, new ContextRequest
    {
        MaxTokens = maxTokens,
        IncludePersona = true
    });
    
    // Assert
    var tokenCount = tokenizer.CountTokens(context);
    Assert.True(tokenCount <= maxTokens);
    Assert.Contains($"Context type: {contextType}", context);
}

[Fact]
public async Task AssembleContext_WithMultipleJournals_PrioritizesSignificance()
{
    // Arrange
    var journey = CreateJourneyWithEntries();
    var assembler = new ContextAssemblyService();
    
    // Act
    var context = await assembler.AssembleContextAsync(journey.Id, new ContextRequest
    {
        MaxTokens = 32000,
        IncludePersona = true
    });
    
    // Assert
    Assert.Contains("Critical finding about neural architectures", context);
    Assert.Contains("milestone in understanding", context);
    Assert.DoesNotContain("routine observation", context); // Should be filtered
}

[Fact]
public async Task AssembleContext_IncludesActivePersonaDomain()
{
    // Arrange
    var journey = CreateJourneyWithPersona("Researcher");
    
    // Act
    var context = await assembler.AssembleContextAsync(journey.Id);
    
    // Assert
    Assert.Contains("Domain: Researcher", context);
    Assert.Contains("research methodology", context); // Domain-specific vocabulary
}

[Fact]
public async Task AssembleContext_MaintainsNarrativeCoherence()
{
    // Arrange
    var journey = CreateResearchJourney();
    
    // Act
    var context = await assembler.AssembleContextAsync(journey.Id);
    
    // Assert
    // Context should flow as coherent narrative, not disconnected entries
    Assert.Matches(@"Initially.*Subsequently.*This led to.*Finally", context);
}
```

### 5. Extension Testing Patterns

Guidelines for testing process extensions.

#### Extension Test Structure

```csharp
public abstract class ProcessExtensionTestBase<TProcess> 
    where TProcess : IAnalyticalProcess
{
    protected abstract TProcess CreateProcess();
    protected abstract ProcessContext CreateValidContext();
    
    [Fact]
    public async Task Process_WithValidContext_ProducesResult()
    {
        // Arrange
        var process = CreateProcess();
        var context = CreateValidContext();
        
        // Act
        var result = await process.ExecuteAsync(context);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(context.ExecutionId, result.ExecutionId);
    }
    
    [Fact]
    public async Task Process_WritesJournalEntries()
    {
        // Every process should journal its activities
        // Verify journal service was called appropriately
    }
}
```

#### Extension Isolation Tests

```csharp
[Fact]
public async Task Extensions_CannotAccessOtherExtensionData()
{
    // Arrange
    var screeningResult = new ProcessResult 
    { 
        ProcessType = "SystematicScreening",
        Data = new { Results = new[] { new ScreeningResult() } }
    };
    context.ScreeningResults.Add(screeningResult);
    await context.SaveChangesAsync();
    
    // Act & Assert
    await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
    {
        // Composition process trying to access screening data
        var compositionProcess = new GuidedCompositionProcess();
        await compositionProcess.AccessProcessResult(screeningResult.Id);
    });
}

[Fact]
public async Task Extensions_CannotQueryOtherExtensionTables()
{
    // Arrange
    var assignment = new Assignment { TeacherId = TestUsers.Teacher.Id };
    context.Assignments.Add(assignment);
    await context.SaveChangesAsync();
    
    // Act & Assert
    // Screening process should not be able to query assignments
    var screeningProcess = new SystematicScreeningProcess();
    Assert.False(screeningProcess.HasAccessTo("assignments"));
}
```

### Extension Storage Tests

```csharp
// For JSONB storage pattern
[Fact]
public async Task ScreeningResults_StoreCorrectlyInProcessResult()
{
    // Arrange
    var results = new List<ScreeningResult> { /* ... */ };
    var processResult = new ProcessResult
    {
        ProcessType = "SystematicScreening",
        Data = new ScreeningProcessResult { Results = results }
    };
    
    // Act - Direct DbContext usage through service
    await processService.SaveResultAsync(processResult);
    var loaded = await processService.GetResultAsync(processResult.ExecutionId);
    
    // Assert
    var data = loaded.GetData<ScreeningProcessResult>();
    Assert.Equal(results.Count, data.Results.Count);
}

// For dedicated table pattern
[Fact]
public async Task Assignment_MaintainsReferentialIntegrity()
{
    // Arrange
    var assignment = new Assignment
    {
        TeacherId = TestUsers.Teacher.Id,
        Title = "Descriptive Writing Exercise"
    };
    
    // Act & Assert
    await Assert.ThrowsAsync<ForeignKeyException>(async () =>
    {
        assignment.TeacherId = Guid.NewGuid(); // Non-existent user
        context.Assignments.Add(assignment);
        await context.SaveChangesAsync();
    });
}
```

## Test Data Management

### Test Data Principles

1. **Realistic Scenarios**: Use data that reflects actual usage
2. **Isolation**: Each test manages its own data
3. **Deterministic**: Same inputs produce same outputs
4. **Minimal**: Only create data necessary for the test

### Test Data Builders

```csharp
public class TestDataBuilder
{
    public static User CreateResearcher(string name = "Test Researcher")
    {
        return new User
        {
            Email = $"{name.Replace(" ", "").ToLower()}@test.edu",
            DisplayName = name,
            LastActiveAt = DateTime.UtcNow
        };
    }
    
    public static Persona CreatePersona(User user, string domain = "Researcher")
    {
        return new Persona
        {
            UserId = user.Id,
            Domain = domain,
            IsActive = true,
            ConceptualVocabulary = new Dictionary<string, int>(),
            Patterns = new List<InquiryPattern>(),
            LastEvolved = DateTime.UtcNow
        };
    }
    
    public static Journey CreateActiveJourney(User user, Persona persona, string processType)
    {
        var journey = new Journey
        {
            UserId = user.Id,
            PersonaId = persona.Id,
            ProcessType = processType,
            Purpose = $"Test {processType} journey",
            State = JourneyState.Active
        };
        
        // Create default journals
        journey.CreateJournal(JournalType.Research);
        journey.CreateJournal(JournalType.Method);
        journey.CreateJournal(JournalType.Decision);
        journey.CreateJournal(JournalType.Reflection);
        
        return journey;
    }
    
    public static Document CreateTestDocument(string title, string[] authors)
    {
        return new Document
        {
            FileName = $"{title.Replace(" ", "_")}.pdf",
            MimeType = "application/pdf",
            FilePath = $"/test/documents/{Guid.NewGuid()}.pdf",
            FileSize = 1024 * 100, // 100KB
            Metadata = new DocumentMetadata
            {
                Title = title,
                Authors = authors,
                PublicationDate = DateTime.UtcNow.AddMonths(-6)
            }
        };
    }
}
```

### Test Fixtures

```csharp
public class MLSecurityTestFixture : IAsyncLifetime
{
    public List<Document> Documents { get; private set; }
    public KnowledgeScope Scope { get; private set; }
    public User Researcher { get; private set; }
    
    public async Task InitializeAsync()
    {
        Researcher = TestDataBuilder.CreateResearcher("ML Security Researcher");
        Scope = new KnowledgeScope
        {
            Name = "Adversarial ML",
            Type = ScopeType.Topic
        };
        
        Documents = new List<Document>
        {
            TestDataBuilder.CreateTestDocument(
                "Adversarial Examples in Neural Networks",
                new[] { "Goodfellow, I.", "Shlens, J." }
            ),
            TestDataBuilder.CreateTestDocument(
                "Robust Physical-World Attacks",
                new[] { "Eykholt, K.", "Evtimov, I." }
            ),
            // ... more test documents
        };
        
        // Create processed content with embeddings
        foreach (var doc in Documents)
        {
            await CreateProcessedContent(doc);
        }
    }
    
    public Task DisposeAsync() => Task.CompletedTask;
}
```

## Coverage Expectations

### Core Platform Coverage

| Component | Unit | Integration | Behavioral | Target |
|-----------|------|-------------|------------|--------|
| User Management | 90% | 85% | 95% | 90% |
| Journey System | 95% | 90% | 95% | 93% |
| Journal Management | 90% | 85% | 90% | 88% |
| Knowledge Storage | 85% | 90% | 85% | 87% |
| Process Infrastructure | 90% | 85% | 90% | 88% |
| Context Assembly | 95% | 90% | 100% | 95% |

### Extension Coverage Requirements

Each extension must achieve:
- Unit test coverage: 85% minimum
- Integration test coverage: 80% minimum
- At least 5 behavioral scenarios
- Journal integration tests
- Storage pattern tests

### Critical Path Coverage

These scenarios must have 100% coverage:

1. **Journey Creation and Context**
   - User creates journey
   - Journals are initialized
   - Context is assembled correctly

2. **Process Execution**
   - Input validation
   - Journal writing
   - Result storage
   - Error handling

3. **User Attribution**
   - All data tied to user
   - Journey boundaries respected
   - No cross-journey leakage

## Test Execution

### Local Development

```bash
# Run all tests
dotnet test

# Run specific category
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration
dotnet test --filter Category=Behavioral

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test veritheia.Tests.Unit
dotnet test veritheia.Tests.Integration
```

### CI/CD Pipeline

```yaml
test:
  stage: test
  script:
    - dotnet test --filter Category!=RequiresDatabase
    - dotnet test --filter Category=RequiresDatabase
    - dotnet test --collect:"XPlat Code Coverage"
    - reportgenerator -reports:coverage.xml -targetdir:coverage
  artifacts:
    reports:
      coverage_report:
        coverage_format: cobertura
        path: coverage/coverage.xml
```

### Test Database

```csharp
public class TestDbContext : VeritheiaDbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql($"Host=localhost;Database=veritheia_test_{Guid.NewGuid()};")
               .EnableSensitiveDataLogging();
    }
    
    public async Task InitializeAsync()
    {
        await Database.EnsureCreatedAsync();
        // Apply any test-specific seed data
    }
    
    public async Task DisposeAsync()
    {
        await Database.EnsureDeletedAsync();
    }
}
```

## Mocking Strategies

### Cognitive Adapter Test Doubles

**CRITICAL**: Test doubles for cognitive adapters are ONLY permitted for integration path testing when LLM endpoints are unavailable. Production and development environments MUST use real LLM endpoints. Test doubles must be clearly isolated and never accessible in production code paths.

```csharp
// TEST ONLY - For CI environments without LLM access
// This implementation validates data flow paths, NOT formation validity
public class TestCognitiveAdapter : ICognitiveAdapter
{
    private readonly bool _deterministicMode;
    
    public TestCognitiveAdapter(bool deterministic = true)
    {
        _deterministicMode = deterministic;
    }
    
    public Task<EmbeddingResult> CreateEmbeddingsAsync(string text)
    {
        // Return deterministic embeddings based on text hash
        var hash = text.GetHashCode();
        var embedding = new float[1536];
        
        if (_deterministicMode)
        {
            // Fill with deterministic values
            for (int i = 0; i < embedding.Length; i++)
            {
                embedding[i] = (float)Math.Sin(hash + i) * 0.1f;
            }
        }
        else
        {
            // Random for similarity testing
            var random = new Random(hash);
            for (int i = 0; i < embedding.Length; i++)
            {
                embedding[i] = (float)(random.NextDouble() - 0.5) * 0.1f;
            }
        }
        
        return Task.FromResult(new EmbeddingResult { Embedding = embedding });
    }
    
    public Task<string> GenerateTextAsync(string prompt, GenerationParameters parameters)
    {
        // Return structured responses for testing
        if (prompt.Contains("relevance assessment"))
        {
            return Task.FromResult("The document discusses topics directly related to the research questions, specifically addressing neural network vulnerabilities.");
        }
        
        if (prompt.Contains("contribution assessment"))
        {
            return Task.FromResult("The document makes a significant contribution by proposing a novel defense mechanism against adversarial attacks.");
        }
        
        return Task.FromResult("Test response for: " + prompt.Substring(0, Math.Min(50, prompt.Length)));
    }
}
```

### Journal Service Mocking

```csharp
public class JournalServiceMock : Mock<IJournalService>
{
    private readonly List<JournalEntry> _entries = new();
    
    public JournalServiceMock()
    {
        Setup(x => x.AddEntryAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<EntryMetadata>()))
            .ReturnsAsync((Guid journalId, string content, EntryMetadata metadata) =>
            {
                var entry = new JournalEntry
                {
                    JournalId = journalId,
                    Content = content,
                    Significance = metadata?.Significance ?? EntrySignificance.Routine,
                    Tags = metadata?.Tags ?? new List<string>()
                };
                _entries.Add(entry);
                return entry;
            });
            
        Setup(x => x.GetRecentEntriesAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<JournalType?>()))
            .ReturnsAsync((Guid journeyId, int count, JournalType? type) =>
            {
                return _entries
                    .OrderByDescending(e => e.CreatedAt)
                    .Take(count)
                    .ToList();
            });
    }
    
    public void VerifyJournalEntry(string expectedContent)
    {
        Assert.Contains(_entries, e => e.Content.Contains(expectedContent));
    }
}
```

## Anti-Patterns to Avoid

### ❌ Mislabeling Integration Tests as Unit Tests

The most common anti-pattern is calling any test that uses mocks a "unit test," regardless of whether it touches external systems. A true unit test exercises pure logic without external dependencies.

```csharp
// BAD: Called "unit test" but uses database - this is integration test
[Fact] // Labeled as "unit test" but isn't
public async Task CreateJourney_WithValidUser_CreatesJourney()
{
    await _dbContext.Users.AddAsync(user); // Database dependency!
    var service = new JourneyService(_dbContext);
    var result = await service.CreateJourneyAsync(request);
    Assert.NotNull(result);
}

// GOOD: Actual unit test - pure logic only
[Fact] // True unit test
public void Journey_ValidateTransition_RejectsInvalidStates()
{
    var journey = new Journey { State = JourneyState.Completed };
    var canTransition = journey.CanTransitionTo(JourneyState.Active);
    Assert.False(canTransition); // Pure logic, no external dependencies
}
```

### ❌ Mocking Non-Mockable Dependencies

Attempting to mock systems with intrinsic logic creates false confidence. PostgreSQL with constraints, triggers, and pgvector operations cannot be adequately simulated.

```csharp
// BAD: Mocking database with complex logic
[Fact]
public async Task StoreVector_WithEmbedding_SavesCorrectly()
{
    var mockContext = new Mock<VeritheiaDbContext>();
    // Mock cannot simulate pgvector operations, constraints, or composite keys
    mockContext.Setup(x => x.SearchVectors1536.Add(It.IsAny<SearchVector1536>()));
    // Test passes but doesn't verify real database behavior
}

// GOOD: Use real database for integration test
[Fact]
public async Task StoreVector_WithEmbedding_SavesCorrectly()
{
    // Real database with real pgvector operations
    var vector = new SearchVector1536 { /* real data */ };
    Context.SearchVectors1536.Add(vector);
    await Context.SaveChangesAsync();
    // Verifies actual database constraints and operations
}
```

### ❌ Testing External Service Content

Testing specific outputs from external services creates brittle tests that break when the service changes, without providing real value.

```csharp
// BAD: Testing specific LLM output content
[Fact]
public async Task CognitiveAdapter_GeneratesSummary_ReturnsExpectedText()
{
    var summary = await adapter.GenerateTextAsync("Summarize this paper");
    Assert.Equal("This paper presents a novel approach to...", summary);
    // Brittle - LLM output can vary while still being correct
}

// GOOD: Testing integration behavior and format
[Fact]
public async Task CognitiveAdapter_GeneratesSummary_ReturnsValidSummary()
{
    var summary = await adapter.GenerateTextAsync("Summarize this paper");
    Assert.NotNull(summary);
    Assert.True(summary.Length > 50); // Reasonable length
    Assert.True(summary.Length < 2000); // Not truncated
    // Tests integration without depending on specific content
}
```

### ❌ Excessive Mocking in Integration Tests

Creating elaborate mock setups for internal services defeats the purpose of integration testing, which is to verify real component collaboration.

```csharp
// BAD: Over-mocking in integration test
[Fact]
public async Task ProcessEngine_ExecutesProcess_WorksCorrectly()
{
    var mockJournalService = new Mock<IJournalService>();
    var mockDocumentService = new Mock<IDocumentService>();
    var mockEmbeddingService = new Mock<IEmbeddingService>();
    // So many mocks that we're not testing real integration
    
    var engine = new ProcessEngine(mockJournalService.Object, /* etc */);
    // Test doesn't verify real service collaboration
}

// GOOD: Real services with mocked external boundaries only
[Fact]
public async Task ProcessEngine_ExecutesProcess_WorksCorrectly()
{
    // Real database, real internal services
    var mockCognitiveAdapter = new Mock<ICognitiveAdapter>(); // Only external service
    var engine = new ProcessEngine(Context, mockCognitiveAdapter.Object);
    // Tests real service integration with controlled external dependency
}
```

## Performance Testing

While not part of functional tests, track these metrics:

### Baseline Metrics

```csharp
[Fact]
[Trait("Category", "Performance")]
public async Task EmbeddingGeneration_MeetsBaselinePerformance()
{
    var text = TestDataBuilder.CreateLongText(5000); // ~1000 tokens
    
    var stopwatch = Stopwatch.StartNew();
    await embeddingGenerator.GenerateEmbeddingAsync(text);
    stopwatch.Stop();
    
    Assert.True(stopwatch.ElapsedMilliseconds < 500, 
        $"Embedding generation took {stopwatch.ElapsedMilliseconds}ms, expected < 500ms");
}

[Fact]
[Trait("Category", "Performance")]
public async Task SemanticSearch_ReturnsResultsQuickly()
{
    // With 10,000 documents
    var query = "adversarial attacks on neural networks";
    
    var stopwatch = Stopwatch.StartNew();
    var results = await searchService.SemanticSearchAsync(query, limit: 20);
    stopwatch.Stop();
    
    Assert.True(stopwatch.ElapsedMilliseconds < 200,
        $"Semantic search took {stopwatch.ElapsedMilliseconds}ms, expected < 200ms");
}
```

## Test Maintenance

### Test Review Checklist

- [ ] Test is correctly categorized (Unit/Integration/E2E)
- [ ] Unit tests have no external dependencies or side effects
- [ ] Integration tests use real database, mock only external services
- [ ] E2E tests exercise complete user workflows
- [ ] Test name clearly describes what is being tested
- [ ] Arrange-Act-Assert structure is followed
- [ ] Test is independent and can run in any order
- [ ] Test data is minimal and focused
- [ ] Mocks are used only for external, uncontrollable services
- [ ] Test verifies behavior, not implementation details
- [ ] Partition boundaries are respected in all test data
- [ ] Edge cases and error conditions are covered

### Test Category Summary

**Unit Tests**: Pure logic, no external dependencies, fully deterministic. Test individual methods, value objects, and domain logic in complete isolation.

**Integration Tests**: Real database with full schema, mocked external services only. Verify component collaboration where correctness depends on systems with intrinsic logic (PostgreSQL constraints, pgvector operations).

**End-to-End Tests**: Complete user workflows through full internal stack, mocked external services for cost/reliability. Validate system orchestration from API/UI through all layers to final outcomes.

### Continuous Improvement

1. **Test Category Accuracy**: Regularly audit that tests are correctly categorized
2. **Flaky Test Detection**: Track and fix intermittent failures
3. **Coverage Gaps**: Regular review of uncovered code paths, especially pure domain logic
4. **Test Speed**: Monitor and optimize slow tests, especially E2E workflows
5. **Documentation**: Keep test scenarios aligned with features and architectural principles

The testing strategy ensures Veritheia's technical implementation serves its philosophical foundation. Every test validates that the system mechanically enforces user authorship boundaries, preserving intellectual sovereignty through structured engagement with knowledge rather than passive consumption of AI-generated content.