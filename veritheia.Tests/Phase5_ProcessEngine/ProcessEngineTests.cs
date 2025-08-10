using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Veritheia.Core.Interfaces;
using Veritheia.Core.ValueObjects;
using Veritheia.Data;
using Veritheia.Data.Entities;
using Veritheia.Data.Processes;
using Veritheia.Data.Services;
using veritheia.Tests.TestBase;
using Xunit;

namespace Veritheia.Tests.Phase5_ProcessEngine;

/// <summary>
/// Tests for Phase 5 Process Engine Infrastructure
/// Validates process registration, discovery, and execution
/// </summary>
[Collection("DatabaseTests")]
[Trait("Category", "Integration")]
public class ProcessEngineTests : DatabaseTestBase
{
    private readonly ProcessEngine _processEngine;
    private readonly IServiceProvider _serviceProvider;
    
    public ProcessEngineTests(DatabaseFixture fixture) : base(fixture)
    {
        var services = new ServiceCollection();
        services.AddSingleton(Context);
        services.AddLogging();
        
        // Register cognitive adapter (mock for testing)
        services.AddSingleton<ICognitiveAdapter, MockCognitiveAdapter>();
        
        // Register processes
        services.AddTransient<BasicSystematicScreeningProcess>();
        services.AddTransient<BasicConstrainedCompositionProcess>();
        
        _serviceProvider = services.BuildServiceProvider();
        
        var logger = _serviceProvider.GetRequiredService<ILogger<ProcessEngine>>();
        _processEngine = new ProcessEngine(Context, _serviceProvider, logger);
    }
    
    [Fact]
    public void RegisterProcess_AddsToRegistry()
    {
        // Act
        _processEngine.RegisterProcess<BasicSystematicScreeningProcess>();
        
        // Assert
        var processes = _processEngine.GetAvailableProcesses();
        Assert.Contains(processes, p => p.ProcessId == "BasicSystematicScreeningProcess");
    }
    
    [Fact]
    public void GetAvailableProcesses_ReturnsAllRegistered()
    {
        // Arrange
        _processEngine.RegisterProcess<BasicSystematicScreeningProcess>();
        _processEngine.RegisterProcess<BasicConstrainedCompositionProcess>();
        
        // Act
        var processes = _processEngine.GetAvailableProcesses();
        
        // Assert
        Assert.Equal(2, processes.Count);
        
        var screening = processes.First(p => p.ProcessId == "BasicSystematicScreeningProcess");
        Assert.Equal("Basic Systematic Screening", screening.Name);
        Assert.Equal("Screening", screening.Category);
        Assert.NotNull(screening.Description);
        Assert.NotNull(screening.InputDefinition);
        
        var composition = processes.First(p => p.ProcessId == "BasicConstrainedCompositionProcess");
        Assert.Equal("Basic Constrained Composition", composition.Name);
        Assert.Equal("Composition", composition.Category);
    }
    
    [Fact]
    public async Task ExecuteProcess_RequiresValidJourney()
    {
        // Arrange
        _processEngine.RegisterProcess<BasicSystematicScreeningProcess>();
        var invalidJourneyId = Guid.NewGuid();
        
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _processEngine.ExecuteProcessAsync(
                "BasicSystematicScreeningProcess",
                invalidJourneyId,
                new Dictionary<string, object>()));
    }
    
    [Fact]
    public async Task ExecuteProcess_CreatesExecutionRecord()
    {
        // Arrange
        _processEngine.RegisterProcess<BasicSystematicScreeningProcess>();
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            DisplayName = "Test User",
            CreatedAt = DateTime.UtcNow
        };
        
        var persona = new Persona
        {
            Id = Guid.NewGuid(),
            Name = "Test Persona",
            Perspective = "Testing",
            CreatedAt = DateTime.UtcNow
        };
        
        var journey = new Journey
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            PersonaId = persona.Id,
            Persona = persona,
            Purpose = "Test Journey",
            State = "Active",
            StartedAt = DateTime.UtcNow
        };
        
        Context.Users.Add(user);
        Context.Personas.Add(persona);
        Context.Journeys.Add(journey);
        await Context.SaveChangesAsync();
        
        var inputs = new Dictionary<string, object>
        {
            ["researchQuestions"] = new[] { "What is the impact of AI on education?" },
            ["documents"] = new[] { Guid.NewGuid() }
        };
        
        // Act
        var result = await _processEngine.ExecuteProcessAsync(
            "BasicSystematicScreeningProcess",
            journey.Id,
            inputs);
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotEqual(Guid.Empty, result.ExecutionId);
        
        // Verify execution record in database
        var execution = await Context.ProcessExecutions
            .FindAsync(result.ExecutionId);
        
        Assert.NotNull(execution);
        Assert.Equal("BasicSystematicScreeningProcess", execution.ProcessId);
        Assert.Equal(journey.Id, execution.JourneyId);
        Assert.Equal("Completed", execution.State);
    }
    
    [Fact]
    public async Task ExecuteProcess_HandlesProcessFailure()
    {
        // Arrange
        _processEngine.RegisterProcess<BasicSystematicScreeningProcess>();
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            DisplayName = "Test User",
            CreatedAt = DateTime.UtcNow
        };
        
        var persona = new Persona
        {
            Id = Guid.NewGuid(),
            Name = "Test Persona",
            Perspective = "Testing",
            CreatedAt = DateTime.UtcNow
        };
        
        var journey = new Journey
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            PersonaId = persona.Id,
            Persona = persona,
            Purpose = "Test Journey",
            State = "Active",
            StartedAt = DateTime.UtcNow
        };
        
        Context.Users.Add(user);
        Context.Personas.Add(persona);
        Context.Journeys.Add(journey);
        await Context.SaveChangesAsync();
        
        // Invalid inputs to trigger validation failure
        var inputs = new Dictionary<string, object>();
        
        // Act
        var result = await _processEngine.ExecuteProcessAsync(
            "BasicSystematicScreeningProcess",
            journey.Id,
            inputs);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains("Required input 'researchQuestions' is missing", result.ErrorMessage);
        
        // Verify execution record shows failure
        var execution = await Context.ProcessExecutions
            .FindAsync(result.ExecutionId);
        
        Assert.NotNull(execution);
        Assert.Equal("Failed", execution.State);
    }
    
    [Fact]
    public void ProcessInfo_ContainsInputDefinition()
    {
        // Arrange
        _processEngine.RegisterProcess<BasicConstrainedCompositionProcess>();
        
        // Act
        var processes = _processEngine.GetAvailableProcesses();
        var composition = processes.First(p => p.ProcessId == "BasicConstrainedCompositionProcess");
        
        // Assert
        Assert.NotNull(composition.InputDefinition);
        
        var inputs = composition.InputDefinition.Inputs;
        Assert.Contains("prompt", inputs.Keys);
        Assert.Contains("constraints", inputs.Keys);
        Assert.Contains("sourceDocuments", inputs.Keys);
        
        var promptInput = inputs["prompt"];
        Assert.Equal("string", promptInput.Type);
        Assert.True(promptInput.Required);
        Assert.Equal("The prompt or assignment description", promptInput.Description);
    }
    
    [Fact]
    public async Task ExecuteProcess_StoresResultData()
    {
        // Arrange
        _processEngine.RegisterProcess<BasicConstrainedCompositionProcess>();
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            DisplayName = "Test User",
            CreatedAt = DateTime.UtcNow
        };
        
        var persona = new Persona
        {
            Id = Guid.NewGuid(),
            Name = "Test Persona",
            Perspective = "Testing",
            CreatedAt = DateTime.UtcNow
        };
        
        var journey = new Journey
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            PersonaId = persona.Id,
            Persona = persona,
            Purpose = "Test Journey",
            State = "Active",
            StartedAt = DateTime.UtcNow
        };
        
        Context.Users.Add(user);
        Context.Personas.Add(persona);
        Context.Journeys.Add(journey);
        await Context.SaveChangesAsync();
        
        var inputs = new Dictionary<string, object>
        {
            ["prompt"] = "Write a test document",
            ["constraints"] = new[] { "Must be brief", "Must be clear" },
            ["sourceDocuments"] = new Guid[] { }
        };
        
        // Act
        var result = await _processEngine.ExecuteProcessAsync(
            "BasicConstrainedCompositionProcess",
            journey.Id,
            inputs);
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        
        // Verify result contains expected data
        Assert.ContainsKey("generatedDocument", result.Data);
        Assert.ContainsKey("validationResults", result.Data);
    }
}

// Mock cognitive adapter for testing
public class MockCognitiveAdapter : ICognitiveAdapter
{
    public Task<float[]> CreateEmbedding(string text)
    {
        // Return mock embedding
        var embedding = new float[384];
        for (int i = 0; i < embedding.Length; i++)
        {
            embedding[i] = (float)(i * 0.001);
        }
        return Task.FromResult(embedding);
    }
    
    public Task<string> GenerateTextAsync(string prompt, string? systemPrompt = null)
    {
        // Return mock response based on prompt
        if (prompt.Contains("research question"))
        {
            return Task.FromResult("INCLUDE: This document is relevant to the research question. Score: 0.8");
        }
        
        if (prompt.Contains("Write"))
        {
            return Task.FromResult("This is a generated test document that meets the specified constraints.");
        }
        
        return Task.FromResult("Mock response for: " + prompt);
    }
}