namespace DomusMind.Contracts.Lists;

public sealed record ToggleListItemResponse(
    Guid ItemId,
    bool Checked,
    int UncheckedCount);
