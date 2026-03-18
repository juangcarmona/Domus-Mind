using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Family;

namespace DomusMind.Application.Features.Family.InviteMember;

public sealed record InviteMemberCommand(
    Guid FamilyId,
    string Name,
    string Role,
    DateOnly? BirthDate,
    bool IsManager,
    string Username,
    string TemporaryPassword,
    Guid RequestedByUserId) : ICommand<InviteMemberResponse>;
