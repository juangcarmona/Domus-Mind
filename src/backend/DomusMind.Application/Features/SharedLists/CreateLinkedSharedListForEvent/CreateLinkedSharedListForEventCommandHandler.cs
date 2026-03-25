using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.SharedLists;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Family;
using DomusMind.Domain.SharedLists;
using DomusMind.Domain.SharedLists.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.SharedLists.CreateLinkedSharedListForEvent;

public sealed class CreateLinkedSharedListForEventCommandHandler
    : ICommandHandler<CreateLinkedSharedListForEventCommand, CreateSharedListResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public CreateLinkedSharedListForEventCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<CreateSharedListResponse> Handle(
        CreateLinkedSharedListForEventCommand command,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, command.FamilyId, cancellationToken);

        if (!canAccess)
            throw new SharedListException(SharedListErrorCode.AccessDenied, "Access to this family is denied.");

        var eventId = CalendarEventId.From(command.CalendarEventId);

        var calendarEvent = await _dbContext.Set<CalendarEvent>()
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.Id == eventId, cancellationToken);

        if (calendarEvent is null)
            throw new SharedListException(SharedListErrorCode.InvalidInput, "Calendar event not found.");

        if (calendarEvent.FamilyId.Value != command.FamilyId)
            throw new SharedListException(SharedListErrorCode.AccessDenied, "Calendar event does not belong to this family.");

        var listName = string.IsNullOrWhiteSpace(command.Name)
            ? $"{calendarEvent.Title.Value} checklist"
            : command.Name;

        SharedListName name;
        try
        {
            name = SharedListName.Create(listName);
        }
        catch (ArgumentException ex)
        {
            throw new SharedListException(SharedListErrorCode.InvalidInput, ex.Message);
        }

        var id = SharedListId.New();
        var familyId = FamilyId.From(command.FamilyId);
        var kind = SharedListKind.Create("General");
        var now = DateTime.UtcNow;

        var list = SharedList.Create(
            id, familyId, name, kind,
            areaId: null,
            linkedEntityType: "CalendarEvent",
            linkedEntityId: command.CalendarEventId,
            createdAtUtc: now);

        _dbContext.Set<SharedList>().Add(list);
        await _eventLogWriter.WriteAsync(list.DomainEvents, cancellationToken);
        list.ClearDomainEvents();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateSharedListResponse(
            list.Id.Value, list.FamilyId.Value, list.Name.Value, list.Kind.Value,
            list.AreaId?.Value, list.LinkedEntityType, list.LinkedEntityId, list.CreatedAtUtc);
    }
}
