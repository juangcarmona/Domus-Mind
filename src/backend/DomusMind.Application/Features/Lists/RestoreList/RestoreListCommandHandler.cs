using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Domain.Lists;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Lists.RestoreList;

public sealed class RestoreListCommandHandler
    : ICommandHandler<RestoreListCommand, bool>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public RestoreListCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<bool> Handle(
        RestoreListCommand command,
        CancellationToken cancellationToken)
    {
        var listId = ListId.From(command.ListId);

        var list = await _dbContext.Set<SharedList>()
            .SingleOrDefaultAsync(l => l.Id == listId, cancellationToken);

        if (list is null)
            throw new ListException(ListErrorCode.ListNotFound, "List not found.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, list.FamilyId.Value, cancellationToken);

        if (!canAccess)
            throw new ListException(ListErrorCode.AccessDenied, "Access to this family is denied.");

        try
        {
            list.Restore(DateTime.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new ListException(ListErrorCode.InvalidInput, ex.Message);
        }

        await _eventLogWriter.WriteAsync(list.DomainEvents, cancellationToken);
        list.ClearDomainEvents();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
