using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Responsibilities;
using DomusMind.Domain.Family;
using DomusMind.Domain.Responsibilities;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Responsibilities.SuggestResponsibilityOwner;

public sealed class SuggestResponsibilityOwnerQueryHandler
    : IQueryHandler<SuggestResponsibilityOwnerQuery, SuggestResponsibilityOwnerResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public SuggestResponsibilityOwnerQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<SuggestResponsibilityOwnerResponse> Handle(
        SuggestResponsibilityOwnerQuery query,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, query.FamilyId, cancellationToken);
        if (!canAccess)
            throw new ResponsibilitiesException(ResponsibilitiesErrorCode.AccessDenied, "Access to this family is denied.");

        var familyId = FamilyId.From(query.FamilyId);

        var domain = await _dbContext.Set<ResponsibilityDomain>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                d => d.Id == ResponsibilityDomainId.From(query.ResponsibilityDomainId) && d.FamilyId == familyId,
                cancellationToken);

        if (domain is null)
            throw new ResponsibilitiesException(
                ResponsibilitiesErrorCode.ResponsibilityDomainNotFound, "Responsibility domain not found.");

        // Get all domains for the family to calculate ownership counts
        var allDomains = await _dbContext.Set<ResponsibilityDomain>()
            .AsNoTracking()
            .Where(d => d.FamilyId == familyId)
            .ToListAsync(cancellationToken);

        // Get all family members
        var family = await _dbContext.Set<Domain.Family.Family>()
            .AsNoTracking()
            .Include(f => f.Members)
            .SingleOrDefaultAsync(f => f.Id == familyId, cancellationToken);

        if (family is null)
            throw new ResponsibilitiesException(ResponsibilitiesErrorCode.AccessDenied, "Family not found.");

        var primaryCounts = new Dictionary<Guid, int>();
        foreach (var member in family.Members)
            primaryCounts[member.Id.Value] = 0;

        foreach (var d in allDomains.Where(d => d.PrimaryOwnerId.HasValue))
        {
            var ownerId = d.PrimaryOwnerId!.Value.Value;
            if (primaryCounts.ContainsKey(ownerId))
                primaryCounts[ownerId]++;
            else
                primaryCounts[ownerId] = 1;
        }

        var suggestions = family.Members
            .OrderBy(m => primaryCounts.GetValueOrDefault(m.Id.Value, 0))
            .Select(m => new OwnerSuggestion(
                m.Id.Value,
                m.Name.Value,
                primaryCounts.GetValueOrDefault(m.Id.Value, 0)))
            .ToList();

        return new SuggestResponsibilityOwnerResponse(query.ResponsibilityDomainId, suggestions);
    }
}
