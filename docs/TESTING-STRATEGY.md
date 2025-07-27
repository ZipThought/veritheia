# Testing Strategy

This document defines the testing approach that ensures Veritheia maintains its core principle: users author their own understanding. Tests verify both technical correctness and philosophical alignment.

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

Test individual components in isolation.

#### Core Platform Unit Tests

```csharp
// Example: Testing Journey creation with Persona
[Fact]
public async Task CreateJourney_WithValidUserAndPersona_CreatesJourneyWithCorrectState()
{
    // Arrange
    var user = new User { Id = Guid.NewGuid(), Email = "researcher@example.com" };
    var persona = new Persona { Id = Guid.NewGuid(), UserId = user.Id, Domain = "Researcher" };
    var repository = new Mock<IJourneyRepository>();
    var service = new JourneyService(repository.Object);
    
    // Act
    var journey = await service.CreateJourneyAsync(new CreateJourneyRequest
    {
        UserId = user.Id,
        PersonaId = persona.Id,
        ProcessType = "SystematicScreening",
        Purpose = "Review ML security literature"
    });
    
    // Assert
    Assert.Equal(JourneyState.Active, journey.State);
    Assert.Equal(user.Id, journey.UserId);
    Assert.Equal(persona.Id, journey.PersonaId);
    Assert.NotEmpty(journey.Purpose);
}

// Example: Testing Multiple Personas
[Fact]
public async Task User_CanHaveMultiplePersonas()
{
    // Arrange
    var user = new User { Id = Guid.NewGuid(), Email = "multi@example.com" };
    var personaService = new PersonaService(repository.Object);
    
    // Act
    var studentPersona = await personaService.CreatePersonaAsync(user.Id, "Student");
    var entrepreneurPersona = await personaService.CreatePersonaAsync(user.Id, "Entrepreneur");
    
    // Assert
    var personas = await personaService.GetByUserIdAsync(user.Id);
    Assert.Equal(2, personas.Count);
    Assert.Contains(personas, p => p.Domain == "Student");
    Assert.Contains(personas, p => p.Domain == "Entrepreneur");
}

// Example: Testing Persona evolution
[Fact]
public void AddConceptualTerm_IncreasesFrequency()
{
    // Arrange
    var persona = new Persona { ConceptualVocabulary = new Dictionary<string, int>() };
    
    // Act
    persona.AddConceptualTerm("epistemic");
    persona.AddConceptualTerm("epistemic");
    
    // Assert
    Assert.Equal(2, persona.ConceptualVocabulary["epistemic"]);
}
```

#### Extension Unit Tests

```csharp
// Example: Testing ScreeningResult creation
[Fact]
public void ScreeningResult_RequiresBothAssessments()
{
    // Arrange & Act
    var result = new ScreeningResult
    {
        DocumentId = Guid.NewGuid(),
        IsRelevant = true,
        RelevanceScore = 0.8m,
        ContributesToRQ = false,
        ContributionScore = 0.3m
    };
    
    // Assert
    Assert.True(result.IsRelevant);
    Assert.False(result.ContributesToRQ);
    Assert.True(result.RelevanceScore > result.ContributionScore);
}
```

### 2. Integration Tests

Test component interactions within bounded contexts.

#### Repository Integration Tests

```csharp
[Fact]
public async Task SaveJourney_WithJournals_PersistsFullAggregate()
{
    // Uses test database
    await using var context = new TestDbContext();
    var repository = new JourneyRepository(context);
    
    // Arrange
    var journey = new Journey 
    { 
        UserId = TestUsers.Researcher.Id,
        ProcessType = "SystematicScreening",
        Purpose = "Test journey"
    };
    
    journey.CreateJournal(JournalType.Research);
    journey.CreateJournal(JournalType.Method);
    
    // Act
    await repository.SaveAsync(journey);
    var loaded = await repository.GetWithJournalsAsync(journey.Id);
    
    // Assert
    Assert.Equal(2, loaded.Journals.Count);
    Assert.Contains(loaded.Journals, j => j.Type == JournalType.Research);
}
```

#### Process Integration Tests

```csharp
[Fact]
public async Task SystematicScreening_ProcessesAllDocuments()
{
    // Arrange
    var processContext = new ProcessContext
    {
        UserId = TestUsers.Researcher.Id,
        JourneyId = Guid.NewGuid(),
        ScopeId = TestScopes.MLSecurity.Id,
        Inputs = new Dictionary<string, object>
        {
            ["researchQuestions"] = "RQ1: What are adversarial attacks?",
            ["definitions"] = new Dictionary<string, string>()
        }
    };
    
    var process = new SystematicScreeningProcess(
        mockKnowledgeRepo.Object,
        mockCognitiveAdapter.Object
    );
    
    // Act
    var result = await process.ExecuteAsync(processContext);
    
    // Assert
    var screeningData = result.GetData<ScreeningProcessResult>();
    Assert.Equal(10, screeningData.Results.Count); // All docs in scope
    Assert.All(screeningData.Results, r => 
    {
        Assert.NotNull(r.RelevanceRationale);
        Assert.NotNull(r.ContributionRationale);
    });
}
```

### 3. Behavioral Tests

Verify system behavior from user perspective using BDD approach.

#### Journey Behavior Specifications

```gherkin
Feature: User Journey Management
  As a researcher
  I want to manage multiple journeys
  So that I can pursue different inquiries simultaneously

  Scenario: Starting a new systematic review journey
    Given I am a registered user with "SystematicScreening" capability
    And I have uploaded 20 research papers to my knowledge base
    When I start a new journey with purpose "Review adversarial ML attacks"
    And I select the "SystematicScreening" process
    Then a new active journey should be created
    And 4 journals should be automatically created (Research, Method, Decision, Reflection)
    And the journey context should contain my purpose

  Scenario: Resuming an existing journey
    Given I have an active journey for "Literature review on privacy"
    And the journey has 15 journal entries
    When I resume the journey
    Then the context should include recent journal entries
    And I should see my previous progress
    And I can continue from where I left off
```

#### Process Behavior Specifications

```gherkin
Feature: Systematic Screening Process
  As a researcher
  I want to screen documents for relevance and contribution
  So that I can identify core papers for my research

  Background:
    Given I have an active journey for systematic review
    And my scope contains 50 research papers

  Scenario: Screening with clear research questions
    When I run systematic screening with:
      | Research Questions | RQ1: What are the main approaches to adversarial attacks? |
      | Definitions       | adversarial: intentionally crafted malicious inputs       |
    Then all 50 documents should be assessed
    And each document should have relevance and contribution scores
    And documents with high contribution should address specific RQs
    And rationales should reference my research questions

  Scenario: Journaling screening decisions
    When I complete a screening process
    Then decision journal should contain entries for:
      | High contribution papers with rationales |
      | Exclusion decisions for irrelevant papers |
      | Borderline cases requiring manual review |
    And method journal should record my screening criteria
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
    await repository.SaveAsync(screeningResult);
    
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
    await assignmentRepository.SaveAsync(assignment);
    
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
    
    // Act
    await repository.SaveProcessResultAsync(processResult);
    var loaded = await repository.GetProcessResultAsync(processResult.ExecutionId);
    
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
        await repository.SaveAsync(assignment);
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

### Cognitive Adapter Mocking

```csharp
public class MockCognitiveAdapter : ICognitiveAdapter
{
    private readonly bool _deterministicMode;
    
    public MockCognitiveAdapter(bool deterministic = true)
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

### ❌ Testing LLM Content

```csharp
// BAD: Testing specific LLM output
[Fact]
public void CognitiveAdapter_ReturnsSpecificSummary()
{
    var summary = await adapter.GenerateTextAsync("Summarize this paper");
    Assert.Equal("This paper presents a novel approach to...", summary);
}

// GOOD: Testing integration behavior
[Fact]
public void CognitiveAdapter_ReturnsSummary()
{
    var summary = await adapter.GenerateTextAsync("Summarize this paper");
    Assert.NotNull(summary);
    Assert.True(summary.Length > 50);
    Assert.True(summary.Length < 500);
}
```

### ❌ Cross-Process Dependencies

```csharp
// BAD: One process test depends on another
[Fact]
public void GuidedComposition_UsesScreeningResults()
{
    // First run screening process
    var screeningResult = await screeningProcess.ExecuteAsync(context);
    // Then use in composition - creates coupling
}

// GOOD: Mock the data you need
[Fact]
public void GuidedComposition_WithSourceDocuments()
{
    var context = new ProcessContext
    {
        Inputs = new Dictionary<string, object>
        {
            ["sourceDocuments"] = TestDataBuilder.CreateDocuments()
        }
    };
}
```

### ❌ Testing UI Implementation

```csharp
// BAD: Testing Blazor component internals
[Fact]
public void ScreeningResultsComponent_RendersTableWithClasses()
{
    var component = render.FindComponent<ScreeningResults>();
    Assert.Contains("table table-striped", component.Markup);
}

// GOOD: Testing component behavior
[Fact]
public void ScreeningResultsComponent_DisplaysAllResults()
{
    var component = render.FindComponent<ScreeningResults>();
    var rows = component.FindAll("tr[data-result]");
    Assert.Equal(10, rows.Count);
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
    var results = await repository.SemanticSearchAsync(query, limit: 20);
    stopwatch.Stop();
    
    Assert.True(stopwatch.ElapsedMilliseconds < 200,
        $"Semantic search took {stopwatch.ElapsedMilliseconds}ms, expected < 200ms");
}
```

## Test Maintenance

### Test Review Checklist

- [ ] Test name clearly describes what is being tested
- [ ] Arrange-Act-Assert structure is followed
- [ ] Test is independent and can run in any order
- [ ] Test data is minimal and focused
- [ ] Mocks are used appropriately, not excessively
- [ ] Test verifies behavior, not implementation
- [ ] Edge cases and error conditions are covered
- [ ] Test will not break with minor refactoring

### Continuous Improvement

1. **Flaky Test Detection**: Track and fix intermittent failures
2. **Coverage Gaps**: Regular review of uncovered code paths
3. **Test Speed**: Monitor and optimize slow tests
4. **Documentation**: Keep test scenarios aligned with features

The testing strategy verifies that Veritheia remains true to its core principle. Every test verifies that users author their own understanding through structured engagement with knowledge.