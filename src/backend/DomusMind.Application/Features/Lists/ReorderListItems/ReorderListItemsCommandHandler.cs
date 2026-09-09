using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Domain.Lists;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Lists.ReorderListItems;

public sealed class ReorderListItemsCommandHandler
    : ICommandHandler<ReorderListItemsCommand, bool>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public ReorderListItemsCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<bool> Handle(
        ReorderListItemsCommand command,
        CancellationToken cancellationToken)
    {
        if (command.ItemIds is null || command.ItemIds.Count == 0)
            throw new ListException(ListErrorCode.InvalidInput, "Reorder payload cannot be empty.");

        if (command.ItemIds.Distinct().Count() != command.ItemIds.Count)
            throw new ListException(ListErrorCode.InvalidInput, "Duplicate item IDs in reorder payload.");

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

        var orderedIds = command.ItemIds.Select(ListItemId.From).ToList();

        try
        {
            list.ReorderItems(orderedIds, DateTime.UtcNow);
        }
        catch (ArgumentException ex)
        {
            throw new ListException(ListErrorCode.InvalidInput, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            throw new ListException(ListErrorCode.ItemNotFound, ex.Message);
        }

        await _eventLogWriter.WriteAsync(list.DomainEvents, cancellationToken);
        list.ClearDomainEvents();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
