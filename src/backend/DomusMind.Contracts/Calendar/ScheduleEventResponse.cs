namespace DomusMind.Contracts.Calendar;

public sealed record ScheduleEventResponse(
    Guid CalendarEventId,
    Guid FamilyId,
    string Title,
    string Date,
    string? Time,
    string? EndDate,
    string? EndTime,
    string Status,
    string Color,
    DateTime CreatedAtUtc);
