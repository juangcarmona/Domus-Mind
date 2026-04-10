using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Lists.Events;

public sealed record ItemOrderEntry(Guid ItemId, int NewOrder);

public sealed record ListItemsReordered(
    Guid EventId,
    Guid ListId,
    IReadOnlyList<ItemOrderEntry> ItemOrders,
    DateTime OccurredAtUtc) : IDomainEvent;
