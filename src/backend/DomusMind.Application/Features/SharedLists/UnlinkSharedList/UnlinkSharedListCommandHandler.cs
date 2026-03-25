using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Domain.SharedLists;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.SharedLists.UnlinkSharedList;

public sealed class UnlinkSharedListCommandHandler
    : ICommandHandler<UnlinkSharedListCommand, bool>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public UnlinkSharedListCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<bool> Handle(
        UnlinkSharedListCommand command,
        CancellationToken cancellationToken)
    {
        var listId = SharedListId.From(command.SharedListId);

        var list = await _dbContext.Set<SharedList>()
            .SingleOrDefaultAsync(l => l.Id == listId, cancellationToken);

        if (list is null)
            throw new SharedListException(SharedListErrorCode.ListNotFound, "Shared list not found.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, list.FamilyId.Value, cancellationToken);

        if (!canAccess)
            throw new SharedListException(SharedListErrorCode.AccessDenied, "Access to this family is denied.");

        list.Unlink(DateTime.UtcNow);

        await _eventLogWriter.WriteAsync(list.DomainEvents, cancellationToken);
        list.ClearDomainEvents();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
