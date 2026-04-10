namespace DomusMind.Contracts.Lists;

public sealed record ListItemDetail(
    Guid ItemId,
    string Name,
    bool Checked,
    string? Quantity,
    string? Note,
    int Order);

public sealed record GetListDetailResponse(
    Guid ListId,
    string Name,
    string Kind,
    Guid? AreaId,
    Guid? LinkedPlanId,
    string? LinkedPlanDisplayName,
    int UncheckedCount,
    IReadOnlyList<ListItemDetail> Items);
