using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.SharedLists.Events;

public sealed record ItemOrderEntry(Guid ItemId, int NewOrder);

public sealed record SharedListItemsReordered(
    Guid EventId,
    Guid SharedListId,
    IReadOnlyList<ItemOrderEntry> ItemOrders,
    DateTime OccurredAtUtc) : IDomainEvent;
