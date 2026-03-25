namespace DomusMind.Contracts.SharedLists;

public sealed record AddItemToSharedListRequest(
    string Name,
    string? Quantity,
    string? Note);
