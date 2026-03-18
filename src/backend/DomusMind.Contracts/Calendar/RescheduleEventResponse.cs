namespace DomusMind.Contracts.Calendar;

public sealed record RescheduleEventResponse(
    Guid CalendarEventId,
    string Title,
    string Date,
    string? Time,
    string? EndDate,
    string? EndTime,
    string Status);
