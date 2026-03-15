using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Family;

namespace DomusMind.Application.Features.Family.GetMemberActivity;

public sealed record GetMemberActivityQuery(
    Guid FamilyId,
    Guid MemberId,
    Guid RequestedByUserId) : IQuery<MemberActivityResponse>;
