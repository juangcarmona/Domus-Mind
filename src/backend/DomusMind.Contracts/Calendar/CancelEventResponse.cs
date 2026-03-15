namespace DomusMind.Contracts.Calendar;

public sealed record CancelEventResponse(
    Guid CalendarEventId,
    string Status);
