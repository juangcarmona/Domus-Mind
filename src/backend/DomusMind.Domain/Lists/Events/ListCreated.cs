using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Lists.Events;

public sealed record ListCreated(
    Guid EventId,
    Guid ListId,
    Guid FamilyId,
    string Name,
    string Kind,
    DateTime OccurredAtUtc) : IDomainEvent;
