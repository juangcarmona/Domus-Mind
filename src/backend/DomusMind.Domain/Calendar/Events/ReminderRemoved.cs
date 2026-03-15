using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Calendar.Events;

public sealed record ReminderRemoved(
    Guid EventId,
    Guid CalendarEventId,
    int MinutesBefore,
    DateTime OccurredAtUtc) : IDomainEvent;
