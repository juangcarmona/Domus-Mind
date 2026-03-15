using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Calendar;

namespace DomusMind.Application.Features.Calendar.CancelEvent;

public sealed record CancelEventCommand(
    Guid CalendarEventId,
    Guid RequestedByUserId)
    : ICommand<CancelEventResponse>;
