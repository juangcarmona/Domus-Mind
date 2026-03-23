using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Family;

namespace DomusMind.Application.Features.Family.EnableMemberAccess;

public sealed record EnableMemberAccessCommand(
    Guid FamilyId,
    Guid MemberId,
    Guid RequestedByUserId) : ICommand<EnableMemberAccessResponse>;
