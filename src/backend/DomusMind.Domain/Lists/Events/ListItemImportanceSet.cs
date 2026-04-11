using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Lists.Events;

public sealed record ListItemImportanceSet(
    Guid EventId,
    Guid ListId,
    Guid ItemId,
    bool Importance,
    DateTime OccurredAtUtc) : IDomainEvent;
