using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Calendar.ExternalConnections.Events;

public sealed record ExternalCalendarConnectionDisconnected(
    Guid EventId,
    Guid ConnectionId,
    Guid MemberId,
    string Provider,
    DateTime OccurredAtUtc) : IDomainEvent;
