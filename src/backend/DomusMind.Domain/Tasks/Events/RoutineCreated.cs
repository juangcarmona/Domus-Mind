using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Tasks.Events;

public sealed record RoutineCreated(
    Guid EventId,
    Guid RoutineId,
    Guid FamilyId,
    string Name,
    string Scope,
    string Kind,
    string Color,
    DateTime OccurredAtUtc
) : IDomainEvent;