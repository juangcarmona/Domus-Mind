using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Calendar.ValueObjects;

namespace DomusMind.Domain.Calendar.Events;

public sealed record EventScheduled(
    Guid EventId,
    Guid CalendarEventId,
    Guid FamilyId,
    string Title,
    EventTime Time,
    DateTime OccurredAtUtc) : IDomainEvent;
