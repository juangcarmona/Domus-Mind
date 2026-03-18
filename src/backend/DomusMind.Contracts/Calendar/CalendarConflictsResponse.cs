namespace DomusMind.Contracts.Calendar;

public sealed record CalendarConflict(
    Guid EventAId,
    string EventATitle,
    string EventADate,
    string? EventATime,
    Guid EventBId,
    string EventBTitle,
    string EventBDate,
    string? EventBTime,
    IReadOnlyCollection<Guid> SharedParticipantIds);

public sealed record CalendarConflictsResponse(
    IReadOnlyCollection<CalendarConflict> Conflicts);
