var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Backend_API>("backendapi1");

builder.Build().Run();
