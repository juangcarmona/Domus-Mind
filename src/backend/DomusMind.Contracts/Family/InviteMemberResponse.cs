namespace DomusMind.Contracts.Family;

public sealed record InviteMemberResponse(
    Guid MemberId,
    Guid FamilyId,
    string Name,
    string Role,
    bool IsManager,
    DateOnly? BirthDate,
    string Username,
    DateTime JoinedAtUtc);
