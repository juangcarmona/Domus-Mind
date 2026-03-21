using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Calendar;

namespace DomusMind.Application.Features.Calendar.RescheduleEvent;

public sealed record RescheduleEventCommand(
    Guid CalendarEventId,
    string Date,
    string? Time,
    string? EndDate,
    string? EndTime,
    string? Title,
    string? Description,
    string? Color,
    Guid RequestedByUserId)
    : ICommand<RescheduleEventResponse>;
