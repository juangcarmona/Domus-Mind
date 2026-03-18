using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Tasks.ValueObjects;

namespace DomusMind.Domain.Tasks.Events;

public sealed record TaskRescheduled(
    Guid EventId,
    Guid TaskId,
    TaskSchedule NewSchedule,
    DateTime OccurredAtUtc) : IDomainEvent;
