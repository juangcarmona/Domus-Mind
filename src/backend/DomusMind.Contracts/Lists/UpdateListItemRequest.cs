namespace DomusMind.Contracts.Lists;

public sealed record UpdateListItemRequest(
    string Name,
    string? Quantity,
    string? Note);
