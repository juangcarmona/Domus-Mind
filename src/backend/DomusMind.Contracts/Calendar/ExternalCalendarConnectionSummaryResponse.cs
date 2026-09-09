namespace DomusMind.Contracts.Calendar;

public sealed record ExternalCalendarConnectionSummaryResponse(
    Guid ConnectionId,
    Guid MemberId,
    string Provider,
    string ProviderLabel,
    string AccountEmail,
    string? AccountDisplayLabel,
    int SelectedCalendarCount,
    int ForwardHorizonDays,
    bool ScheduledRefreshEnabled,
    int ScheduledRefreshIntervalMinutes,
    DateTime? LastSuccessfulSyncUtc,
    DateTime? LastSyncAttemptUtc,
    DateTime? LastSyncFailureUtc,
    string Status,
    bool IsSyncInProgress,
    int ImportedEntryCount,
    string? LastErrorCode,
    string? LastErrorMessage);
