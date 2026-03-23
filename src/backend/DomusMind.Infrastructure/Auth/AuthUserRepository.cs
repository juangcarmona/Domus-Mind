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

    public async Task<IReadOnlyDictionary<Guid, AuthUserStatusProjection>> GetStatusByIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
            return new Dictionary<Guid, AuthUserStatusProjection>();

        var result = await _db.Set<AuthUser>()
            .AsNoTracking()
            .Where(u => userIds.Contains(u.UserId))
            .Select(u => new AuthUserStatusProjection(u.UserId, u.Email, u.IsDisabled, u.MustChangePassword, u.LastLoginAtUtc))
            .ToListAsync(cancellationToken);

        return result.ToDictionary(p => p.UserId);
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
            DisplayName = user.DisplayName,
            IsDisabled = user.IsDisabled,
            MemberId = user.MemberId,
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

    public async Task UpdatePasswordChangedAtAsync(Guid userId, DateTime changedAtUtc, CancellationToken cancellationToken)
    {
        await _db.Set<AuthUser>()
            .Where(u => u.UserId == userId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(u => u.PasswordChangedAtUtc, changedAtUtc),
                cancellationToken);
    }

    public async Task DisableUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        await _db.Set<AuthUser>()
            .Where(u => u.UserId == userId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(u => u.IsDisabled, true),
                cancellationToken);
    }

    public async Task EnableUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        await _db.Set<AuthUser>()
            .Where(u => u.UserId == userId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(u => u.IsDisabled, false),
                cancellationToken);
    }

    public async Task UpdateLastLoginAtAsync(Guid userId, DateTime lastLoginAtUtc, CancellationToken cancellationToken)
    {
        await _db.Set<AuthUser>()
            .Where(u => u.UserId == userId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(u => u.LastLoginAtUtc, lastLoginAtUtc),
                cancellationToken);
    }

    public async Task<bool> AnyUsersAsync(CancellationToken cancellationToken)
    {
        return await _db.Set<AuthUser>().AnyAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _db.SaveChangesAsync(cancellationToken);

    private static AuthUserRecord Map(AuthUser entity)
        => new(
            entity.UserId,
            entity.Email,
            entity.PasswordHash,
            entity.MustChangePassword,
            entity.DisplayName,
            entity.IsDisabled,
            entity.MemberId);
}
