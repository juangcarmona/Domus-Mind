using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Tasks.Events;

public sealed record TaskReassigned(
    Guid EventId,
    Guid TaskId,
    Guid FamilyId,
    Guid? PreviousAssigneeId,
    Guid NewAssigneeId,
    DateTime OccurredAtUtc) : IDomainEvent;
