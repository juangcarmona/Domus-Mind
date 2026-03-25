using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.SharedLists;
using DomusMind.Domain.Family;
using DomusMind.Domain.SharedLists;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.SharedLists.ToggleSharedListItem;

public sealed class ToggleSharedListItemCommandHandler
    : ICommandHandler<ToggleSharedListItemCommand, ToggleSharedListItemResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public ToggleSharedListItemCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<ToggleSharedListItemResponse> Handle(
        ToggleSharedListItemCommand command,
        CancellationToken cancellationToken)
    {
        var listId = SharedListId.From(command.SharedListId);

        var list = await _dbContext.Set<SharedList>()
            .Include(l => l.Items)
            .SingleOrDefaultAsync(l => l.Id == listId, cancellationToken);

        if (list is null)
            throw new SharedListException(SharedListErrorCode.ListNotFound, "Shared list not found.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, list.FamilyId.Value, cancellationToken);

        if (!canAccess)
            throw new SharedListException(SharedListErrorCode.AccessDenied, "Access to this family is denied.");

        var itemId = SharedListItemId.From(command.ItemId);
        var updatedByMemberId = command.UpdatedByMemberId.HasValue
            ? MemberId.From(command.UpdatedByMemberId.Value)
            : (MemberId?)null;
        var now = DateTime.UtcNow;

        SharedListItem item;
        try
        {
            item = list.ToggleItem(itemId, updatedByMemberId, now);
        }
        catch (InvalidOperationException)
        {
            throw new SharedListException(SharedListErrorCode.ItemNotFound, "Item not found in this list.");
        }

        await _eventLogWriter.WriteAsync(list.DomainEvents, cancellationToken);
        list.ClearDomainEvents();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ToggleSharedListItemResponse(
            itemId.Value,
            item.Checked,
            item.UpdatedAtUtc,
            item.UpdatedByMemberId?.Value,
            list.UncheckedCount);
    }
}
