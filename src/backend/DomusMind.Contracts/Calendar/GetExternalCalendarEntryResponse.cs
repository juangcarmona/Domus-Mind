namespace DomusMind.Contracts.Calendar;

public sealed record GetExternalCalendarEntryResponse(
    Guid EntryId,
    string Title,
    string Date,
    string? Time,
    string? EndDate,
    string? EndTime,
    bool IsAllDay,
    string Status,
    string? Location,
    string? CalendarName,
    string? ProviderLabel,
    string? OpenInProviderUrl);
