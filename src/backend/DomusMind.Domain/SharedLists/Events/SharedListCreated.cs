using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.SharedLists.Events;

public sealed record SharedListCreated(
    Guid EventId,
    Guid SharedListId,
    Guid FamilyId,
    string Name,
    string Kind,
    DateTime OccurredAtUtc) : IDomainEvent;
