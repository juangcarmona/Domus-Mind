using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Calendar;
using DomusMind.Application.Temporal;
using DomusMind.Contracts.Calendar;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.ValueObjects;
using DomusMind.Domain.Family;
using DomusMind.Domain.Shared;

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

        if (string.IsNullOrWhiteSpace(command.EndDate))
            throw new CalendarException(CalendarErrorCode.InvalidInput, "A plan must have an end date. Use the same date as the start for single-day events.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, command.FamilyId, cancellationToken);

        if (!canAccess)
            throw new CalendarException(CalendarErrorCode.AccessDenied, "Access to this family is denied.");

        EventTime eventTime;
        try
        {
            eventTime = TemporalParser.ParseEventTime(command.Date, command.Time, command.EndDate, command.EndTime);
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException or InvalidOperationException)
        {
            throw new CalendarException(CalendarErrorCode.InvalidInput, ex.Message);
        }

        HexColor color;
        try
        {
            color = HexColor.From(command.Color ?? "#3B82F6");
        }
        catch (InvalidOperationException ex)
        {
            throw new CalendarException(CalendarErrorCode.InvalidInput, ex.Message);
        }

        var id = CalendarEventId.New();
        var familyId = FamilyId.From(command.FamilyId);
        var title = EventTitle.Create(command.Title);
        var now = DateTime.UtcNow;

        Domain.Calendar.CalendarEvent calendarEvent;
        try
        {
            calendarEvent = Domain.Calendar.CalendarEvent.Create(
                id, familyId, title, command.Description, eventTime, color, now);
        }
        catch (InvalidOperationException ex)
        {
            throw new CalendarException(CalendarErrorCode.InvalidInput, ex.Message);
        }

        if (command.ParticipantMemberIds is not null)
        {
            foreach (var memberId in command.ParticipantMemberIds)
            {
                calendarEvent.AddParticipant(MemberId.From(memberId));
            }
        }

        _dbContext.Set<Domain.Calendar.CalendarEvent>().Add(calendarEvent);

        await _eventLogWriter.WriteAsync(calendarEvent.DomainEvents, cancellationToken);
        calendarEvent.ClearDomainEvents();

        var (date, time, endDate, endTime) = TemporalParser.FormatEventTime(eventTime);

        return new ScheduleEventResponse(
            id.Value,
            familyId.Value,
            title.Value,
            date,
            time,
            endDate,
            endTime,
            calendarEvent.Status.ToString(),
            color.Value,
            now);
    }
}
