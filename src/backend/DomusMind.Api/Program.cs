using DomusMind.Api.OpenApi;
using DomusMind.Application.DependencyInjection;
using DomusMind.Infrastructure.Auth;
using DomusMind.Infrastructure.DependencyInjection;
using DomusMind.Infrastructure.Languages;
using DomusMind.Infrastructure.Messaging;
using DomusMind.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddDomusMindAuthentication(builder.Configuration);
builder.Services.AddDomusMindAuthorization();
builder.Services.AddDomusMindOpenApi();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionFeature?.Error;
        var logger = context.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("GlobalExceptionHandler");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        if (exception is HandlerResolutionException resolutionException)
        {
            logger.LogError(
                resolutionException,
                "Dispatcher handler resolution failed for {HandlerType}",
                resolutionException.HandlerType);

            await context.Response.WriteAsJsonAsync(new
            {
                code = "handler_resolution_failed",
                message = "An internal handler registration is missing.",
                traceId = context.TraceIdentifier,
            });
            return;
        }

        logger.LogError(exception, "Unhandled server exception");
        await context.Response.WriteAsJsonAsync(new
        {
            code = "internal_server_error",
            message = "An unexpected error occurred.",
            traceId = context.TraceIdentifier,
        });
    });
});

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
