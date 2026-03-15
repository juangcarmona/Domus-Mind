using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Tasks.Events;

public sealed record TaskCancelled(
    Guid EventId,
    Guid TaskId,
    DateTime OccurredAtUtc) : IDomainEvent;
