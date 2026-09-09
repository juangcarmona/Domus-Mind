namespace DomusMind.Application.Abstractions.Integrations.Calendar;

/// <summary>
/// Represents an event occurrence returned from a provider calendar.
/// </summary>
public sealed record ExternalCalendarProviderEvent(
    string ExternalEventId,
    string? ICalUId,
    string? SeriesMasterId,
    string Title,
    DateTime StartsAtUtc,
    DateTime? EndsAtUtc,
    bool IsAllDay,
    string? Location,
    string? ParticipantSummaryJson,
    string Status,
    string? OpenInProviderUrl,
    DateTime? ProviderModifiedAtUtc,
    bool IsDeleted,
    /// <summary>
    /// IANA or Windows timezone ID from the source event (e.g. "America/New_York").
    /// Used to render event times in the household's local time context.
    /// Null when the source reported UTC or did not carry timezone metadata.
    /// </summary>
    string? OriginalTimezone = null);
