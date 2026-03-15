using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Family;

namespace DomusMind.Application.Features.Family.GetFamilyMembers;

public sealed record GetFamilyMembersQuery(Guid FamilyId, Guid RequestedByUserId)
    : IQuery<IReadOnlyCollection<FamilyMemberResponse>>;
