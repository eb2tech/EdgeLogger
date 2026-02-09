var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.EdgeLogger_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.EdgeLogger_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

// MAUI Mobile App - References API service for backend connectivity
builder.AddProject<Projects.EdgeLogger_Mobile>("edgelogger-mobile")
    .WithReference(apiService);

builder.Build().Run();
