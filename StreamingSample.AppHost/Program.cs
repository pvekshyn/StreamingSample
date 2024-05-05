var builder = DistributedApplication.CreateBuilder(args);

var sourceService = builder.AddProject<Projects.SourceUserService>("sourceUserService");
builder.AddProject<Projects.TargetService>("targetService")
    .WithReference(sourceService);

builder.Build().Run();
