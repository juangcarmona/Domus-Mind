using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Lists.Events;

public sealed record ListItemToggled(
    Guid EventId,
    Guid ListId,
    Guid ItemId,
    bool Checked,
    DateTime OccurredAtUtc) : IDomainEvent;
