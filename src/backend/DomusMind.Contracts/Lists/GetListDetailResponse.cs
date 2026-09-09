namespace DomusMind.Contracts.Lists;

public sealed record ListItemDetail(
    Guid ItemId,
    string Name,
    bool Checked,
    string? Quantity,
    string? Note,
    int Order,
    bool Importance,
    DateOnly? DueDate,
    DateTimeOffset? Reminder,
    string? Repeat,
    Guid? ItemAreaId = null,
    Guid? TargetMemberId = null);

public sealed record GetListDetailResponse(
    Guid ListId,
    string Name,
    string Kind,
    string? Color,
    Guid? AreaId,
    Guid? LinkedPlanId,
    string? LinkedPlanDisplayName,
    int UncheckedCount,
    IReadOnlyList<ListItemDetail> Items);
