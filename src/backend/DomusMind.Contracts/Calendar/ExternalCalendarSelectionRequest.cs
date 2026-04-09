namespace DomusMind.Contracts.Calendar;

public sealed record ExternalCalendarSelectionRequest(
    string CalendarId,
    string CalendarName,
    bool IsSelected,
    string? ColorHex = null);
