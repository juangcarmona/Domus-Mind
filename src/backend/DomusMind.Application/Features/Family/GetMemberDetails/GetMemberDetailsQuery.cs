using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Family;

namespace DomusMind.Application.Features.Family.GetMemberDetails;

public sealed record GetMemberDetailsQuery(Guid FamilyId, Guid MemberId, Guid RequestedByUserId)
    : IQuery<MemberDetailResponse>;
