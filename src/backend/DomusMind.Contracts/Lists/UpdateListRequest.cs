namespace DomusMind.Contracts.Lists;

public sealed record UpdateListRequest(
    string? Name,
    Guid? AreaId,
    bool ClearArea,
    Guid? LinkedPlanId,
    bool ClearLinkedPlan,
    string? Kind);

public sealed record UpdateListResponse(
    Guid ListId,
    string Name,
    Guid? AreaId,
    Guid? LinkedPlanId,
    string Kind);
