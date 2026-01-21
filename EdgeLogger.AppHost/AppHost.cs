var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.EdgeLogger_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.EdgeLogger_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.AddProject<Projects.EdgeLogger_Mobile>("edgelogger-mobile");

builder.Build().Run();
