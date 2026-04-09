namespace DomusMind.Contracts.Calendar;

public sealed record ExternalCalendarConnectionDetailResponse(
    Guid ConnectionId,
    Guid MemberId,
    string Provider,
    string ProviderLabel,
    string AccountEmail,
    string? AccountDisplayLabel,
    string? TenantId,
    int ForwardHorizonDays,
    bool ScheduledRefreshEnabled,
    int ScheduledRefreshIntervalMinutes,
    DateTime? LastSuccessfulSyncUtc,
    DateTime? LastSyncAttemptUtc,
    DateTime? LastSyncFailureUtc,
    string Status,
    bool IsSyncInProgress,
    int ImportedEntryCount,
    IReadOnlyCollection<ExternalCalendarFeedResponse> Feeds,
    IReadOnlyCollection<AvailableExternalCalendarResponse> AvailableCalendars,
    string? LastErrorCode,
    string? LastErrorMessage);
