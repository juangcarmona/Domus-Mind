using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Calendar.Events;

public sealed record ReminderAdded(
    Guid EventId,
    Guid CalendarEventId,
    int MinutesBefore,
    DateTime OccurredAtUtc) : IDomainEvent;
