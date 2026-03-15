namespace DomusMind.Contracts.Calendar;

public sealed record RemoveEventParticipantResponse(
    Guid CalendarEventId,
    Guid MemberId);
