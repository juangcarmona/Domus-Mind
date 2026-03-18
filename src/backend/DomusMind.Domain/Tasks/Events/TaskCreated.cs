using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Tasks.ValueObjects;

namespace DomusMind.Domain.Tasks.Events;

public sealed record TaskCreated(
    Guid EventId,
    Guid TaskId,
    Guid FamilyId,
    string Title,
    TaskSchedule Schedule,
    DateTime OccurredAtUtc) : IDomainEvent;
