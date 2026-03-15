using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Calendar;
using DomusMind.Contracts.Calendar;
using DomusMind.Domain.Calendar;
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

        try
        {
            calendarEvent.Reschedule(command.NewStartTime, command.NewEndTime);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("cancelled"))
        {
            throw new CalendarException(CalendarErrorCode.EventAlreadyCancelled, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            throw new CalendarException(CalendarErrorCode.InvalidInput, ex.Message);
        }

        await _eventLogWriter.WriteAsync(calendarEvent.DomainEvents, cancellationToken);
        calendarEvent.ClearDomainEvents();

        return new RescheduleEventResponse(
            command.CalendarEventId,
            calendarEvent.Title.Value,
            command.NewStartTime,
            command.NewEndTime,
            calendarEvent.Status.ToString());
    }
}
