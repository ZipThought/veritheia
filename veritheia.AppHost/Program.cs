var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL 17 with pgvector for journey projection spaces
var postgres = builder.AddPostgres("postgres")
    .WithImage("pgvector/pgvector", "pg17") // PostgreSQL 17 with pgvector
    .WithDataVolume("veritheia-postgres-data")
    .WithLifetime(ContainerLifetime.Persistent);

var veritheiaDb = postgres.AddDatabase("veritheiadb");

builder.AddProject<Projects.veritheia_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(veritheiaDb)
    .WaitFor(veritheiaDb);

builder.Build().Run();
