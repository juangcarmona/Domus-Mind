namespace DomusMind.Contracts.Calendar;

public sealed record RemoveReminderResponse(
    Guid CalendarEventId,
    int MinutesBefore);
