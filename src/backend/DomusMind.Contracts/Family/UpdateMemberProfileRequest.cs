namespace DomusMind.Contracts.Family;

public sealed record UpdateMemberProfileRequest(
    string? PreferredName,
    string? PrimaryPhone,
    string? PrimaryEmail,
    string? HouseholdNote);
