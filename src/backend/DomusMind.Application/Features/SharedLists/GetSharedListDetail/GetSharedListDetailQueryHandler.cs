using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.SharedLists;
using DomusMind.Domain.SharedLists;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.SharedLists.GetSharedListDetail;

public sealed class GetSharedListDetailQueryHandler
    : IQueryHandler<GetSharedListDetailQuery, GetSharedListDetailResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public GetSharedListDetailQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<GetSharedListDetailResponse> Handle(
        GetSharedListDetailQuery query,
        CancellationToken cancellationToken)
    {
        var listId = SharedListId.From(query.SharedListId);

        var list = await _dbContext.Set<SharedList>()
            .AsNoTracking()
            .Include(l => l.Items)
            .SingleOrDefaultAsync(l => l.Id == listId, cancellationToken);

        if (list is null)
            throw new SharedListException(SharedListErrorCode.ListNotFound, "Shared list not found.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, list.FamilyId.Value, cancellationToken);

        if (!canAccess)
            throw new SharedListException(SharedListErrorCode.AccessDenied, "Access to this family is denied.");

        var items = list.Items
            .OrderBy(i => i.Order)
            .Select(i => new SharedListItemDetail(
                i.Id.Value,
                i.Name.Value,
                i.Checked,
                i.Quantity,
                i.Note,
                i.Order,
                i.UpdatedAtUtc,
                i.UpdatedByMemberId?.Value))
            .ToList();

        return new GetSharedListDetailResponse(
            list.Id.Value,
            list.Name.Value,
            list.Kind.Value,
            list.AreaId?.Value,
            list.LinkedEntityType,
            list.LinkedEntityId,
            items);
    }
}
