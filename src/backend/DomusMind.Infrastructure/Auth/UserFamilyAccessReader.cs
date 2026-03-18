using DomusMind.Application.Abstractions.Security;
using DomusMind.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Infrastructure.Auth;

public sealed class UserFamilyAccessReader : IUserFamilyAccessReader
{
    private readonly DomusMindDbContext _dbContext;

    public UserFamilyAccessReader(DomusMindDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid?> GetFamilyIdForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var access = await _dbContext.UserFamilyAccesses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);

        return access?.FamilyId;
    }
}
