using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Responsibilities;
using DomusMind.Domain.Family;
using DomusMind.Domain.Responsibilities;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Responsibilities.GetResponsibilityVisibility;

public sealed class GetResponsibilityVisibilityQueryHandler
    : IQueryHandler<GetResponsibilityVisibilityQuery, ResponsibilityVisibilityResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public GetResponsibilityVisibilityQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<ResponsibilityVisibilityResponse> Handle(
        GetResponsibilityVisibilityQuery query,
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

        var views = family.Members
            .OrderBy(m => m.Name.Value)
            .Select(m =>
            {
                var connections = new List<ResponsibilityConnection>();

                foreach (var domain in domains)
                {
                    if (domain.PrimaryOwnerId.HasValue && domain.PrimaryOwnerId.Value.Value == m.Id.Value)
                        connections.Add(new ResponsibilityConnection(domain.Id.Value, domain.Name.Value, "PrimaryOwner"));
                    else if (domain.SecondaryOwnerIds.Any(s => s.Value == m.Id.Value))
                        connections.Add(new ResponsibilityConnection(domain.Id.Value, domain.Name.Value, "SecondaryOwner"));
                }

                return new MemberResponsibilityView(m.Id.Value, m.Name.Value, connections);
            })
            .ToList();

        return new ResponsibilityVisibilityResponse(views);
    }
}
