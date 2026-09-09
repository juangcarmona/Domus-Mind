using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Calendar.ExternalConnections.Events;

public sealed record ExternalCalendarConnectionConnected(
    Guid EventId,
    Guid ConnectionId,
    Guid MemberId,
    string Provider,
    string AccountEmail,
    DateTime OccurredAtUtc) : IDomainEvent;
