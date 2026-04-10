using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Lists.Events;

public sealed record ListDeleted(
    Guid EventId,
    Guid ListId,
    DateTime OccurredAtUtc) : IDomainEvent;
