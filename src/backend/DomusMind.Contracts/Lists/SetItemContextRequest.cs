namespace DomusMind.Contracts.Lists;

public sealed record SetItemContextRequest(
    Guid? ItemAreaId,
    Guid? TargetMemberId);

public sealed record SetItemContextResponse(
    Guid ItemId,
    Guid? ItemAreaId,
    Guid? TargetMemberId,
    DateTime UpdatedAtUtc);
