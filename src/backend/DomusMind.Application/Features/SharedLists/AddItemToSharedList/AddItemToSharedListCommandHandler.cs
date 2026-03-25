using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.SharedLists;
using DomusMind.Domain.SharedLists;
using DomusMind.Domain.SharedLists.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.SharedLists.AddItemToSharedList;

public sealed class AddItemToSharedListCommandHandler
    : ICommandHandler<AddItemToSharedListCommand, AddItemToSharedListResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public AddItemToSharedListCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<AddItemToSharedListResponse> Handle(
        AddItemToSharedListCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new SharedListException(SharedListErrorCode.InvalidInput, "Item name is required.");

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

        SharedListItemName itemName;
        try
        {
            itemName = SharedListItemName.Create(command.Name);
        }
        catch (ArgumentException ex)
        {
            throw new SharedListException(SharedListErrorCode.InvalidInput, ex.Message);
        }

        var itemId = SharedListItemId.New();
        var now = DateTime.UtcNow;

        var item = list.AddItem(itemId, itemName, command.Quantity, command.Note, now);

        await _eventLogWriter.WriteAsync(list.DomainEvents, cancellationToken);
        list.ClearDomainEvents();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AddItemToSharedListResponse(
            itemId.Value,
            list.Id.Value,
            item.Name.Value,
            item.Checked,
            item.Quantity,
            item.Note,
            item.Order);
    }
}
