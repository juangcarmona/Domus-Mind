namespace DomusMind.Contracts.Lists;

public sealed record AddItemToListRequest(
    string Name,
    string? Quantity,
    string? Note);
