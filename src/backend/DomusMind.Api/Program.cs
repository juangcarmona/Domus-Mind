using DomusMind.Api.OpenApi;
using DomusMind.Application.DependencyInjection;
using DomusMind.Infrastructure.Auth;
using DomusMind.Infrastructure.DependencyInjection;
using DomusMind.Infrastructure.Languages;
using DomusMind.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddDomusMindAuthentication(builder.Configuration);
builder.Services.AddDomusMindAuthorization();
builder.Services.AddDomusMindOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DomusMindDbContext>();
    await dbContext.Database.MigrateAsync();
}
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
await AuthSeedService.SeedAdminAsync(app.Services, CancellationToken.None);
await LanguageSeedService.SeedAsync(app.Services, CancellationToken.None);

app.Run();


