using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Veritheia.Data;
using Xunit;

namespace veritheia.Tests.TestBase;

public class DatabaseFixture : IAsyncLifetime
{
    private PostgreSqlContainer _container = null!;
    private Respawner _respawner = null!;
    private string _connectionString = null!;
    
    public VeritheiaDbContext CreateContext()
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionString);
        dataSourceBuilder.EnableDynamicJson();
        dataSourceBuilder.UseVector();
        var dataSource = dataSourceBuilder.Build();
        
        var optionsBuilder = new DbContextOptionsBuilder<VeritheiaDbContext>();
        optionsBuilder.UseNpgsql(dataSource, o => o.UseVector());
        return new VeritheiaDbContext(optionsBuilder.Options);
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
    }
    
    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}