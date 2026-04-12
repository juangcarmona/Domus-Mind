using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Lists;
using DomusMind.Domain.Lists;
using DomusMind.Domain.Lists.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Lists.SetItemContext;

public sealed class SetItemContextCommandHandler
    : ICommandHandler<SetItemContextCommand, SetItemContextResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public SetItemContextCommandHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<SetItemContextResponse> Handle(
        SetItemContextCommand command,
        CancellationToken cancellationToken)
    {
        var listId = ListId.From(command.ListId);

        var list = await _dbContext.Set<SharedList>()
            .Include(l => l.Items)
            .SingleOrDefaultAsync(l => l.Id == listId, cancellationToken);

        if (list is null)
            throw new ListException(ListErrorCode.ListNotFound, "Shared list not found.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, list.FamilyId.Value, cancellationToken);

        if (!canAccess)
            throw new ListException(ListErrorCode.AccessDenied, "Access to this family is denied.");

        var itemId = ListItemId.From(command.ItemId);

        ListItem item;
        try
        {
            item = list.SetItemContext(itemId, command.ItemAreaId, command.TargetMemberId, DateTime.UtcNow);
        }
        catch (InvalidOperationException)
        {
            throw new ListException(ListErrorCode.ItemNotFound, "Item not found in this list.");
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new SetItemContextResponse(
            item.Id.Value,
            item.ItemAreaId,
            item.TargetMemberId,
            item.UpdatedAtUtc);
    }
}
