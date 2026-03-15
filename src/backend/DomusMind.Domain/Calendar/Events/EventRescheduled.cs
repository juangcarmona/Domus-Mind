using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Calendar.Events;

public sealed record EventRescheduled(
    Guid EventId,
    Guid CalendarEventId,
    DateTime NewStartTime,
    DateTime? NewEndTime,
    DateTime OccurredAtUtc) : IDomainEvent;
