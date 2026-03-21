using DomusMind.Contracts.Calendar;

namespace DomusMind.Contracts.Family;

public sealed record EnrichedTimelineEntry(
    Guid EntryId,
    string EntryType,
    string Title,
    string? EffectiveDate,
    string Status,
    string Priority,
    string Group,
    bool IsOverdue,
    bool IsUnassigned,
    Guid? AssigneeId,
    IReadOnlyCollection<ParticipantProjection>? Participants,
    string Color);

public sealed record TimelineGroup(
    string GroupKey,
    IReadOnlyCollection<EnrichedTimelineEntry> Entries);

public sealed record EnrichedTimelineResponse(
    IReadOnlyCollection<TimelineGroup> Groups,
    int TotalEntries);
