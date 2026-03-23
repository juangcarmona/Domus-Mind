using DomusMind.Application.Abstractions.Security;

namespace DomusMind.Application.Tests.Features.Auth;

/// <summary>Shared test stubs for auth handler tests.</summary>
internal sealed class InMemoryAuthUserRepository : IAuthUserRepository
{
    public readonly List<AuthUserRecord> Users;

    public InMemoryAuthUserRepository(IEnumerable<AuthUserRecord>? seed = null)
    {
        Users = seed?.ToList() ?? [];
    }

    public Task<AuthUserRecord?> FindByEmailAsync(string email, CancellationToken ct)
        => Task.FromResult(Users.FirstOrDefault(u => u.Email == email));

    public Task<AuthUserRecord?> FindByIdAsync(Guid userId, CancellationToken ct)
        => Task.FromResult(Users.FirstOrDefault(u => u.UserId == userId));

    public Task AddAsync(AuthUserRecord user, CancellationToken ct)
    {
        Users.Add(user);
        return Task.CompletedTask;
    }

    public Task UpdatePasswordHashAsync(Guid userId, string newPasswordHash, CancellationToken ct)
    {
        var index = Users.FindIndex(u => u.UserId == userId);
        if (index >= 0)
            Users[index] = Users[index] with { PasswordHash = newPasswordHash };
        return Task.CompletedTask;
    }

    public Task UpdateMustChangePasswordAsync(Guid userId, bool mustChangePassword, CancellationToken ct)
    {
        var index = Users.FindIndex(u => u.UserId == userId);
        if (index >= 0)
            Users[index] = Users[index] with { MustChangePassword = mustChangePassword };
        return Task.CompletedTask;
    }

    public Task UpdatePasswordChangedAtAsync(Guid userId, DateTime changedAtUtc, CancellationToken ct)
    {
        // no-op in test stub
        return Task.CompletedTask;
    }

    public Task DisableUserAsync(Guid userId, CancellationToken ct)
    {
        var index = Users.FindIndex(u => u.UserId == userId);
        if (index >= 0)
            Users[index] = Users[index] with { IsDisabled = true };
        return Task.CompletedTask;
    }

    public Task EnableUserAsync(Guid userId, CancellationToken ct)
    {
        var index = Users.FindIndex(u => u.UserId == userId);
        if (index >= 0)
            Users[index] = Users[index] with { IsDisabled = false };
        return Task.CompletedTask;
    }

    public Task UpdateLastLoginAtAsync(Guid userId, DateTime lastLoginAtUtc, CancellationToken ct)
    {
        // no-op in test stub (LastLoginAtUtc not tracked in AuthUserRecord)
        return Task.CompletedTask;
    }

    public Task<IReadOnlyDictionary<Guid, AuthUserStatusProjection>> GetStatusByIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken ct)
    {
        IReadOnlyDictionary<Guid, AuthUserStatusProjection> result = Users
            .Where(u => userIds.Contains(u.UserId))
            .ToDictionary(
                u => u.UserId,
                u => new AuthUserStatusProjection(u.UserId, u.Email, u.IsDisabled, u.MustChangePassword, null));
        return Task.FromResult(result);
    }

    public Task<bool> AnyUsersAsync(CancellationToken ct)
        => Task.FromResult(Users.Count > 0);

    public Task SaveChangesAsync(CancellationToken ct)
        => Task.CompletedTask;
}

internal sealed class StubPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => $"HASHED:{password}";
    public bool Verify(string password, string hash) => hash == $"HASHED:{password}";
}

internal sealed class StubRefreshTokenService : IRefreshTokenService
{
    public Task<string> CreateAsync(Guid userId, CancellationToken ct)
        => Task.FromResult($"refresh-token-for-{userId}");

    public Task<RotateRefreshTokenResult> ValidateAndRotateAsync(string token, CancellationToken ct)
        => Task.FromResult(new RotateRefreshTokenResult(false, null, null, null));

    public Task RevokeAsync(string token, CancellationToken ct)
        => Task.CompletedTask;

    public Task RevokeAllForUserAsync(Guid userId, CancellationToken ct)
        => Task.CompletedTask;
}

internal sealed class StubAccessTokenGenerator : IAccessTokenGenerator
{
    public string Generate(Guid userId, string email, IReadOnlyCollection<string>? roles = null)
        => $"access-token-for-{userId}";
}
