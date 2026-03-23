using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Family;
using DomusMind.Domain.Areas;
using DomusMind.Domain.Family;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Family.GetHouseholdAreas;

public sealed class GetHouseholdAreasQueryHandler
    : IQueryHandler<GetHouseholdAreasQuery, HouseholdAreasResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public GetHouseholdAreasQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<HouseholdAreasResponse> Handle(
        GetHouseholdAreasQuery query,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, query.FamilyId, cancellationToken);
        if (!canAccess)
            throw new FamilyException(FamilyErrorCode.AccessDenied, "Access to this family is denied.");

        var familyId = FamilyId.From(query.FamilyId);

        var familyExists = await _dbContext.Set<Domain.Family.Family>()
            .AsNoTracking()
            .AnyAsync(f => f.Id == familyId, cancellationToken);

        if (!familyExists)
            throw new FamilyException(FamilyErrorCode.FamilyNotFound, "Family not found.");

        var areas = await _dbContext.Set<Area>()
            .AsNoTracking()
            .Where(area => area.FamilyId == familyId)
            .OrderBy(area => area.Name)
            .Select(area => new HouseholdAreaItem(
                area.Id,
                area.Name,
                area.Color))
            .ToListAsync(cancellationToken);

        return new HouseholdAreasResponse(areas);
    }
}
