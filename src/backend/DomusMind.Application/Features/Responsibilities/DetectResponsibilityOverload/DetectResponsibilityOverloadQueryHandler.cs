using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Responsibilities;
using DomusMind.Domain.Family;
using DomusMind.Domain.Responsibilities;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Responsibilities.DetectResponsibilityOverload;

public sealed class DetectResponsibilityOverloadQueryHandler
    : IQueryHandler<DetectResponsibilityOverloadQuery, ResponsibilityOverloadResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public DetectResponsibilityOverloadQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<ResponsibilityOverloadResponse> Handle(
        DetectResponsibilityOverloadQuery query,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, query.FamilyId, cancellationToken);
        if (!canAccess)
            throw new ResponsibilitiesException(ResponsibilitiesErrorCode.AccessDenied, "Access to this family is denied.");

        var threshold = query.Threshold > 0 ? query.Threshold : 3;
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

        // Build per-member domain lists (primary + secondary)
        var memberDomains = family.Members.ToDictionary(
            m => m.Id.Value,
            _ => new List<string>());

        foreach (var domain in domains)
        {
            if (domain.PrimaryOwnerId.HasValue)
            {
                var id = domain.PrimaryOwnerId.Value.Value;
                if (memberDomains.ContainsKey(id))
                    memberDomains[id].Add(domain.Name.Value);
            }

            foreach (var secondary in domain.SecondaryOwnerIds)
            {
                var id = secondary.Value;
                if (memberDomains.ContainsKey(id) && !memberDomains[id].Contains(domain.Name.Value))
                    memberDomains[id].Add(domain.Name.Value);
            }
        }

        var overloaded = family.Members
            .Where(m => memberDomains[m.Id.Value].Count > threshold)
            .Select(m => new OverloadedMember(
                m.Id.Value,
                m.Name.Value,
                memberDomains[m.Id.Value].Count,
                memberDomains[m.Id.Value]))
            .OrderByDescending(o => o.TotalLoad)
            .ToList();

        return new ResponsibilityOverloadResponse(threshold, overloaded);
    }
}
