using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Calendar;

namespace DomusMind.Application.Features.Calendar.RemoveEventParticipant;

public sealed record RemoveEventParticipantCommand(
    Guid CalendarEventId,
    Guid MemberId,
    Guid RequestedByUserId)
    : ICommand<RemoveEventParticipantResponse>;
