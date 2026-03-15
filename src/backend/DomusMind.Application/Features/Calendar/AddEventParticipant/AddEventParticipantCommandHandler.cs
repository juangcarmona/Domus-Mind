using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Calendar;
using DomusMind.Contracts.Calendar;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Family;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Calendar.AddEventParticipant;

public sealed class AddEventParticipantCommandHandler
    : ICommandHandler<AddEventParticipantCommand, AddEventParticipantResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public AddEventParticipantCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<AddEventParticipantResponse> Handle(
        AddEventParticipantCommand command,
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
            calendarEvent.AddParticipant(MemberId.From(command.MemberId));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already a participant"))
        {
            throw new CalendarException(CalendarErrorCode.DuplicateParticipant, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            throw new CalendarException(CalendarErrorCode.EventAlreadyCancelled, ex.Message);
        }

        await _eventLogWriter.WriteAsync(calendarEvent.DomainEvents, cancellationToken);
        calendarEvent.ClearDomainEvents();

        return new AddEventParticipantResponse(command.CalendarEventId, command.MemberId);
    }
}
