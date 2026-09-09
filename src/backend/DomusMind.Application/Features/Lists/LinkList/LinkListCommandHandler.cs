using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Lists;
using DomusMind.Domain.Lists;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Lists.LinkList;

public sealed class LinkListCommandHandler
    : ICommandHandler<LinkListCommand, LinkListResponse>
{
    private const string SupportedEntityType = "CalendarEvent";

    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public LinkListCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<LinkListResponse> Handle(
        LinkListCommand command,
        CancellationToken cancellationToken)
    {
        if (command.LinkedEntityType != SupportedEntityType)
            throw new ListException(
                ListErrorCode.InvalidInput,
                $"Entity type '{command.LinkedEntityType}' is not supported. Only '{SupportedEntityType}' is allowed.");

        var listId = ListId.From(command.ListId);

        var list = await _dbContext.Set<SharedList>()
            .SingleOrDefaultAsync(l => l.Id == listId, cancellationToken);

        if (list is null)
            throw new ListException(ListErrorCode.ListNotFound, "Shared list not found.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, list.FamilyId.Value, cancellationToken);

        if (!canAccess)
            throw new ListException(ListErrorCode.AccessDenied, "Access to this family is denied.");

        list.LinkToEntity(command.LinkedEntityType, command.LinkedEntityId, DateTime.UtcNow);

        await _eventLogWriter.WriteAsync(list.DomainEvents, cancellationToken);
        list.ClearDomainEvents();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new LinkListResponse(list.Id.Value, command.LinkedEntityType, command.LinkedEntityId);
    }
}
