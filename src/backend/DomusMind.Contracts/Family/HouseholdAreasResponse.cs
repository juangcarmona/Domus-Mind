namespace DomusMind.Contracts.Family;

public sealed record HouseholdAreaItem(
    Guid AreaId,
    string Name,
    string? Color);

public sealed record HouseholdAreasResponse(
    IReadOnlyCollection<HouseholdAreaItem> Areas);
