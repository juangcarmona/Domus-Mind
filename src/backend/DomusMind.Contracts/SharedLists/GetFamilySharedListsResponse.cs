namespace DomusMind.Contracts.SharedLists;

public sealed record SharedListSummary(
    Guid Id,
    string Name,
    string Kind,
    Guid? AreaId,
    string? LinkedEntityType,
    Guid? LinkedEntityId,
    int ItemCount,
    int UncheckedCount);

public sealed record GetFamilySharedListsResponse(
    IReadOnlyList<SharedListSummary> Lists);
