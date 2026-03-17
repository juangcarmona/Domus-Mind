using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Tasks.Events;

public sealed record RoutineResumed(
    Guid EventId,
    Guid RoutineId,
    DateTime OccurredAtUtc
) : IDomainEvent;