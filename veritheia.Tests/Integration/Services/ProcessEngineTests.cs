using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Veritheia.Core.Interfaces;
using Veritheia.Data;
using Veritheia.Data.Services;
using veritheia.Tests.TestBase;
using Moq;
using Xunit;

namespace veritheia.Tests.Integration.Services;

/// <summary>
/// Basic process engine tests
/// Using real PostgreSQL database with Respawn for proper integration testing
/// </summary>
[Trait("Category", "Integration")]
public class ProcessEngineTests : DatabaseTestBase
{
    public ProcessEngineTests(DatabaseFixture fixture) : base(fixture) { }

    [Fact]
    public void ProcessEngine_CanBeCreated()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLogger = new Mock<ILogger<ProcessEngine>>();

        // Act
        var engine = new ProcessEngine(
            Context,
            mockServiceProvider.Object,
            mockLogger.Object);

        // Assert
        Assert.NotNull(engine);
    }

    [Fact]
    public void ProcessEngine_GetAvailableProcesses_ReturnsEmptyList()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLogger = new Mock<ILogger<ProcessEngine>>();

        var engine = new ProcessEngine(
            Context,
            mockServiceProvider.Object,
            mockLogger.Object);

        // Act
        var processes = engine.GetAvailableProcesses();

        // Assert
        Assert.NotNull(processes);
        Assert.Empty(processes);
    }
}