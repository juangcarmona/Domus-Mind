namespace DomusMind.Contracts.SharedLists;

public sealed record SharedListItemDetail(
    Guid ItemId,
    string Name,
    bool Checked,
    string? Quantity,
    string? Note,
    int Order,
    DateTime UpdatedAtUtc,
    Guid? UpdatedByMemberId);

public sealed record GetSharedListDetailResponse(
    Guid ListId,
    string Name,
    string Kind,
    Guid? AreaId,
    string? LinkedEntityType,
    Guid? LinkedEntityId,
    IReadOnlyList<SharedListItemDetail> Items);
