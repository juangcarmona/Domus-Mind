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
            logger.LogDebug(
                "Bootstrap admin: Enabled={BootstrapAdminEnabled}. Skipping — bootstrap is disabled.",
                options.Enabled);
            return;
        }

        var initState = sp.GetRequiredService<ISystemInitializationState>();

        if (await initState.IsInitializedAsync(cancellationToken))
        {
            logger.LogInformation(
                "Bootstrap admin seeding skipped: system already initialized. BootstrapAdminEnabled={BootstrapAdminEnabled} EmailConfigured={EmailConfigured}",
                options.Enabled,
                !string.IsNullOrWhiteSpace(options.Email));
            return;
        }

        var repository = sp.GetRequiredService<IAuthUserRepository>();
        var hasher = sp.GetRequiredService<IPasswordHasher>();

        var email = options.Email.Trim().ToLowerInvariant();

        var existing = await repository.FindByEmailAsync(email, cancellationToken);
        if (existing is not null)
        {
            // User already exists (e.g. created in a previous partial run before MarkInitialized could be called).
            // Reconcile state: mark the system initialized so subsequent restarts are consistent.
            if (!existing.IsOperator)
                logger.LogWarning(
                    "Bootstrap admin: user {Email} already exists but does not have the Operator flag. Manual correction may be required.",
                    email);

            await initState.MarkInitializedAsync(cancellationToken);
            logger.LogInformation(
                "Bootstrap admin: user {Email} already exists. Marking system as initialized.",
                email);
            return;
        }

        var user = new AuthUserRecord(Guid.NewGuid(), email, hasher.Hash(options.Password), IsOperator: true);

        await repository.AddAsync(user, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await initState.MarkInitializedAsync(cancellationToken);

        logger.LogInformation(
            "Bootstrap admin: operator account seeded. Email={Email} IsInitialized=true",
            email);
    }
}
