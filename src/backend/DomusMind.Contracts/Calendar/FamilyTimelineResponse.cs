namespace DomusMind.Contracts.Calendar;

public sealed record FamilyTimelineEventItem(
    Guid CalendarEventId,
    string Title,
    DateTime StartTime,
    DateTime? EndTime,
    string Status,
    IReadOnlyCollection<Guid> ParticipantMemberIds);

public sealed record FamilyTimelineResponse(
    IReadOnlyCollection<FamilyTimelineEventItem> Events);
