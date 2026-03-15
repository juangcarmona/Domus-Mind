namespace DomusMind.Contracts.Calendar;

public sealed record RescheduleEventResponse(
    Guid CalendarEventId,
    string Title,
    DateTime NewStartTime,
    DateTime? NewEndTime,
    string Status);
