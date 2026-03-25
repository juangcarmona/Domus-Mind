namespace DomusMind.Contracts.SharedLists;

public sealed record ReorderSharedListItemsRequest(IReadOnlyList<Guid> ItemIds);
