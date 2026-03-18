namespace DomusMind.Application.Abstractions.Security;

public interface IAuthUserRepository
{
    Task<AuthUserRecord?> FindByEmailAsync(string email, CancellationToken cancellationToken);

    Task<AuthUserRecord?> FindByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task AddAsync(AuthUserRecord user, CancellationToken cancellationToken);

    Task UpdatePasswordHashAsync(Guid userId, string newPasswordHash, CancellationToken cancellationToken);

    Task UpdateMustChangePasswordAsync(Guid userId, bool mustChangePassword, CancellationToken cancellationToken);

    Task<bool> AnyUsersAsync(CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
