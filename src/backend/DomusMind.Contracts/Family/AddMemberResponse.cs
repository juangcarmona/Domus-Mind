namespace DomusMind.Contracts.Family;

public sealed record AddMemberResponse(
    Guid MemberId,
    Guid FamilyId,
    string Name,
    string Role,
    DateTime JoinedAtUtc);
