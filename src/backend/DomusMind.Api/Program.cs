using DomusMind.Api.OpenApi;
using DomusMind.Application.DependencyInjection;
using DomusMind.Infrastructure.Auth;
using DomusMind.Infrastructure.DependencyInjection;
using DomusMind.Infrastructure.Languages;
using DomusMind.Infrastructure.Persistence;
using DomusMind.Infrastructure.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddDomusMindAuthentication(builder.Configuration);
builder.Services.AddDomusMindAuthorization();
builder.Services.AddDomusMindOpenApi();
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

// Log effective deployment policy at startup — observable in logs and App Insights.
var deploymentSettings = app.Services.GetRequiredService<DeploymentSettings>();
var bootstrapOptions = app.Services.GetRequiredService<IOptions<BootstrapAdminOptions>>().Value;
app.Logger.LogInformation(
    "DomusMind startup: Mode={DeploymentMode} AllowHouseholdCreation={AllowHouseholdCreation} InvitationsEnabled={InvitationsEnabled} RequireInvitationForSignup={RequireInvitationForSignup} AdminToolsEnabled={AdminToolsEnabled} MaxHouseholdsPerDeployment={MaxHouseholdsPerDeployment} BootstrapAdminEnabled={BootstrapAdminEnabled} BootstrapAdminEmailConfigured={BootstrapAdminEmailConfigured}",
    deploymentSettings.Mode,
    deploymentSettings.AllowHouseholdCreation,
    deploymentSettings.InvitationsEnabled,
    deploymentSettings.RequireInvitationForSignup,
    deploymentSettings.AdminToolsEnabled,
    deploymentSettings.MaxHouseholdsPerDeployment,
    bootstrapOptions.Enabled,
    !string.IsNullOrWhiteSpace(bootstrapOptions.Email));

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

var webRootPath = app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");
if (Directory.Exists(webRootPath))
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

app.MapControllers();
var spaIndexPath = Path.Combine(webRootPath, "index.html");
if (File.Exists(spaIndexPath))
{
    app.MapFallback(async context =>
    {
        if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase) ||
            context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.SendFileAsync(spaIndexPath);
    });
}
else
{
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DomusMindDbContext>();
    await dbContext.Database.MigrateAsync();
}
await AuthSeedService.SeedAdminAsync(app.Services, CancellationToken.None);
await LanguageSeedService.SeedAsync(app.Services, CancellationToken.None);

app.Run();
