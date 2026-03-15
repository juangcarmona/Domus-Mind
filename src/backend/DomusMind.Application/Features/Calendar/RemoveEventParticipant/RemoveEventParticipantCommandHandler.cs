using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Calendar;
using DomusMind.Contracts.Calendar;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Family;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Calendar.RemoveEventParticipant;

public sealed class RemoveEventParticipantCommandHandler
    : ICommandHandler<RemoveEventParticipantCommand, RemoveEventParticipantResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public RemoveEventParticipantCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<RemoveEventParticipantResponse> Handle(
        RemoveEventParticipantCommand command,
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
            calendarEvent.RemoveParticipant(MemberId.From(command.MemberId));
        }
        catch (InvalidOperationException ex)
        {
            throw new CalendarException(CalendarErrorCode.ParticipantNotFound, ex.Message);
        }

        await _eventLogWriter.WriteAsync(calendarEvent.DomainEvents, cancellationToken);
        calendarEvent.ClearDomainEvents();

        return new RemoveEventParticipantResponse(command.CalendarEventId, command.MemberId);
    }
}
