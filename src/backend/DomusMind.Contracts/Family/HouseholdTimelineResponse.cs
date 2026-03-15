namespace DomusMind.Contracts.Family;

public sealed record HouseholdTimelineEntry(
    Guid EntryId,
    string EntryType,
    string Title,
    DateTime? EffectiveDate,
    string Status);

public sealed record HouseholdTimelineResponse(
    IReadOnlyCollection<HouseholdTimelineEntry> Entries);
