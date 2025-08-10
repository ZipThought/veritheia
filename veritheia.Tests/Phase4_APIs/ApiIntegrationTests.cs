using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Veritheia.Data;
using Veritheia.Data.Entities;
using veritheia.Tests.TestBase;
using Xunit;

namespace Veritheia.Tests.Phase4_APIs;

/// <summary>
/// Integration tests for Phase 4 Knowledge Database APIs
/// Validates that API endpoints actually work end-to-end
/// </summary>
[Collection("DatabaseTests")]
[Trait("Category", "Integration")]
public class ApiIntegrationTests : DatabaseTestBase
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public ApiIntegrationTests(DatabaseFixture fixture) : base(fixture)
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Use test database
                    services.AddSingleton<VeritheiaDbContext>(Context);
                });
            });
        _client = _factory.CreateClient();
    }
    
    [Fact]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/api/health");
        var content = await response.Content.ReadAsStringAsync();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Healthy", content);
        Assert.Contains("Veritheia API Service", content);
    }
    
    [Fact]
    public async Task UserEndpoints_CreateAndRetrieveUser()
    {
        // Arrange & Act - Get or create default user
        var response = await _client.GetAsync("/api/users/current");
        var content = await response.Content.ReadAsStringAsync();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = JsonSerializer.Deserialize<User>(content, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });
        
        Assert.NotNull(user);
        Assert.NotEqual(Guid.Empty, user.Id);
        
        // Act - Get specific user
        var getUserResponse = await _client.GetAsync($"/api/users/{user.Id}");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, getUserResponse.StatusCode);
    }
    
    [Fact]
    public async Task PersonaEndpoints_CreateAndListPersonas()
    {
        // Arrange
        var personaData = new
        {
            name = "Test Researcher",
            perspective = "Academic research focus",
            vocabulary = new[] { "epistemic", "ontological" },
            patterns = new[] { "evidence-based", "peer-reviewed" },
            context = "Research methodology"
        };
        
        var json = JsonSerializer.Serialize(personaData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // Act - Create persona
        var response = await _client.PostAsync("/api/personas", content);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var persona = JsonSerializer.Deserialize<Persona>(responseContent, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });
        
        Assert.NotNull(persona);
        Assert.Equal("Test Researcher", persona.Name);
        Assert.NotEqual(Guid.Empty, persona.Id);
        
        // Act - List personas
        var listResponse = await _client.GetAsync("/api/personas");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var listContent = await listResponse.Content.ReadAsStringAsync();
        Assert.Contains("Test Researcher", listContent);
    }
    
    [Fact]
    public async Task JourneyEndpoints_FullJourneyLifecycle()
    {
        // Arrange - Create user and persona first
        var userResponse = await _client.GetAsync("/api/users/current");
        var userContent = await userResponse.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<User>(userContent, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });
        
        var personaData = new
        {
            name = "Journey Test Persona",
            perspective = "Testing journey creation"
        };
        
        var personaJson = JsonSerializer.Serialize(personaData);
        var personaContent = new StringContent(personaJson, Encoding.UTF8, "application/json");
        var personaResponse = await _client.PostAsync("/api/personas", personaContent);
        var personaResponseContent = await personaResponse.Content.ReadAsStringAsync();
        var persona = JsonSerializer.Deserialize<Persona>(personaResponseContent, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });
        
        // Act - Create journey
        var journeyData = new
        {
            userId = user.Id,
            personaId = persona.Id,
            purpose = "Test journey for API validation"
        };
        
        var journeyJson = JsonSerializer.Serialize(journeyData);
        var journeyContent = new StringContent(journeyJson, Encoding.UTF8, "application/json");
        var journeyResponse = await _client.PostAsync("/api/journeys", journeyContent);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, journeyResponse.StatusCode);
        
        var journeyResponseContent = await journeyResponse.Content.ReadAsStringAsync();
        var journey = JsonSerializer.Deserialize<Journey>(journeyResponseContent, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });
        
        Assert.NotNull(journey);
        Assert.NotEqual(Guid.Empty, journey.Id);
        Assert.Equal("Test journey for API validation", journey.Purpose);
        
        // Act - Get journey details
        var getResponse = await _client.GetAsync($"/api/journeys/{journey.Id}");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        // Act - Get active journeys
        var activeResponse = await _client.GetAsync($"/api/journeys/user/{user.Id}/active");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, activeResponse.StatusCode);
        var activeContent = await activeResponse.Content.ReadAsStringAsync();
        Assert.Contains(journey.Id.ToString(), activeContent);
        
        // Act - Archive journey
        var archiveResponse = await _client.PostAsync($"/api/journeys/{journey.Id}/archive", null);
        
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, archiveResponse.StatusCode);
    }
    
    [Fact]
    public async Task SearchEndpoint_ReturnsResults()
    {
        // Arrange - Ensure we have some data
        await SeedTestData();
        
        // Act
        var response = await _client.GetAsync("/api/search?query=test&limit=10");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
        // Should return array even if empty
        Assert.StartsWith("[", content.Trim());
    }
    
    [Fact]
    public async Task ProcessEndpoints_ListAvailableProcesses()
    {
        // Act
        var response = await _client.GetAsync("/api/processes");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        
        // Should list our two implemented processes
        Assert.Contains("BasicSystematicScreeningProcess", content);
        Assert.Contains("BasicConstrainedCompositionProcess", content);
    }
    
    [Fact]
    public async Task DocumentEndpoints_ListDocuments()
    {
        // Act
        var response = await _client.GetAsync("/api/documents");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
        // Should return array even if empty
        Assert.StartsWith("[", content.Trim());
    }
    
    private async Task SeedTestData()
    {
        // Add a test document if needed
        var document = new Document
        {
            Id = Guid.NewGuid(),
            Title = "Test Document",
            OriginalName = "test.txt",
            MimeType = "text/plain",
            FileHash = "testhash",
            FilePath = "/test/path",
            Size = 100,
            UploadedAt = DateTime.UtcNow,
            LastAccessedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>()
        };
        
        Context.Documents.Add(document);
        await Context.SaveChangesAsync();
    }
    
    public override async Task DisposeAsync()
    {
        _client?.Dispose();
        _factory?.Dispose();
        await base.DisposeAsync();
    }
}