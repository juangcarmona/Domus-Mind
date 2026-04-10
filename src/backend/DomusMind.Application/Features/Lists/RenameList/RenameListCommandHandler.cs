using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Lists;
using DomusMind.Domain.Lists;
using DomusMind.Domain.Lists.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Lists.RenameList;

public sealed class RenameListCommandHandler
    : ICommandHandler<RenameListCommand, RenameListResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public RenameListCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<RenameListResponse> Handle(
        RenameListCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.NewName))
            throw new ListException(ListErrorCode.InvalidInput, "List name is required.");

        var listId = ListId.From(command.ListId);

        var list = await _dbContext.Set<SharedList>()
            .SingleOrDefaultAsync(l => l.Id == listId, cancellationToken);

        if (list is null)
            throw new ListException(ListErrorCode.ListNotFound, "Shared list not found.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, list.FamilyId.Value, cancellationToken);

        if (!canAccess)
            throw new ListException(ListErrorCode.AccessDenied, "Access to this family is denied.");

        ListName newName;
        try
        {
            newName = ListName.Create(command.NewName);
        }
        catch (ArgumentException ex)
        {
            throw new ListException(ListErrorCode.InvalidInput, ex.Message);
        }

        list.Rename(newName, DateTime.UtcNow);

        await _eventLogWriter.WriteAsync(list.DomainEvents, cancellationToken);
        list.ClearDomainEvents();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RenameListResponse(list.Id.Value, list.Name.Value);
    }
}
