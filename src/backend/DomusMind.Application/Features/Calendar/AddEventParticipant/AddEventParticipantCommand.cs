using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Calendar;

namespace DomusMind.Application.Features.Calendar.AddEventParticipant;

public sealed record AddEventParticipantCommand(
    Guid CalendarEventId,
    Guid MemberId,
    Guid RequestedByUserId)
    : ICommand<AddEventParticipantResponse>;
