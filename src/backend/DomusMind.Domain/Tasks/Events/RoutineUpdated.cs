using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Tasks.Events;

public sealed record RoutineUpdated(
    Guid EventId,
    Guid RoutineId,
    string Name,
    string Scope,
    string Kind,
    string Color,
    DateTime OccurredAtUtc
) : IDomainEvent;