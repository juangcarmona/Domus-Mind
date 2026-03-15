using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Calendar.Events;

public sealed record EventParticipantRemoved(
    Guid EventId,
    Guid CalendarEventId,
    Guid MemberId,
    DateTime OccurredAtUtc) : IDomainEvent;
