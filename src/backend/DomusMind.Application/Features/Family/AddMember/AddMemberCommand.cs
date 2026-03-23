using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Family;

namespace DomusMind.Application.Features.Family.AddMember;

public sealed record AddMemberCommand(
    Guid FamilyId,
    string Name,
    string Role,
    DateOnly? BirthDate,
    bool IsManager,
    Guid RequestedByUserId)
    : ICommand<AddMemberResponse>;
