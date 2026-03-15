namespace DomusMind.Contracts.Family;

public sealed record FamilyResponse(
    Guid FamilyId,
    string Name,
    DateTime CreatedAtUtc,
    int MemberCount);
