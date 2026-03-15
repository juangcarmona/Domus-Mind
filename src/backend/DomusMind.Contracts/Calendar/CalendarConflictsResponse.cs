namespace DomusMind.Contracts.Calendar;

public sealed record CalendarConflict(
    Guid EventAId,
    string EventATitle,
    DateTime EventAStart,
    Guid EventBId,
    string EventBTitle,
    DateTime EventBStart,
    IReadOnlyCollection<Guid> SharedParticipantIds);

public sealed record CalendarConflictsResponse(
    IReadOnlyCollection<CalendarConflict> Conflicts);
