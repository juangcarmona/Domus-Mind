using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.SharedLists;
using DomusMind.Domain.SharedLists;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.SharedLists.LinkSharedList;

public sealed class LinkSharedListCommandHandler
    : ICommandHandler<LinkSharedListCommand, LinkSharedListResponse>
{
    private const string SupportedEntityType = "CalendarEvent";

    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public LinkSharedListCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<LinkSharedListResponse> Handle(
        LinkSharedListCommand command,
        CancellationToken cancellationToken)
    {
        if (command.LinkedEntityType != SupportedEntityType)
            throw new SharedListException(
                SharedListErrorCode.InvalidInput,
                $"Entity type '{command.LinkedEntityType}' is not supported. Only '{SupportedEntityType}' is allowed.");

        var listId = SharedListId.From(command.SharedListId);

        var list = await _dbContext.Set<SharedList>()
            .SingleOrDefaultAsync(l => l.Id == listId, cancellationToken);

        if (list is null)
            throw new SharedListException(SharedListErrorCode.ListNotFound, "Shared list not found.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, list.FamilyId.Value, cancellationToken);

        if (!canAccess)
            throw new SharedListException(SharedListErrorCode.AccessDenied, "Access to this family is denied.");

        list.LinkToEntity(command.LinkedEntityType, command.LinkedEntityId, DateTime.UtcNow);

        await _eventLogWriter.WriteAsync(list.DomainEvents, cancellationToken);
        list.ClearDomainEvents();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new LinkSharedListResponse(list.Id.Value, command.LinkedEntityType, command.LinkedEntityId);
    }
}
