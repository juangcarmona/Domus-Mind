namespace DomusMind.Contracts.Calendar;

public sealed record ConfigureExternalCalendarConnectionRequest(
    IReadOnlyCollection<ExternalCalendarSelectionRequest> SelectedCalendars,
    int ForwardHorizonDays,
    bool ScheduledRefreshEnabled,
    int ScheduledRefreshIntervalMinutes);
