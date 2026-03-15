namespace DomusMind.Contracts.Calendar;

public sealed record ScheduleEventResponse(
    Guid CalendarEventId,
    Guid FamilyId,
    string Title,
    DateTime StartTime,
    DateTime? EndTime,
    string Status,
    DateTime CreatedAtUtc);
