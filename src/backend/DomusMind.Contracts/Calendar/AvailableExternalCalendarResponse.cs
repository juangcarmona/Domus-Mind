namespace DomusMind.Contracts.Calendar;

public sealed record AvailableExternalCalendarResponse(
    string CalendarId,
    string CalendarName,
    bool IsDefault,
    bool IsSelected,
    string? ColorHex = null);
