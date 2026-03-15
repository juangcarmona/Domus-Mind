using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Infrastructure.Auth;

public sealed class FamilyAuthorizationService : IFamilyAuthorizationService
{
    private readonly IDomusMindDbContext _dbContext;

    public FamilyAuthorizationService(IDomusMindDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> CanAccessFamilyAsync(
        Guid userId,
        Guid familyId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<UserFamilyAccess>()
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.FamilyId == familyId, cancellationToken);
    }
}

