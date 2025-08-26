using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Veritheia.Data;
using Veritheia.Data.Entities;
using Xunit;

namespace veritheia.Tests.TestBase;

public class DatabaseFixture : IAsyncLifetime
{
    private PostgreSqlContainer _container = null!;
    private Respawner _respawner = null!;
    private string _connectionString = null!;

    public string ConnectionString => _connectionString;

    public VeritheiaDbContext CreateContext()
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionString);
        dataSourceBuilder.EnableDynamicJson();
        dataSourceBuilder.UseVector();
        var dataSource = dataSourceBuilder.Build();

        var optionsBuilder = new DbContextOptionsBuilder<VeritheiaDbContext>();
        optionsBuilder.UseNpgsql(dataSource, o => o.UseVector())
            .UseSeeding((context, _) =>
            {
                SeedDemoData(context);
            })
            .UseAsyncSeeding(async (context, _, cancellationToken) =>
            {
                await SeedDemoDataAsync(context, cancellationToken);
            });
        return new VeritheiaDbContext(optionsBuilder.Options);
    }

    private void SeedDemoData(DbContext context)
    {
        var veritheiaContext = (VeritheiaDbContext)context;

        // Check if demo user already exists
        var demoUser = veritheiaContext.Users.FirstOrDefault(u => u.Email == "demo@veritheia.local");
        if (demoUser == null)
        {
            demoUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "demo@veritheia.local",
                DisplayName = "Dr. Sarah Chen",
                CreatedAt = DateTime.UtcNow,
                LastActiveAt = DateTime.UtcNow
            };
            veritheiaContext.Users.Add(demoUser);
            veritheiaContext.SaveChanges();

            // Create default personas
            CreateDefaultPersonas(veritheiaContext, demoUser.Id);
        }
    }

    private async Task SeedDemoDataAsync(DbContext context, CancellationToken cancellationToken)
    {
        var veritheiaContext = (VeritheiaDbContext)context;

        // Check if demo user already exists
        var demoUser = await veritheiaContext.Users.FirstOrDefaultAsync(u => u.Email == "demo@veritheia.local", cancellationToken);
        if (demoUser == null)
        {
            demoUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "demo@veritheia.local",
                DisplayName = "Dr. Sarah Chen",
                CreatedAt = DateTime.UtcNow,
                LastActiveAt = DateTime.UtcNow
            };
            veritheiaContext.Users.Add(demoUser);
            await veritheiaContext.SaveChangesAsync(cancellationToken);

            // Create default personas
            await CreateDefaultPersonasAsync(veritheiaContext, demoUser.Id, cancellationToken);
        }
    }

    private void CreateDefaultPersonas(VeritheiaDbContext context, Guid userId)
    {
        var personas = new[]
        {
            new Persona
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Domain = "Researcher",
                ConceptualVocabulary = new Dictionary<string, object>
                {
                    ["methodological_terms"] = new[] { "hypothesis", "methodology", "analysis", "validation", "peer_review" },
                    ["assessment_criteria"] = new[] { "rigor", "validity", "reliability", "reproducibility" }
                },
                MethodologicalPreferences = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["preferred_processes"] = new[] { "systematic-screening", "meta-analysis" },
                        ["documentation_style"] = "academic",
                        ["validation_approach"] = "peer_reviewed"
                    }
                },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Persona
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Domain = "Student",
                ConceptualVocabulary = new Dictionary<string, object>
                {
                    ["learning_terms"] = new[] { "concept", "understanding", "practice", "application", "synthesis" },
                    ["assessment_focus"] = new[] { "comprehension", "application", "critical_thinking" }
                },
                MethodologicalPreferences = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["preferred_processes"] = new[] { "basic-constrained-composition", "guided-exploration" },
                        ["documentation_style"] = "structured_learning",
                        ["validation_approach"] = "instructor_feedback"
                    }
                },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Persona
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Domain = "Entrepreneur",
                ConceptualVocabulary = new Dictionary<string, object>
                {
                    ["business_terms"] = new[] { "opportunity", "market", "value_proposition", "execution", "scalability" },
                    ["assessment_focus"] = new[] { "viability", "market_fit", "competitive_advantage" }
                },
                MethodologicalPreferences = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["preferred_processes"] = new[] { "rapid-prototyping", "market-validation" },
                        ["documentation_style"] = "business_oriented",
                        ["validation_approach"] = "market_feedback"
                    }
                },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Personas.AddRange(personas);
        context.SaveChanges();
    }

    private async Task CreateDefaultPersonasAsync(VeritheiaDbContext context, Guid userId, CancellationToken cancellationToken)
    {
        var personas = new[]
        {
            new Persona
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Domain = "Researcher",
                ConceptualVocabulary = new Dictionary<string, object>
                {
                    ["methodological_terms"] = new[] { "hypothesis", "methodology", "analysis", "validation", "peer_review" },
                    ["assessment_criteria"] = new[] { "rigor", "validity", "reliability", "reproducibility" }
                },
                MethodologicalPreferences = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["preferred_processes"] = new[] { "systematic-screening", "meta-analysis" },
                        ["documentation_style"] = "academic",
                        ["validation_approach"] = "peer_reviewed"
                    }
                },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Persona
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Domain = "Student",
                ConceptualVocabulary = new Dictionary<string, object>
                {
                    ["learning_terms"] = new[] { "concept", "understanding", "practice", "application", "synthesis" },
                    ["assessment_focus"] = new[] { "comprehension", "application", "critical_thinking" }
                },
                MethodologicalPreferences = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["preferred_processes"] = new[] { "basic-constrained-composition", "guided-exploration" },
                        ["documentation_style"] = "structured_learning",
                        ["validation_approach"] = "instructor_feedback"
                    }
                },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Persona
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Domain = "Entrepreneur",
                ConceptualVocabulary = new Dictionary<string, object>
                {
                    ["business_terms"] = new[] { "opportunity", "market", "value_proposition", "execution", "scalability" },
                    ["assessment_focus"] = new[] { "viability", "market_fit", "competitive_advantage" }
                },
                MethodologicalPreferences = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["preferred_processes"] = new[] { "rapid-prototyping", "market-validation" },
                        ["documentation_style"] = "business_oriented",
                        ["validation_approach"] = "market_feedback"
                    }
                },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Personas.AddRange(personas);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task InitializeAsync()
    {
        // Start PostgreSQL container with pgvector
        _container = new PostgreSqlBuilder()
            .WithImage("pgvector/pgvector:pg17")
            .WithDatabase("veritheia_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        await _container.StartAsync();

        _connectionString = _container.GetConnectionString();

        // Apply migrations to create schema
        using var context = CreateContext();
        await context.Database.MigrateAsync();

        // Setup Respawn for fast data reset
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = ["__EFMigrationsHistory"]
        });
    }

    public async Task ResetAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);

        // Re-seed demo data after reset
        using var context = CreateContext();
        await SeedDemoDataAsync(context, CancellationToken.None);
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}