namespace DomusMind.Contracts.SharedLists;

public sealed record ToggleSharedListItemResponse(
    Guid ItemId,
    bool Checked,
    DateTime UpdatedAtUtc,
    Guid? UpdatedByMemberId,
    int UncheckedCount);
