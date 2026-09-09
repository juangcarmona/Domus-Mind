using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Lists;
using DomusMind.Domain.Lists;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Lists.SetItemTemporal;

public sealed class SetItemTemporalCommandHandler
    : ICommandHandler<SetItemTemporalCommand, SetItemTemporalResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public SetItemTemporalCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<SetItemTemporalResponse> Handle(
        SetItemTemporalCommand command,
        CancellationToken cancellationToken)
    {
        if (command.DueDate is null && command.Reminder is null && command.Repeat is null)
            throw new ListException(ListErrorCode.InvalidInput,
                "At least one temporal field (dueDate, reminder, repeat) must be provided.");

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
        var now = DateTime.UtcNow;

        ListItem item;
        try
        {
            item = list.SetItemTemporal(itemId, command.DueDate, command.Reminder, command.Repeat, now);
        }
        catch (InvalidOperationException)
        {
            throw new ListException(ListErrorCode.ItemNotFound, "Item not found in this list.");
        }
        catch (ArgumentException ex)
        {
            throw new ListException(ListErrorCode.InvalidInput, ex.Message);
        }

        await _eventLogWriter.WriteAsync(list.DomainEvents, cancellationToken);
        list.ClearDomainEvents();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new SetItemTemporalResponse(
            item.Id.Value,
            item.DueDate,
            item.Reminder,
            item.Repeat,
            item.UpdatedAtUtc);
    }
}
