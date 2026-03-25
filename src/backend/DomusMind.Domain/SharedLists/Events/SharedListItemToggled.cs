using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.SharedLists.Events;

public sealed record SharedListItemToggled(
    Guid EventId,
    Guid SharedListId,
    Guid ItemId,
    bool Checked,
    DateTime OccurredAtUtc) : IDomainEvent;
