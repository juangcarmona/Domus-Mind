using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Lists.Events;

/// <summary>
/// Emitted when an item transitions from non-temporal to temporal (first temporal field set)
/// or when all temporal fields are cleared. Consumers use this to update Agenda projection state.
/// </summary>
public sealed record ListItemScheduled(
    Guid EventId,
    Guid ListId,
    Guid ItemId,
    DateOnly? DueDate,
    DateTimeOffset? Reminder,
    string? Repeat,
    DateTime OccurredAtUtc) : IDomainEvent;
