namespace DomusMind.Application.Abstractions.Security;

/// <summary>Status projection used for displaying member access state without loading full auth records.</summary>
public sealed record AuthUserStatusProjection(
    Guid UserId,
    string Email,
    bool IsDisabled,
    bool MustChangePassword,
    DateTime? LastLoginAtUtc);

public interface IAuthUserRepository
{
    Task<AuthUserRecord?> FindByEmailAsync(string email, CancellationToken cancellationToken);

    Task<AuthUserRecord?> FindByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, AuthUserStatusProjection>> GetStatusByIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken);

    Task AddAsync(AuthUserRecord user, CancellationToken cancellationToken);

    Task UpdatePasswordHashAsync(Guid userId, string newPasswordHash, CancellationToken cancellationToken);

    Task UpdateMustChangePasswordAsync(Guid userId, bool mustChangePassword, CancellationToken cancellationToken);

    Task UpdatePasswordChangedAtAsync(Guid userId, DateTime changedAtUtc, CancellationToken cancellationToken);

    Task DisableUserAsync(Guid userId, CancellationToken cancellationToken);

    Task EnableUserAsync(Guid userId, CancellationToken cancellationToken);

    Task UpdateLastLoginAtAsync(Guid userId, DateTime lastLoginAtUtc, CancellationToken cancellationToken);

    Task<bool> AnyUsersAsync(CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
