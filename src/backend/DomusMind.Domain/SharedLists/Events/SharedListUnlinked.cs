using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.SharedLists.Events;

public sealed record SharedListUnlinked(
    Guid EventId,
    Guid SharedListId,
    DateTime OccurredAtUtc) : IDomainEvent;
