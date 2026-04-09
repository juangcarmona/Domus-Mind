namespace DomusMind.Application.Abstractions.Integrations.Calendar;

/// <summary>
/// Represents the result of a provider calendar list discovery.
/// </summary>
public sealed record ExternalCalendarProviderCalendar(
    string CalendarId,
    string CalendarName,
    bool IsDefault,
    /// <summary>
    /// Provider-supplied hex color (e.g. "#A9BCF5") from the calendar's hexColor field.
    /// Null when the provider does not supply one.
    /// </summary>
    string? ColorHex = null);
