namespace DomusMind.Contracts.SharedLists;

public sealed record AddItemToSharedListResponse(
    Guid ItemId,
    Guid ListId,
    string Name,
    bool Checked,
    string? Quantity,
    string? Note,
    int Order);
