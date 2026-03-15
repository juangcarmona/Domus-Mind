using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Infrastructure.Persistence;

namespace DomusMind.Infrastructure.Auth;

public sealed class FamilyAccessGranter : IFamilyAccessGranter
{
    private readonly IDomusMindDbContext _dbContext;

    public FamilyAccessGranter(IDomusMindDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task GrantAccessAsync(Guid userId, Guid familyId, CancellationToken cancellationToken = default)
    {
        var access = new UserFamilyAccess
        {
            UserId = userId,
            FamilyId = familyId,
            GrantedAtUtc = DateTime.UtcNow,
        };

        _dbContext.Set<UserFamilyAccess>().Add(access);

        // SaveChangesAsync is the caller's responsibility (typically via EventLogWriter).
        return Task.CompletedTask;
    }
}
