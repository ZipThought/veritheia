using System;
using System.Collections.Generic;
using System.Linq;
using Veritheia.Core.ValueObjects;
using Xunit;

namespace veritheia.Tests.Phase2_DomainModels;

public class ValueObjectTests
{
    [Fact]
    public void ProcessContext_Can_Store_And_Retrieve_Inputs()
    {
        // Arrange
        var context = new ProcessContext
        {
            ExecutionId = Guid.CreateVersion7(),
            UserId = Guid.CreateVersion7(),
            JourneyId = Guid.CreateVersion7(),
            Inputs = new Dictionary<string, object>
            {
                ["researchQuestions"] = new[] { "RQ1: What is AI?", "RQ2: How does ML work?" },
                ["threshold"] = 0.7,
                ["maxResults"] = 100,
                ["includeMetadata"] = true
            }
        };
        
        // Act & Assert - Test GetInput<T> method
        var questions = context.GetInput<string[]>("researchQuestions");
        Assert.NotNull(questions);
        Assert.Equal(2, questions.Length);
        Assert.Contains("RQ1: What is AI?", questions);
        
        var threshold = context.GetInput<double>("threshold");
        Assert.Equal(0.7, threshold);
        
        var maxResults = context.GetInput<int>("maxResults");
        Assert.Equal(100, maxResults);
        
        var includeMetadata = context.GetInput<bool>("includeMetadata");
        Assert.True(includeMetadata);
        
        // Test missing key
        var missing = context.GetInput<string>("nonexistent");
        Assert.Null(missing);
    }
    
    [Fact]
    public void JourneyContext_Stores_Journey_Specific_Data()
    {
        // Arrange
        var journeyContext = new JourneyContext
        {
            Purpose = "Systematic review of AI safety literature",
            ResearchQuestions = new List<string>
            {
                "What are the main AI safety concerns?",
                "How are these concerns being addressed?"
            },
            ConceptualVocabulary = new Dictionary<string, string>
            {
                ["alignment"] = "The process of ensuring AI systems pursue intended goals",
                ["robustness"] = "System resilience to distributional shift",
                ["interpretability"] = "Understanding model decision-making"
            },
            State = new Dictionary<string, object>
            {
                ["currentPhase"] = "Initial screening",
                ["documentsReviewed"] = 45
            },
            RecentEntries = new List<JournalEntryContext>
            {
                new JournalEntryContext
                {
                    Id = Guid.CreateVersion7(),
                    Content = "Identified key themes",
                    Significance = "High",
                    CreatedAt = DateTime.UtcNow,
                    Tags = new List<string> { "milestone", "synthesis" }
                }
            }
        };
        
        // Assert
        Assert.Equal("Systematic review of AI safety literature", journeyContext.Purpose);
        Assert.Equal(2, journeyContext.ResearchQuestions.Count);
        Assert.Equal(3, journeyContext.ConceptualVocabulary.Count);
        Assert.Contains("alignment", journeyContext.ConceptualVocabulary.Keys);
        Assert.Equal("Initial screening", journeyContext.State["currentPhase"]);
        Assert.Single(journeyContext.RecentEntries);
    }
    
    [Fact]
    public void PersonaContext_Tracks_User_Evolution()
    {
        // Arrange
        var personaContext = new PersonaContext
        {
            DomainFocus = "Machine Learning Research",
            RelevantVocabulary = new List<string>
            {
                "neural network",
                "gradient descent",
                "backpropagation",
                "loss function"
            },
            ActivePatterns = new List<InquiryPattern>
            {
                new InquiryPattern 
                { 
                    PatternType = "comparative",
                    Description = "Compares different model architectures",
                    OccurrenceCount = 12,
                    LastObserved = DateTime.UtcNow.AddDays(-2)
                },
                new InquiryPattern
                {
                    PatternType = "causal",
                    Description = "Investigates why models fail",
                    OccurrenceCount = 8,
                    LastObserved = DateTime.UtcNow.AddDays(-1)
                }
            },
            MethodologicalPreferences = new List<string>
            {
                "Empirical validation",
                "Mathematical proofs",
                "Ablation studies"
            }
        };
        
        // Assert
        Assert.Equal("Machine Learning Research", personaContext.DomainFocus);
        Assert.Equal(4, personaContext.RelevantVocabulary.Count);
        Assert.Contains("neural network", personaContext.RelevantVocabulary);
        Assert.Equal(2, personaContext.ActivePatterns.Count);
        Assert.Equal("comparative", personaContext.ActivePatterns[0].PatternType);
        Assert.Equal(3, personaContext.MethodologicalPreferences.Count);
    }
    
    [Fact]
    public void InputDefinition_Fluent_API_Works()
    {
        // Arrange & Act
        var inputDef = new InputDefinition()
            .AddTextArea("researchQuestion", "The main research question for your investigation", true)
            .AddTextInput("hypothesis", "Your working hypothesis", false)
            .AddDropdown("methodology", "Research methodology", 
                new[] { "systematic", "narrative", "scoping", "rapid" }, true)
            .AddScopeSelector("scope", "Knowledge scope to search", false)
            .AddDocumentSelector("documents", "Select documents to analyze", true)
            .AddMultiSelect("themes", "Select relevant themes", 
                new[] { "safety", "ethics", "performance", "interpretability" }, false);
        
        // Assert
        Assert.Equal(6, inputDef.Fields.Count);
        
        var textAreaField = inputDef.Fields[0];
        Assert.Equal("researchQuestion", textAreaField.Name);
        Assert.Equal(InputFieldType.TextArea, textAreaField.Type);
        Assert.True(textAreaField.Required);
        
        var dropdownField = inputDef.Fields[2];
        Assert.Equal("methodology", dropdownField.Name);
        Assert.Equal(InputFieldType.Dropdown, dropdownField.Type);
        Assert.Equal(4, dropdownField.Options.Count);
        Assert.Contains("systematic", dropdownField.Options);
        
        var multiSelectField = inputDef.Fields[5];
        Assert.Equal("themes", multiSelectField.Name);
        Assert.Equal(InputFieldType.MultiSelect, multiSelectField.Type);
        Assert.False(multiSelectField.Required);
        Assert.Contains("ethics", multiSelectField.Options);
    }
    
    [Fact]
    public void FormationMarker_Tracks_Journey_Milestones()
    {
        // Arrange
        var marker = new FormationMarker
        {
            JourneyId = Guid.CreateVersion7(),
            OccurredAt = DateTime.UtcNow,
            InsightDescription = "Connected distributed systems theory with biological neural networks",
            Context = "After reading three papers on parallel processing in nature",
            ContributingSegmentIds = new List<Guid>
            {
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Guid.CreateVersion7()
            }
        };
        
        // Assert
        Assert.NotEqual(Guid.Empty, marker.JourneyId);
        Assert.Contains("distributed systems", marker.InsightDescription);
        Assert.NotNull(marker.OccurredAt);
        Assert.Contains("parallel processing", marker.Context);
        Assert.Equal(3, marker.ContributingSegmentIds.Count);
    }
    
    [Fact]
    public void JournalEntryContext_Provides_Entry_Summary()
    {
        // Arrange
        var entry = new JournalEntryContext
        {
            Id = Guid.CreateVersion7(),
            Content = "Discovered that attention mechanisms mirror human cognitive processes",
            Significance = "Critical",
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            Tags = new List<string> { "breakthrough", "attention", "cognition" }
        };
        
        // Assert
        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.Contains("attention mechanisms", entry.Content);
        Assert.Equal("Critical", entry.Significance);
        Assert.Equal(3, entry.Tags.Count);
        Assert.Contains("breakthrough", entry.Tags);
    }
    
    [Fact]
    public void InputField_Stores_Field_Metadata()
    {
        // Arrange
        var field = new InputField
        {
            Name = "researchDomain",
            Description = "Select your primary research domain",
            Type = InputFieldType.Dropdown,
            Required = true,
            Options = new List<string> { "Computer Science", "Biology", "Physics", "Mathematics" },
            DefaultValue = "Computer Science"
        };
        
        // Assert
        Assert.Equal("researchDomain", field.Name);
        Assert.Equal(InputFieldType.Dropdown, field.Type);
        Assert.True(field.Required);
        Assert.Equal(4, field.Options.Count);
        Assert.Equal("Computer Science", field.DefaultValue);
    }
}