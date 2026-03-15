using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Calendar;

namespace DomusMind.Application.Features.Calendar.RescheduleEvent;

public sealed record RescheduleEventCommand(
    Guid CalendarEventId,
    DateTime NewStartTime,
    DateTime? NewEndTime,
    Guid RequestedByUserId)
    : ICommand<RescheduleEventResponse>;
