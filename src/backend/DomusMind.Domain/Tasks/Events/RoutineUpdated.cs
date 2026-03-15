using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Tasks.Events;

public sealed record RoutineUpdated(
    Guid EventId,
    Guid RoutineId,
    string Name,
    string Cadence,
    DateTime OccurredAtUtc) : IDomainEvent;
