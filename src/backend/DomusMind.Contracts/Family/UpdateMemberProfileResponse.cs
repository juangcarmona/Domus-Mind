namespace DomusMind.Contracts.Family;

public sealed record UpdateMemberProfileResponse(
    Guid MemberId,
    Guid FamilyId,
    string? PreferredName,
    string? PrimaryPhone,
    string? PrimaryEmail,
    string? HouseholdNote);
