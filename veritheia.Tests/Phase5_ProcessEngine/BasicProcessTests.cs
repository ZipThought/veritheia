using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Veritheia.Core.Interfaces;
using Veritheia.Data;
using Veritheia.Data.Services;
using Moq;
using Xunit;

namespace Veritheia.Tests.Phase5_ProcessEngine;

/// <summary>
/// Basic process engine tests
/// Simplified to ensure compilation and basic validation
/// </summary>
[Trait("Category", "Unit")]
public class BasicProcessTests
{
    [Fact]
    public void ProcessEngine_CanBeCreated()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<VeritheiaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        using var context = new VeritheiaDbContext(options);
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLogger = new Mock<ILogger<ProcessEngine>>();
        
        // Act
        var engine = new ProcessEngine(
            context,
            mockServiceProvider.Object,
            mockLogger.Object);
        
        // Assert
        Assert.NotNull(engine);
    }
    
    [Fact]
    public void ProcessEngine_GetAvailableProcesses_ReturnsEmptyList()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<VeritheiaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        using var context = new VeritheiaDbContext(options);
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLogger = new Mock<ILogger<ProcessEngine>>();
        
        var engine = new ProcessEngine(
            context,
            mockServiceProvider.Object,
            mockLogger.Object);
        
        // Act
        var processes = engine.GetAvailableProcesses();
        
        // Assert
        Assert.NotNull(processes);
        Assert.Empty(processes);
    }
}