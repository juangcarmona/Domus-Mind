using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Calendar.ExternalConnections.Events;

public sealed record ExternalCalendarConnectionSyncFailed(
    Guid EventId,
    Guid ConnectionId,
    Guid MemberId,
    string ErrorCode,
    string ErrorMessage,
    DateTime OccurredAtUtc) : IDomainEvent;
