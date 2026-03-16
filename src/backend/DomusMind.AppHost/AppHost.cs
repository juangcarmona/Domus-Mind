var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(5050))
    .WithDataVolume();

var domusmindDb = postgres.AddDatabase("domusmind");

var api = builder.AddProject<Projects.DomusMind_Api>("api")
    .WithReference(domusmindDb)
    .WaitFor(domusmindDb);

builder.AddViteApp("web-app", "../../web/app")
    .WithReference(api)
    .WaitFor(api)    
    .WithEnvironment("PORT", "3000");

builder.Build().Run();