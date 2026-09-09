using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Lists.Events;

public sealed record ListRenamed(
    Guid EventId,
    Guid ListId,
    string NewName,
    DateTime OccurredAtUtc) : IDomainEvent;
