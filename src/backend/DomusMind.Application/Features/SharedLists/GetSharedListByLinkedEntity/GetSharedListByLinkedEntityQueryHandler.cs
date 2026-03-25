using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.SharedLists;
using DomusMind.Domain.SharedLists;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.SharedLists.GetSharedListByLinkedEntity;

public sealed class GetSharedListByLinkedEntityQueryHandler
    : IQueryHandler<GetSharedListByLinkedEntityQuery, GetSharedListByLinkedEntityResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public GetSharedListByLinkedEntityQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<GetSharedListByLinkedEntityResponse> Handle(
        GetSharedListByLinkedEntityQuery query,
        CancellationToken cancellationToken)
    {
        var list = await _dbContext.Set<SharedList>()
            .AsNoTracking()
            .Include(l => l.Items)
            .SingleOrDefaultAsync(
                l => l.LinkedEntityType == query.EntityType && l.LinkedEntityId == query.EntityId,
                cancellationToken);

        if (list is null)
            throw new SharedListException(SharedListErrorCode.ListNotFound, "No shared list is linked to this entity.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, list.FamilyId.Value, cancellationToken);

        if (!canAccess)
            throw new SharedListException(SharedListErrorCode.AccessDenied, "Access to this family is denied.");

        return new GetSharedListByLinkedEntityResponse(
            list.Id.Value,
            list.Name.Value,
            list.Kind.Value,
            list.Items.Count,
            list.UncheckedCount);
    }
}
