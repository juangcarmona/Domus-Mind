namespace DomusMind.Contracts.SharedLists;

public sealed record CreateSharedListRequest(
    Guid FamilyId,
    string Name,
    string Kind,
    Guid? AreaId,
    string? LinkedEntityType,
    Guid? LinkedEntityId);
