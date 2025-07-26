var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.veritheia_ApiService>("apiservice");

builder.AddProject<Projects.veritheia_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
