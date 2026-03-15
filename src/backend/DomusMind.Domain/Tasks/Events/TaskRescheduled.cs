using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Tasks.Events;

public sealed record TaskRescheduled(
    Guid EventId,
    Guid TaskId,
    DateTime? NewDueDate,
    DateTime OccurredAtUtc) : IDomainEvent;
