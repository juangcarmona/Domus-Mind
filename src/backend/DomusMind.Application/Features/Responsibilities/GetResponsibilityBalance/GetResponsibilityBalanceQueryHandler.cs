using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Responsibilities;
using DomusMind.Domain.Family;
using DomusMind.Domain.Responsibilities;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Responsibilities.GetResponsibilityBalance;

public sealed class GetResponsibilityBalanceQueryHandler
    : IQueryHandler<GetResponsibilityBalanceQuery, ResponsibilityBalanceResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public GetResponsibilityBalanceQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<ResponsibilityBalanceResponse> Handle(
        GetResponsibilityBalanceQuery query,
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

        var primaryCounts = new Dictionary<Guid, int>();
        var secondaryCounts = new Dictionary<Guid, int>();

        foreach (var member in family.Members)
        {
            primaryCounts[member.Id.Value] = 0;
            secondaryCounts[member.Id.Value] = 0;
        }

        foreach (var domain in domains)
        {
            if (domain.PrimaryOwnerId.HasValue)
            {
                var id = domain.PrimaryOwnerId.Value.Value;
                if (primaryCounts.ContainsKey(id))
                    primaryCounts[id]++;
            }

            foreach (var secondary in domain.SecondaryOwnerIds)
            {
                var id = secondary.Value;
                if (secondaryCounts.ContainsKey(id))
                    secondaryCounts[id]++;
            }
        }

        var loads = family.Members
            .OrderBy(m => m.Name.Value)
            .Select(m =>
            {
                var primary = primaryCounts.GetValueOrDefault(m.Id.Value, 0);
                var secondary = secondaryCounts.GetValueOrDefault(m.Id.Value, 0);
                return new MemberResponsibilityLoad(m.Id.Value, m.Name.Value, primary, secondary, primary + secondary);
            })
            .ToList();

        return new ResponsibilityBalanceResponse(loads);
    }
}
