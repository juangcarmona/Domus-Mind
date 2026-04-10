using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Lists;
using DomusMind.Domain.Family;
using DomusMind.Domain.Lists;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Lists.GetFamilyLists;

public sealed class GetFamilyListsQueryHandler
    : IQueryHandler<GetFamilyListsQuery, GetFamilyListsResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public GetFamilyListsQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<GetFamilyListsResponse> Handle(
        GetFamilyListsQuery query,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, query.FamilyId, cancellationToken);

        if (!canAccess)
            throw new ListException(ListErrorCode.AccessDenied, "Access to this family is denied.");

        var familyId = FamilyId.From(query.FamilyId);

        var lists = await _dbContext.Set<SharedList>()
            .AsNoTracking()
            .Include(l => l.Items)
            .Where(l => l.FamilyId == familyId && !l.IsArchived)
            .OrderBy(l => l.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var summaries = lists.Select(l => new ListSummary(
            l.Id.Value,
            l.Name.Value,
            l.Kind.Value,
            l.AreaId?.Value,
            l.LinkedEntityType == "CalendarEvent" ? l.LinkedEntityId : null,
            l.UncheckedCount))
            .ToList();

        return new GetFamilyListsResponse(summaries);
    }
}
