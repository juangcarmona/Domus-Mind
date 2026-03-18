using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Family;
using DomusMind.Contracts.Family;
using DomusMind.Domain.Family;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Family.GetMyFamily;

public sealed class GetMyFamilyQueryHandler : IQueryHandler<GetMyFamilyQuery, FamilyResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IUserFamilyAccessReader _familyAccessReader;

    public GetMyFamilyQueryHandler(
        IDomusMindDbContext dbContext,
        IUserFamilyAccessReader familyAccessReader)
    {
        _dbContext = dbContext;
        _familyAccessReader = familyAccessReader;
    }

    public async Task<FamilyResponse> Handle(GetMyFamilyQuery query, CancellationToken cancellationToken)
    {
        var familyId = await _familyAccessReader.GetFamilyIdForUserAsync(
            query.RequestedByUserId, cancellationToken);

        if (!familyId.HasValue)
            throw new FamilyException(FamilyErrorCode.FamilyNotFound, "No family found for this user.");

        var family = await _dbContext.Set<Domain.Family.Family>()
            .AsNoTracking()
            .Include(f => f.Members)
            .SingleOrDefaultAsync(f => f.Id == FamilyId.From(familyId.Value), cancellationToken);

        if (family is null)
            throw new FamilyException(FamilyErrorCode.FamilyNotFound, "Family was not found.");

        return new FamilyResponse(
            family.Id.Value,
            family.Name.Value,
            family.PrimaryLanguageCode,
            family.CreatedAtUtc,
            family.Members.Count,
            family.FirstDayOfWeek,
            family.DateFormatPreference);
    }
}
