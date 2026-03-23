using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Family;

namespace DomusMind.Application.Features.Family.UpdateMemberProfile;

public sealed record UpdateMemberProfileCommand(
    Guid FamilyId,
    Guid MemberId,
    Guid RequestedByUserId,
    string? PreferredName,
    string? PrimaryPhone,
    string? PrimaryEmail,
    string? HouseholdNote) : ICommand<UpdateMemberProfileResponse>;
