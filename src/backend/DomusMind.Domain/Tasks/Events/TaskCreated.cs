using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Tasks.Events;

public sealed record TaskCreated(
    Guid EventId,
    Guid TaskId,
    Guid FamilyId,
    string Title,
    DateTime? DueDate,
    DateTime OccurredAtUtc) : IDomainEvent;
