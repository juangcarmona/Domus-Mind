using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Calendar.ExternalConnections;

/// <summary>
/// Represents a provider calendar selected for sync under an ExternalCalendarConnection.
/// Internal entity — not an aggregate root.
/// </summary>
public sealed class ExternalCalendarFeed : Entity<Guid>
{
    public ExternalCalendarConnectionId ConnectionId { get; private set; }
    public string ProviderCalendarId { get; private set; }
    public string CalendarName { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsSelected { get; private set; }
    /// <summary>
    /// Hex color supplied by the provider (e.g. "#A9BCF5").
    /// Null when the provider did not supply a color or color was not fetched yet.
    /// </summary>
    public string? ColorHex { get; private set; }
    public DateTime? WindowStartUtc { get; private set; }
    public DateTime? WindowEndUtc { get; private set; }
    public string? LastDeltaToken { get; private set; }
    public DateTime? LastSuccessfulSyncUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private ExternalCalendarFeed(
        Guid id,
        ExternalCalendarConnectionId connectionId,
        string providerCalendarId,
        string calendarName,
        bool isDefault,
        bool isSelected,
        string? colorHex,
        DateTime createdAtUtc)
        : base(id)
    {
        ConnectionId = connectionId;
        ProviderCalendarId = providerCalendarId;
        CalendarName = calendarName;
        IsDefault = isDefault;
        IsSelected = isSelected;
        ColorHex = colorHex;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    // Required by EF Core for materialization
    private ExternalCalendarFeed() : base(Guid.Empty)
    {
        ProviderCalendarId = string.Empty;
        CalendarName = string.Empty;
    }

    public static ExternalCalendarFeed Create(
        ExternalCalendarConnectionId connectionId,
        string providerCalendarId,
        string calendarName,
        bool isDefault,
        bool isSelected,
        DateTime createdAtUtc,
        string? colorHex = null)
    {
        return new ExternalCalendarFeed(
            Guid.NewGuid(), connectionId, providerCalendarId, calendarName, isDefault, isSelected, colorHex, createdAtUtc);
    }

    public void UpdateSelection(string calendarName, bool isSelected, DateTime nowUtc, string? colorHex = null)
    {
        CalendarName = calendarName;
        IsSelected = isSelected;
        if (colorHex is not null)
            ColorHex = colorHex;
        UpdatedAtUtc = nowUtc;

        if (!isSelected)
        {
            // Clear sync state when deselected; delta token no longer valid for this window.
            LastDeltaToken = null;
            WindowStartUtc = null;
            WindowEndUtc = null;
        }
    }

    public void RecordSyncWindow(DateTime windowStartUtc, DateTime windowEndUtc, DateTime nowUtc)
    {
        WindowStartUtc = windowStartUtc;
        WindowEndUtc = windowEndUtc;
        UpdatedAtUtc = nowUtc;
    }

    public void RecordDeltaToken(string deltaToken, DateTime nowUtc)
    {
        LastDeltaToken = deltaToken;
        UpdatedAtUtc = nowUtc;
    }

    public void RecordSuccessfulSync(DateTime nowUtc)
    {
        LastSuccessfulSyncUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    public void InvalidateDeltaState(DateTime nowUtc)
    {
        LastDeltaToken = null;
        WindowStartUtc = null;
        WindowEndUtc = null;
        UpdatedAtUtc = nowUtc;
    }
}
