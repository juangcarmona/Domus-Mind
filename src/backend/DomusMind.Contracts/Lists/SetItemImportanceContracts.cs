namespace DomusMind.Contracts.Lists;

public sealed record SetItemImportanceRequest(bool Importance);

public sealed record SetItemImportanceResponse(
    Guid ItemId,
    bool Importance,
    DateTime UpdatedAtUtc);
