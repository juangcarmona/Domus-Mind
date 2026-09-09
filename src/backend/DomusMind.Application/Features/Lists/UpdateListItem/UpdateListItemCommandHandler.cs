using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Lists;
using DomusMind.Domain.Lists;
using DomusMind.Domain.Lists.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Lists.UpdateListItem;

public sealed class UpdateListItemCommandHandler
    : ICommandHandler<UpdateListItemCommand, UpdateListItemResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public UpdateListItemCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<UpdateListItemResponse> Handle(
        UpdateListItemCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ListException(ListErrorCode.InvalidInput, "Item name is required.");

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

        ListItemName itemName;
        try
        {
            itemName = ListItemName.Create(command.Name);
        }
        catch (ArgumentException ex)
        {
            throw new ListException(ListErrorCode.InvalidInput, ex.Message);
        }

        var itemId = ListItemId.From(command.ItemId);
        ListItem item;
        try
        {
            item = list.UpdateItem(itemId, itemName, command.Quantity, command.Note, DateTime.UtcNow);
        }
        catch (InvalidOperationException)
        {
            throw new ListException(ListErrorCode.ItemNotFound, "Item not found in this list.");
        }

        await _eventLogWriter.WriteAsync(list.DomainEvents, cancellationToken);
        list.ClearDomainEvents();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateListItemResponse(
            item.Id.Value,
            item.Name.Value,
            item.Quantity,
            item.Note,
            item.UpdatedAtUtc,
            item.Importance,
            item.DueDate,
            item.Reminder,
            item.Repeat);
    }
}
