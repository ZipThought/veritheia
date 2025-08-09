using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Veritheia.Core.Enums;
using Veritheia.Data.Entities;
using veritheia.Tests.TestBase;
using Xunit;

namespace veritheia.Tests.Phase2_DomainModels;

public class EnumSerializationTests : DatabaseTestBase
{
    public EnumSerializationTests(DatabaseFixture fixture) : base(fixture) { }
    
    [Fact]
    public async Task JourneyState_Enum_Serializes_As_String()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "enum@test.com",
            DisplayName = "Enum Test",
            LastActiveAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        var persona = new Persona
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            Domain = "EnumTester",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var activeJourney = new Journey
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            PersonaId = persona.Id,
            Purpose = "Active journey",
            State = JourneyState.Active.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        
        var pausedJourney = new Journey
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            PersonaId = persona.Id,
            Purpose = "Paused journey",
            State = JourneyState.Paused.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        
        // Act
        Context.Users.Add(user);
        Context.Personas.Add(persona);
        Context.Journeys.AddRange(activeJourney, pausedJourney);
        await Context.SaveChangesAsync();
        
        // Assert - Check raw database values
        var journeys = await Context.Journeys
            .Where(j => j.UserId == user.Id)
            .ToListAsync();
            
        Assert.Equal(2, journeys.Count);
        Assert.Contains(journeys, j => j.State == "Active");
        Assert.Contains(journeys, j => j.State == "Paused");
    }
    
    [Fact]
    public async Task ProcessState_Enum_Works_With_ProcessExecution()
    {
        // Arrange
        var processDefinition = new ProcessDefinition
        {
            Id = Guid.CreateVersion7(),
            ProcessType = "SystematicScreening",
            Name = "Systematic Screening",
            Description = "Literature review process",
            Category = ProcessCategory.Analytical.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "process@test.com",
            DisplayName = "Process Test",
            LastActiveAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        var persona = new Persona
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            Domain = "Process Tester",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var journey = new Journey
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            PersonaId = persona.Id,
            Purpose = "Process test journey",
            State = JourneyState.Active.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        
        var execution = new ProcessExecution
        {
            Id = Guid.CreateVersion7(),
            JourneyId = journey.Id,
            ProcessType = processDefinition.ProcessType,
            State = ProcessState.Running.ToString(),
            Inputs = new Dictionary<string, object> { ["trigger"] = "manual" },
            StartedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        // Act
        Context.ProcessDefinitions.Add(processDefinition);
        Context.Users.Add(user);
        Context.Personas.Add(persona);
        Context.Journeys.Add(journey);
        Context.ProcessExecutions.Add(execution);
        await Context.SaveChangesAsync();
        
        // Update state
        execution.State = ProcessState.Completed.ToString();
        execution.CompletedAt = DateTime.UtcNow;
        await Context.SaveChangesAsync();
        
        // Assert
        var savedExecution = await Context.ProcessExecutions
            .FirstOrDefaultAsync(e => e.Id == execution.Id);
            
        Assert.NotNull(savedExecution);
        Assert.Equal("Completed", savedExecution.State);
        Assert.Equal("manual", savedExecution.Inputs["trigger"]);
    }
    
    [Fact]
    public async Task JournalType_Enum_Works_With_Journal()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "journal@test.com",
            DisplayName = "Journal Test",
            LastActiveAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        var persona = new Persona
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            Domain = "Journal Tester",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var journey = new Journey
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            PersonaId = persona.Id,
            Purpose = "Journal test journey",
            State = JourneyState.Active.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        
        var researchJournal = new Journal
        {
            Id = Guid.CreateVersion7(),
            JourneyId = journey.Id,
            Type = JournalType.Research.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        
        var methodJournal = new Journal
        {
            Id = Guid.CreateVersion7(),
            JourneyId = journey.Id,
            Type = JournalType.Method.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        
        var decisionJournal = new Journal
        {
            Id = Guid.CreateVersion7(),
            JourneyId = journey.Id,
            Type = JournalType.Decision.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        
        var reflectionJournal = new Journal
        {
            Id = Guid.CreateVersion7(),
            JourneyId = journey.Id,
            Type = JournalType.Reflection.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        
        // Act
        Context.Users.Add(user);
        Context.Personas.Add(persona);
        Context.Journeys.Add(journey);
        Context.Journals.AddRange(researchJournal, methodJournal, decisionJournal, reflectionJournal);
        await Context.SaveChangesAsync();
        
        // Assert
        var journals = await Context.Journals
            .Where(j => j.JourneyId == journey.Id)
            .Select(j => j.Type)
            .ToListAsync();
            
        Assert.Equal(4, journals.Count);
        Assert.Contains("Research", journals);
        Assert.Contains("Method", journals);
        Assert.Contains("Decision", journals);
        Assert.Contains("Reflection", journals);
    }
    
    [Fact]
    public async Task EntrySignificance_Enum_Works_With_JournalEntry()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = "significance@test.com",
            DisplayName = "Significance Test",
            LastActiveAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        var persona = new Persona
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            Domain = "Significance Tester",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var journey = new Journey
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            PersonaId = persona.Id,
            Purpose = "Significance test journey",
            State = JourneyState.Active.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        
        var journal = new Journal
        {
            Id = Guid.CreateVersion7(),
            JourneyId = journey.Id,
            Type = JournalType.Research.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        
        var criticalEntry = new JournalEntry
        {
            Id = Guid.CreateVersion7(),
            JournalId = journal.Id,
            Content = "Critical discovery about the research",
            Significance = EntrySignificance.Critical.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        
        var routineEntry = new JournalEntry
        {
            Id = Guid.CreateVersion7(),
            JournalId = journal.Id,
            Content = "Regular observation",
            Significance = EntrySignificance.Routine.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        
        // Act
        Context.Users.Add(user);
        Context.Personas.Add(persona);
        Context.Journeys.Add(journey);
        Context.Journals.Add(journal);
        Context.JournalEntries.AddRange(criticalEntry, routineEntry);
        await Context.SaveChangesAsync();
        
        // Assert
        var entries = await Context.JournalEntries
            .Where(e => e.JournalId == journal.Id)
            .ToListAsync();
            
        Assert.Equal(2, entries.Count);
        Assert.Contains(entries, e => e.Significance == "Critical");
        Assert.Contains(entries, e => e.Significance == "Routine");
    }
    
    [Fact]
    public async Task ScopeType_Enum_Works_With_KnowledgeScope()
    {
        // Arrange
        var topicScope = new KnowledgeScope
        {
            Id = Guid.CreateVersion7(),
            Name = "Machine Learning",
            Type = ScopeType.Topic.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        
        var projectScope = new KnowledgeScope
        {
            Id = Guid.CreateVersion7(),
            Name = "Research Project 2024",
            Type = ScopeType.Project.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        
        var customScope = new KnowledgeScope
        {
            Id = Guid.CreateVersion7(),
            Name = "Personal Research",
            Type = ScopeType.Custom.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        
        // Act
        Context.KnowledgeScopes.AddRange(topicScope, projectScope, customScope);
        await Context.SaveChangesAsync();
        
        // Assert
        var scopes = await Context.KnowledgeScopes.ToListAsync();
        Assert.Equal(3, scopes.Count);
        Assert.Contains(scopes, s => s.Type == "Topic");
        Assert.Contains(scopes, s => s.Type == "Project");
        Assert.Contains(scopes, s => s.Type == "Custom");
    }
}