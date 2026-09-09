namespace DomusMind.Contracts.Lists;

public sealed record ClearItemTemporalResponse(
    Guid ItemId,
    DateTime UpdatedAtUtc);
