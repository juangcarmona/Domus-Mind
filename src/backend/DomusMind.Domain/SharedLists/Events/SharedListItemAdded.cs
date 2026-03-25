using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.SharedLists.Events;

public sealed record SharedListItemAdded(
    Guid EventId,
    Guid SharedListId,
    Guid ItemId,
    string ItemName,
    int Order,
    DateTime OccurredAtUtc) : IDomainEvent;
