using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Family;

namespace DomusMind.Application.Features.Family.UpdateMember;

public sealed record UpdateMemberCommand(
    Guid FamilyId,
    Guid MemberId,
    string Name,
    string Role,
    DateOnly? BirthDate,
    bool IsManager,
    Guid RequestedByUserId) : ICommand<UpdateMemberResponse>;
