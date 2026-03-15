using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Responsibilities;
using DomusMind.Domain.Family;
using DomusMind.Domain.Responsibilities;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Responsibilities.GetHouseholdAreas;

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
            throw new ResponsibilitiesException(ResponsibilitiesErrorCode.AccessDenied, "Access to this family is denied.");

        var familyId = FamilyId.From(query.FamilyId);

        var family = await _dbContext.Set<Domain.Family.Family>()
            .AsNoTracking()
            .Include(f => f.Members)
            .SingleOrDefaultAsync(f => f.Id == familyId, cancellationToken);

        if (family is null)
            throw new ResponsibilitiesException(ResponsibilitiesErrorCode.AccessDenied, "Family not found.");

        var domains = await _dbContext.Set<ResponsibilityDomain>()
            .AsNoTracking()
            .Where(d => d.FamilyId == familyId)
            .ToListAsync(cancellationToken);

        var memberNameById = family.Members.ToDictionary(m => m.Id.Value, m => m.Name.Value);

        var areas = domains
            .OrderBy(d => d.Name.Value)
            .Select(d => new HouseholdAreaItem(
                d.Id.Value,
                d.Name.Value,
                d.PrimaryOwnerId?.Value,
                d.PrimaryOwnerId.HasValue && memberNameById.TryGetValue(d.PrimaryOwnerId.Value.Value, out var name)
                    ? name
                    : null,
                d.SecondaryOwnerIds.Select(s => s.Value).ToList()))
            .ToList();

        return new HouseholdAreasResponse(areas);
    }
}
