namespace DomusMind.Contracts.Family;

public sealed record CreateFamilyResponse(Guid FamilyId, string Name, string? PrimaryLanguageCode, DateTime CreatedAtUtc);
