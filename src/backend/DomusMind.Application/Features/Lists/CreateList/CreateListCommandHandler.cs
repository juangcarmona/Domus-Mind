using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Lists;
using DomusMind.Domain.Family;
using DomusMind.Domain.Responsibilities;
using DomusMind.Domain.Lists;
using DomusMind.Domain.Lists.ValueObjects;

namespace DomusMind.Application.Features.Lists.CreateList;

public sealed class CreateListCommandHandler
    : ICommandHandler<CreateListCommand, CreateListResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public CreateListCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<CreateListResponse> Handle(
        CreateListCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ListException(ListErrorCode.InvalidInput, "List name is required.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, command.FamilyId, cancellationToken);

        if (!canAccess)
            throw new ListException(ListErrorCode.AccessDenied, "Access to this family is denied.");

        ListName name;
        ListKind kind;
        try
        {
            name = ListName.Create(command.Name);
            kind = ListKind.Create(command.Kind ?? "general");
        }
        catch (ArgumentException ex)
        {
            throw new ListException(ListErrorCode.InvalidInput, ex.Message);
        }

        var id = ListId.New();
        var familyId = FamilyId.From(command.FamilyId);
        var areaId = command.AreaId.HasValue
            ? ResponsibilityDomainId.From(command.AreaId.Value)
            : (ResponsibilityDomainId?)null;
        var now = DateTime.UtcNow;

        var list = SharedList.Create(
            id, familyId, name, kind, areaId,
            command.LinkedEntityId.HasValue ? "CalendarEvent" : null,
            command.LinkedEntityId, now);

        _dbContext.Set<SharedList>().Add(list);
        await _eventLogWriter.WriteAsync(list.DomainEvents, cancellationToken);
        list.ClearDomainEvents();
        await _dbContext.SaveChangesAsync(cancellationToken);

        var linkedPlanId = command.LinkedEntityId;

        return new CreateListResponse(
            id.Value,
            familyId.Value,
            name.Value,
            kind.Value,
            areaId?.Value,
            linkedPlanId,
            now);
    }
}
