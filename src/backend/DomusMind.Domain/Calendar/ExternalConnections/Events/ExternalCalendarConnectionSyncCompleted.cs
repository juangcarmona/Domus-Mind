using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Calendar.ExternalConnections.Events;

public sealed record ExternalCalendarConnectionSyncCompleted(
    Guid EventId,
    Guid ConnectionId,
    Guid MemberId,
    int ImportedEntryCount,
    int UpdatedEntryCount,
    int DeletedEntryCount,
    DateTime OccurredAtUtc) : IDomainEvent;
