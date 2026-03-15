namespace DomusMind.Contracts.Family;

public sealed record FamilyMemberResponse(
    Guid MemberId,
    Guid FamilyId,
    string Name,
    string Role,
    DateTime JoinedAtUtc);
