using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.SharedLists;
using DomusMind.Domain.Family;
using DomusMind.Domain.Responsibilities;
using DomusMind.Domain.SharedLists;
using DomusMind.Domain.SharedLists.ValueObjects;

namespace DomusMind.Application.Features.SharedLists.CreateSharedList;

public sealed class CreateSharedListCommandHandler
    : ICommandHandler<CreateSharedListCommand, CreateSharedListResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public CreateSharedListCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<CreateSharedListResponse> Handle(
        CreateSharedListCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new SharedListException(SharedListErrorCode.InvalidInput, "List name is required.");

        if (string.IsNullOrWhiteSpace(command.Kind))
            throw new SharedListException(SharedListErrorCode.InvalidInput, "List kind is required.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, command.FamilyId, cancellationToken);

        if (!canAccess)
            throw new SharedListException(SharedListErrorCode.AccessDenied, "Access to this family is denied.");

        SharedListName name;
        SharedListKind kind;
        try
        {
            name = SharedListName.Create(command.Name);
            kind = SharedListKind.Create(command.Kind);
        }
        catch (ArgumentException ex)
        {
            throw new SharedListException(SharedListErrorCode.InvalidInput, ex.Message);
        }

        var id = SharedListId.New();
        var familyId = FamilyId.From(command.FamilyId);
        var areaId = command.AreaId.HasValue
            ? ResponsibilityDomainId.From(command.AreaId.Value)
            : (ResponsibilityDomainId?)null;
        var now = DateTime.UtcNow;

        var list = SharedList.Create(
            id, familyId, name, kind, areaId,
            command.LinkedEntityType, command.LinkedEntityId, now);

        _dbContext.Set<SharedList>().Add(list);
        await _eventLogWriter.WriteAsync(list.DomainEvents, cancellationToken);
        list.ClearDomainEvents();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateSharedListResponse(
            id.Value,
            familyId.Value,
            name.Value,
            kind.Value,
            areaId?.Value,
            command.LinkedEntityType,
            command.LinkedEntityId,
            now);
    }
}
