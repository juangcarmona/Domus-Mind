using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Calendar.Events;

public sealed record EventCancelled(
    Guid EventId,
    Guid CalendarEventId,
    DateTime OccurredAtUtc) : IDomainEvent;
