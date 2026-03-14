var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();

var domusmindDb = postgres.AddDatabase("domusmind");

builder.AddProject<Projects.DomusMind_Api>("api")
    .WithReference(domusmindDb)
    .WaitFor(domusmindDb);

builder.Build().Run();