using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Abstractions.System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DomusMind.Infrastructure.Auth;

/// <summary>
/// Fallback bootstrap path: seeds the initial admin when BootstrapAdmin is explicitly enabled
/// and the system has not yet been initialized via the setup endpoint.
/// This is intended for headless or recovery deployments only.
/// The normal first-run path is POST /api/setup/initialize.
/// </summary>
public sealed class AuthSeedService
{
    public static async Task SeedAdminAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var options = sp.GetRequiredService<IOptions<BootstrapAdminOptions>>().Value;
        var logger = sp.GetRequiredService<ILogger<AuthSeedService>>();

        if (!options.Enabled)
        {
            logger.LogDebug("Auth bootstrap fallback is disabled. Skipping.");
            return;
        }

        var initState = sp.GetRequiredService<ISystemInitializationState>();

        if (await initState.IsInitializedAsync(cancellationToken))
        {
            logger.LogInformation("Auth bootstrap fallback skipped: system is already initialized.");
            return;
        }

        var repository = sp.GetRequiredService<IAuthUserRepository>();
        var hasher = sp.GetRequiredService<IPasswordHasher>();

        var email = options.Email.Trim().ToLowerInvariant();
        var user = new AuthUserRecord(Guid.NewGuid(), email, hasher.Hash(options.Password));

        await repository.AddAsync(user, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await initState.MarkInitializedAsync(cancellationToken);

        logger.LogInformation("Bootstrap fallback: admin user created for {Email}. System marked as initialized.", email);
    }
}
