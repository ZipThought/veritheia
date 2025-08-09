var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL 17 with pgvector for journey projection spaces
var postgres = builder.AddPostgres("postgres")
    .WithImage("pgvector/pgvector", "pg17") // PostgreSQL 17 with pgvector
    .WithDataVolume("veritheia-postgres-data")
    .WithLifetime(ContainerLifetime.Persistent);

var veritheiaDb = postgres.AddDatabase("veritheiadb");

var apiService = builder.AddProject<Projects.veritheia_ApiService>("apiservice")
    .WithReference(veritheiaDb)
    .WaitFor(veritheiaDb);

builder.AddProject<Projects.veritheia_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
