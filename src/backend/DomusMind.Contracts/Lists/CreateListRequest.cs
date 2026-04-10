namespace DomusMind.Contracts.Lists;

public sealed record CreateListRequest(
    Guid FamilyId,
    string Name,
    string? Kind,
    Guid? AreaId,
    Guid? LinkedPlanId);
