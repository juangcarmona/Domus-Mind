using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Lists.Events;

public sealed record ListRestored(
    Guid EventId,
    Guid ListId,
    DateTime OccurredAtUtc) : IDomainEvent;
