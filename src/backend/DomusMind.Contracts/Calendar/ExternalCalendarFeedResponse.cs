namespace DomusMind.Contracts.Calendar;

public sealed record ExternalCalendarFeedResponse(
    string CalendarId,
    string CalendarName,
    bool IsSelected,
    DateTime? LastSuccessfulSyncUtc,
    DateTime? WindowStartUtc,
    DateTime? WindowEndUtc);
