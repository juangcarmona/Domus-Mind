namespace DomusMind.Contracts.SharedLists;

public sealed record ToggleSharedListItemRequest(
    Guid? UpdatedByMemberId);
