namespace DomusMind.Contracts.Calendar;

public sealed record AddEventParticipantResponse(
    Guid CalendarEventId,
    Guid MemberId);
