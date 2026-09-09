namespace DomusMind.Contracts.Lists;

public sealed record ListSummary(
    Guid Id,
    string Name,
    string Kind,
    Guid? AreaId,
    Guid? LinkedPlanId,
    int UncheckedCount);

public sealed record GetFamilyListsResponse(
    IReadOnlyList<ListSummary> Lists);
