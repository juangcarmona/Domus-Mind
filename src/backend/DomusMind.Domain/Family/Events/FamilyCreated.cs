using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Family.Events;

public sealed record FamilyCreated(
    Guid EventId,
    Guid FamilyId,
    DateTime OccurredAtUtc) : IDomainEvent;
