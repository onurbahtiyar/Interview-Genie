var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects>("backendapi1");

builder.Build().Run();
