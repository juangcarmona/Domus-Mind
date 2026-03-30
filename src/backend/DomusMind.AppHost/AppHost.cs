var builder = DistributedApplication.CreateBuilder(args);

var isCloudHostedLocal = builder.Configuration["DomusMind:LocalMode"] == "CloudHosted";

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(isCloudHostedLocal ? 5051 : 5050))
    .WithDataVolume(isCloudHostedLocal ? "domusmind-cloudhosted-local" : "domusmind-data");

var domusmindDb = postgres.AddDatabase("domusmind");

var api = builder.AddProject<Projects.DomusMind_Api>("api")
    .WithReference(domusmindDb)
    .WaitFor(domusmindDb);

if (isCloudHostedLocal)
{
    api.WithEnvironment("Deployment__Mode", "CloudHosted")
       .WithEnvironment("Deployment__AllowHouseholdCreation", "false")
       .WithEnvironment("Deployment__InvitationsEnabled", "true")
       .WithEnvironment("Deployment__RequireInvitationForSignup", "true")
       .WithEnvironment("Deployment__AdminToolsEnabled", "true")
       .WithEnvironment("Deployment__MaxHouseholdsPerDeployment", "0")
       .WithEnvironment("BootstrapAdmin__Enabled", "true")
       .WithEnvironment("BootstrapAdmin__Email", "admin@domusmind.local")
       .WithEnvironment("BootstrapAdmin__Password", "ChangeMeNow123!")
       .WithEnvironment("BootstrapAdmin__DisplayName", "DomusMind Admin");
}

builder.AddViteApp("web-app", "../../web/app")
    .WithReference(api)
    .WaitFor(api)
    .WithEnvironment("PORT", "3000");

builder.Build().Run();