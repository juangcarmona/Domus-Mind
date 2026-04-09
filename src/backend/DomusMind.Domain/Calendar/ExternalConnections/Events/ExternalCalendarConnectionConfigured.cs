using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Calendar.ExternalConnections.Events;

public sealed record ExternalCalendarConnectionConfigured(
    Guid EventId,
    Guid ConnectionId,
    Guid MemberId,
    int SelectedCalendarCount,
    int ForwardHorizonDays,
    DateTime OccurredAtUtc) : IDomainEvent;
