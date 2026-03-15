using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Calendar;
using DomusMind.Contracts.Calendar;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.ValueObjects;
using DomusMind.Domain.Family;

namespace DomusMind.Application.Features.Calendar.ScheduleEvent;

public sealed class ScheduleEventCommandHandler
    : ICommandHandler<ScheduleEventCommand, ScheduleEventResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public ScheduleEventCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<ScheduleEventResponse> Handle(
        ScheduleEventCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
            throw new CalendarException(CalendarErrorCode.InvalidInput, "Event title is required.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, command.FamilyId, cancellationToken);

        if (!canAccess)
            throw new CalendarException(CalendarErrorCode.AccessDenied, "Access to this family is denied.");

        var id = CalendarEventId.New();
        var familyId = FamilyId.From(command.FamilyId);
        var title = EventTitle.Create(command.Title);
        var now = DateTime.UtcNow;

        Domain.Calendar.CalendarEvent calendarEvent;
        try
        {
            calendarEvent = Domain.Calendar.CalendarEvent.Create(
                id, familyId, title, command.Description, command.StartTime, command.EndTime, now);
        }
        catch (InvalidOperationException ex)
        {
            throw new CalendarException(CalendarErrorCode.InvalidInput, ex.Message);
        }

        _dbContext.Set<Domain.Calendar.CalendarEvent>().Add(calendarEvent);

        await _eventLogWriter.WriteAsync(calendarEvent.DomainEvents, cancellationToken);
        calendarEvent.ClearDomainEvents();

        return new ScheduleEventResponse(
            id.Value,
            familyId.Value,
            title.Value,
            command.StartTime,
            command.EndTime,
            calendarEvent.Status.ToString(),
            now);
    }
}
