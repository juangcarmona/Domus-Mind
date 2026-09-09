namespace DomusMind.Contracts.Calendar;

public sealed record ConfigureExternalCalendarConnectionResponse(
    Guid ConnectionId,
    int SelectedCalendarCount,
    int ForwardHorizonDays,
    bool ScheduledRefreshEnabled,
    int ScheduledRefreshIntervalMinutes,
    string Status);
