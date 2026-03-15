using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Family.Events;

public sealed record MemberAdded(
    Guid EventId,
    Guid FamilyId,
    Guid MemberId,
    DateTime OccurredAtUtc) : IDomainEvent;
