namespace DomusMind.Contracts.Family;

public sealed record FamilyResponse(
    Guid FamilyId,
    string Name,
    string? PrimaryLanguageCode,
    DateTime CreatedAtUtc,
    int MemberCount);
