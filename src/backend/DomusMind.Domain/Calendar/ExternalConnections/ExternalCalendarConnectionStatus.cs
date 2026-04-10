namespace DomusMind.Domain.Calendar.ExternalConnections;

public enum ExternalCalendarConnectionStatus
{
    PendingInitialSync,
    Healthy,
    Syncing,
    NeedsAttention,
    PartialFailure,
    Failed,
    AuthExpired,
    Rehydrating,
    Disconnected
}

public static class ExternalCalendarConnectionStatusNames
{
    public static string ToStatusString(ExternalCalendarConnectionStatus status) => status switch
    {
        ExternalCalendarConnectionStatus.PendingInitialSync => "idle",
        ExternalCalendarConnectionStatus.Healthy => "success",
        ExternalCalendarConnectionStatus.Syncing => "syncing",
        ExternalCalendarConnectionStatus.NeedsAttention => "failed",
        ExternalCalendarConnectionStatus.PartialFailure => "partial_failure",
        ExternalCalendarConnectionStatus.Failed => "failed",
        ExternalCalendarConnectionStatus.AuthExpired => "auth_expired",
        ExternalCalendarConnectionStatus.Rehydrating => "rehydrating",
        ExternalCalendarConnectionStatus.Disconnected => "disconnected",
        _ => throw new ArgumentOutOfRangeException(nameof(status))
    };
}
