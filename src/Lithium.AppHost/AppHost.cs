var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("env");

var server = builder.AddProject<Projects.Lithium_Server>("server");

builder.AddProject<Projects.Lithium_Client>("client")
    .WithReference(server);

builder.AddProject<Projects.Lithium_Server_Dashboard>("dashboard")
    .WithReference(server);

builder.Build().Run();
