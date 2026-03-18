namespace DomusMind.Contracts.Family;

public sealed record UpdateMemberResponse(
    Guid MemberId,
    Guid FamilyId,
    string Name,
    string Role,
    bool IsManager,
    DateOnly? BirthDate,
    DateTime JoinedAtUtc);
