namespace DomusMind.Contracts.Lists;

public sealed record UpdateListRequest(
    string? Name,
    Guid? AreaId,
    bool ClearArea,
    Guid? LinkedPlanId,
    bool ClearLinkedPlan,
    string? Kind,
    string? Color,
    bool ClearColor);

public sealed record UpdateListResponse(
    Guid ListId,
    string Name,
    string? Color,
    Guid? AreaId,
    Guid? LinkedPlanId,
    string Kind);
