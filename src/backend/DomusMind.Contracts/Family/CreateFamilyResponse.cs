namespace DomusMind.Contracts.Family;

public sealed record CreateFamilyResponse(Guid FamilyId, string Name, DateTime CreatedAtUtc);
