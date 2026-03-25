using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.SharedLists.Events;

public sealed record SharedListLinked(
    Guid EventId,
    Guid SharedListId,
    string LinkedEntityType,
    Guid LinkedEntityId,
    DateTime OccurredAtUtc) : IDomainEvent;
