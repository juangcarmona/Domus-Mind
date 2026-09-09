using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Lists;
using DomusMind.Domain.Lists;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Lists.GetListByLinkedEntity;

public sealed class GetListByLinkedEntityQueryHandler
    : IQueryHandler<GetListByLinkedEntityQuery, GetListByLinkedEntityResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public GetListByLinkedEntityQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<GetListByLinkedEntityResponse> Handle(
        GetListByLinkedEntityQuery query,
        CancellationToken cancellationToken)
    {
        var list = await _dbContext.Set<SharedList>()
            .AsNoTracking()
            .Include(l => l.Items)
            .SingleOrDefaultAsync(
                l => l.LinkedEntityType == query.EntityType && l.LinkedEntityId == query.EntityId,
                cancellationToken);

        if (list is null)
            throw new ListException(ListErrorCode.ListNotFound, "No shared list is linked to this entity.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, list.FamilyId.Value, cancellationToken);

        if (!canAccess)
            throw new ListException(ListErrorCode.AccessDenied, "Access to this family is denied.");

        return new GetListByLinkedEntityResponse(
            list.Id.Value,
            list.Name.Value,
            list.Kind.Value,
            list.Items.Count,
            list.UncheckedCount);
    }
}
