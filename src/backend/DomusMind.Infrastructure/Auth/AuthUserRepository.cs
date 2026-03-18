using DomusMind.Application.Abstractions.Security;
using DomusMind.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Infrastructure.Auth;

public sealed class AuthUserRepository : IAuthUserRepository
{
    private readonly DomusMindDbContext _db;

    public AuthUserRepository(DomusMindDbContext db)
    {
        _db = db;
    }

    public async Task<AuthUserRecord?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var entity = await _db.Set<AuthUser>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        return entity is null ? null : Map(entity);
    }

    public async Task<AuthUserRecord?> FindByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var entity = await _db.Set<AuthUser>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        return entity is null ? null : Map(entity);
    }

    public async Task AddAsync(AuthUserRecord user, CancellationToken cancellationToken)
    {
        var entity = new AuthUser
        {
            UserId = user.UserId,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            CreatedAtUtc = DateTime.UtcNow,
            MustChangePassword = user.MustChangePassword,
        };

        await _db.Set<AuthUser>().AddAsync(entity, cancellationToken);
    }

    public async Task UpdatePasswordHashAsync(Guid userId, string newPasswordHash, CancellationToken cancellationToken)
    {
        await _db.Set<AuthUser>()
            .Where(u => u.UserId == userId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(u => u.PasswordHash, newPasswordHash),
                cancellationToken);
    }

    public async Task UpdateMustChangePasswordAsync(Guid userId, bool mustChangePassword, CancellationToken cancellationToken)
    {
        await _db.Set<AuthUser>()
            .Where(u => u.UserId == userId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(u => u.MustChangePassword, mustChangePassword),
                cancellationToken);
    }

    public async Task<bool> AnyUsersAsync(CancellationToken cancellationToken)
    {
        return await _db.Set<AuthUser>().AnyAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _db.SaveChangesAsync(cancellationToken);

    private static AuthUserRecord Map(AuthUser entity)
        => new(entity.UserId, entity.Email, entity.PasswordHash, entity.MustChangePassword);
}
