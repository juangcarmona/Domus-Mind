using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Tasks.Events;

public sealed record TaskAssigned(
    Guid EventId,
    Guid TaskId,
    Guid AssigneeId,
    DateTime OccurredAtUtc) : IDomainEvent;
