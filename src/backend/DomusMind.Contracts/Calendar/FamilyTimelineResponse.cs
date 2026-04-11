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
    Guid? AreaId,
    IReadOnlyCollection<Guid> ParticipantMemberIds,
    IReadOnlyCollection<ParticipantProjection> Participants);

public sealed record FamilyTimelineListItem(
    Guid ListId,
    string ListName,
    Guid ItemId,
    string ItemName,
    string? DueDate,
    string? Reminder,
    bool Checked,
    bool Importance);

public sealed record FamilyTimelineResponse(
    IReadOnlyCollection<FamilyTimelineEventItem> Events,
    IReadOnlyCollection<FamilyTimelineListItem> ListItems);
