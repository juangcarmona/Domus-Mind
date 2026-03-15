namespace DomusMind.Contracts.Calendar;

public sealed record AddReminderResponse(
    Guid CalendarEventId,
    int MinutesBefore);
