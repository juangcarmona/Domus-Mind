using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Lists.Events;

public sealed record ListItemAdded(
    Guid EventId,
    Guid ListId,
    Guid ItemId,
    string ItemName,
    int Order,
    DateTime OccurredAtUtc) : IDomainEvent;
