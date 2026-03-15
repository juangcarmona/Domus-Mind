using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Tasks.Events;

public sealed record RoutinePaused(
    Guid EventId,
    Guid RoutineId,
    DateTime OccurredAtUtc) : IDomainEvent;
