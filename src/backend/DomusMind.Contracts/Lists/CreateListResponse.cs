namespace DomusMind.Contracts.Lists;

public sealed record CreateListResponse(
    Guid ListId,
    Guid FamilyId,
    string Name,
    string Kind,
    Guid? AreaId,
    Guid? LinkedPlanId,
    DateTime CreatedAtUtc);
