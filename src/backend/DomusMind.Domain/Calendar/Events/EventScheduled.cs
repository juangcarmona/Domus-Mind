using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Calendar.Events;

public sealed record EventScheduled(
    Guid EventId,
    Guid CalendarEventId,
    Guid FamilyId,
    string Title,
    DateTime StartTime,
    DateTime? EndTime,
    DateTime OccurredAtUtc) : IDomainEvent;
