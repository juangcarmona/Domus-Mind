using DomusMind.Application.Abstractions.Integrations.Calendar;

namespace DomusMind.Application.Abstractions.Integrations.Calendar;

/// <summary>
/// Fetches calendars and events from an external provider (Microsoft Graph).
/// </summary>
public interface IExternalCalendarProviderClient
{
    /// <summary>Retrieves available calendars for the authenticated account.</summary>
    Task<IReadOnlyCollection<ExternalCalendarProviderCalendar>> GetCalendarsAsync(
        string accessToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs the initial bounded load for a calendar using calendarView.
    /// Returns all event pages plus the terminal delta token.
    /// </summary>
    IAsyncEnumerable<ExternalCalendarProviderDeltaPage> GetInitialEventsAsync(
        string accessToken,
        string calendarId,
        DateTime windowStartUtc,
        DateTime windowEndUtc,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs an incremental delta sync using the stored delta token.
    /// </summary>
    IAsyncEnumerable<ExternalCalendarProviderDeltaPage> GetDeltaEventsAsync(
        string accessToken,
        string calendarId,
        string deltaToken,
        CancellationToken cancellationToken = default);
}
