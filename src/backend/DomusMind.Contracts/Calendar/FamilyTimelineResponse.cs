namespace DomusMind.Contracts.Calendar;

public sealed record ParticipantProjection(Guid MemberId, string DisplayName);

public sealed record FamilyTimelineEventItem(
    Guid CalendarEventId,
    string Title,
    string Date,
    string? Time,
    string? EndDate,
    string? EndTime,
    string Status,
    string Color,
    IReadOnlyCollection<Guid> ParticipantMemberIds,
    IReadOnlyCollection<ParticipantProjection> Participants);

public sealed record FamilyTimelineResponse(
    IReadOnlyCollection<FamilyTimelineEventItem> Events);
