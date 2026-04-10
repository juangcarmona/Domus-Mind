using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Lists;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Family;
using DomusMind.Domain.Lists;
using DomusMind.Domain.Lists.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Lists.CreateLinkedListForEvent;

public sealed class CreateLinkedListForEventCommandHandler
    : ICommandHandler<CreateLinkedListForEventCommand, CreateListResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public CreateLinkedListForEventCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<CreateListResponse> Handle(
        CreateLinkedListForEventCommand command,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, command.FamilyId, cancellationToken);

        if (!canAccess)
            throw new ListException(ListErrorCode.AccessDenied, "Access to this family is denied.");

        var eventId = CalendarEventId.From(command.CalendarEventId);

        var calendarEvent = await _dbContext.Set<CalendarEvent>()
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.Id == eventId, cancellationToken);

        if (calendarEvent is null)
            throw new ListException(ListErrorCode.InvalidInput, "Calendar event not found.");

        if (calendarEvent.FamilyId.Value != command.FamilyId)
            throw new ListException(ListErrorCode.AccessDenied, "Calendar event does not belong to this family.");

        var listName = string.IsNullOrWhiteSpace(command.Name)
            ? $"{calendarEvent.Title.Value} checklist"
            : command.Name;

        ListName name;
        try
        {
            name = ListName.Create(listName);
        }
        catch (ArgumentException ex)
        {
            throw new ListException(ListErrorCode.InvalidInput, ex.Message);
        }

        var id = ListId.New();
        var familyId = FamilyId.From(command.FamilyId);
        var kind = ListKind.Create("General");
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

        return new CreateListResponse(
            list.Id.Value, list.FamilyId.Value, list.Name.Value, list.Kind.Value,
            list.AreaId?.Value, list.LinkedEntityId, list.CreatedAtUtc);
    }
}
