using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Calendar.ValueObjects;

namespace DomusMind.Domain.Calendar.Events;

public sealed record EventRescheduled(
    Guid EventId,
    Guid CalendarEventId,
    EventTime NewTime,
    DateTime OccurredAtUtc) : IDomainEvent;
