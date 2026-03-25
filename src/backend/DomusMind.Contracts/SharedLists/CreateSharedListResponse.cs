namespace DomusMind.Contracts.SharedLists;

public sealed record CreateSharedListResponse(
    Guid ListId,
    Guid FamilyId,
    string Name,
    string Kind,
    Guid? AreaId,
    string? LinkedEntityType,
    Guid? LinkedEntityId,
    DateTime CreatedAtUtc);
