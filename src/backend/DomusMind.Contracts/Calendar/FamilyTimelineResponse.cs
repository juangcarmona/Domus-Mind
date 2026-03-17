namespace DomusMind.Contracts.Calendar;

public sealed record ParticipantProjection(Guid MemberId, string DisplayName);

public sealed record FamilyTimelineEventItem(
    Guid CalendarEventId,
    string Title,
    DateTime StartTime,
    DateTime? EndTime,
    string Status,
    IReadOnlyCollection<Guid> ParticipantMemberIds,
    IReadOnlyCollection<ParticipantProjection> Participants);

public sealed record FamilyTimelineResponse(
    IReadOnlyCollection<FamilyTimelineEventItem> Events);
