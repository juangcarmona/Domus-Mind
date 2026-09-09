namespace DomusMind.Contracts.Lists;

public sealed record ReorderListItemsRequest(IReadOnlyList<Guid> ItemIds);
