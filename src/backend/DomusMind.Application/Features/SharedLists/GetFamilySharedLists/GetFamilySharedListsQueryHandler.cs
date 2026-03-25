using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.SharedLists;
using DomusMind.Domain.Family;
using DomusMind.Domain.SharedLists;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.SharedLists.GetFamilySharedLists;

public sealed class GetFamilySharedListsQueryHandler
    : IQueryHandler<GetFamilySharedListsQuery, GetFamilySharedListsResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public GetFamilySharedListsQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<GetFamilySharedListsResponse> Handle(
        GetFamilySharedListsQuery query,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, query.FamilyId, cancellationToken);

        if (!canAccess)
            throw new SharedListException(SharedListErrorCode.AccessDenied, "Access to this family is denied.");

        var familyId = FamilyId.From(query.FamilyId);

        var lists = await _dbContext.Set<SharedList>()
            .AsNoTracking()
            .Include(l => l.Items)
            .Where(l => l.FamilyId == familyId)
            .OrderBy(l => l.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var summaries = lists.Select(l => new SharedListSummary(
            l.Id.Value,
            l.Name.Value,
            l.Kind.Value,
            l.AreaId?.Value,
            l.LinkedEntityType,
            l.LinkedEntityId,
            l.Items.Count,
            l.UncheckedCount))
            .ToList();

        return new GetFamilySharedListsResponse(summaries);
    }
}
