using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Calendar;
using DomusMind.Application.Temporal;
using DomusMind.Contracts.Calendar;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.ValueObjects;
using DomusMind.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Calendar.RescheduleEvent;

public sealed class RescheduleEventCommandHandler
    : ICommandHandler<RescheduleEventCommand, RescheduleEventResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public RescheduleEventCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<RescheduleEventResponse> Handle(
        RescheduleEventCommand command,
        CancellationToken cancellationToken)
    {
        var calendarEvent = await _dbContext
            .Set<Domain.Calendar.CalendarEvent>()
            .SingleOrDefaultAsync(
                e => e.Id == CalendarEventId.From(command.CalendarEventId),
                cancellationToken);

        if (calendarEvent is null)
            throw new CalendarException(CalendarErrorCode.EventNotFound, "Event was not found.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, calendarEvent.FamilyId.Value, cancellationToken);

        if (!canAccess)
            throw new CalendarException(CalendarErrorCode.AccessDenied, "Access to this family is denied.");

        Domain.Calendar.ValueObjects.EventTime newTime;
        try
        {
            if (string.IsNullOrWhiteSpace(command.EndDate))
                throw new CalendarException(CalendarErrorCode.InvalidInput, "A plan must have an end date. Use the same date as the start for single-day events.");

            newTime = TemporalParser.ParseEventTime(command.Date, command.Time, command.EndDate, command.EndTime);
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException or InvalidOperationException)
        {
            throw new CalendarException(CalendarErrorCode.InvalidInput, ex.Message);
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(command.Title) || command.Description is not null)
            {
                var title = !string.IsNullOrWhiteSpace(command.Title)
                    ? EventTitle.Create(command.Title)
                    : calendarEvent.Title;
                calendarEvent.Edit(title, command.Description);
            }
            calendarEvent.Reschedule(newTime);
            if (command.Color is not null)
            {
                calendarEvent.Repaint(HexColor.From(command.Color));
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("cancelled"))
        {
            throw new CalendarException(CalendarErrorCode.EventAlreadyCancelled, ex.Message);
        }
        catch (ArgumentException ex)
        {
            throw new CalendarException(CalendarErrorCode.InvalidInput, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            throw new CalendarException(CalendarErrorCode.InvalidInput, ex.Message);
        }

        await _eventLogWriter.WriteAsync(calendarEvent.DomainEvents, cancellationToken);
        calendarEvent.ClearDomainEvents();

        var (date, time, endDate, endTime) = TemporalParser.FormatEventTime(newTime);

        return new RescheduleEventResponse(
            command.CalendarEventId,
            calendarEvent.Title.Value,
            date,
            time,
            endDate,
            endTime,
            calendarEvent.Status.ToString(),
            calendarEvent.Color.Value);
    }
}
