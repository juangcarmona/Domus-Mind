using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Lists.Events;

public sealed record ListItemUpdated(
    Guid EventId,
    Guid ListId,
    Guid ItemId,
    string NewName,
    string? NewQuantity,
    string? NewNote,
    DateTime OccurredAtUtc) : IDomainEvent;
