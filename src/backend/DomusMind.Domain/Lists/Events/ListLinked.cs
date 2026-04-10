using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Lists.Events;

public sealed record ListLinked(
    Guid EventId,
    Guid ListId,
    string LinkedEntityType,
    Guid LinkedEntityId,
    DateTime OccurredAtUtc) : IDomainEvent;
