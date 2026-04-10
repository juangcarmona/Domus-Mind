using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Lists.Events;

public sealed record ListItemRemoved(
    Guid EventId,
    Guid ListId,
    Guid ItemId,
    DateTime OccurredAtUtc) : IDomainEvent;
