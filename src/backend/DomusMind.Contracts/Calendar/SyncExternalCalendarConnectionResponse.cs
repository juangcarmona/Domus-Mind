namespace DomusMind.Contracts.Calendar;

public sealed record SyncExternalCalendarConnectionResponse(
    Guid ConnectionId,
    int SelectedFeedCount,
    int SyncedFeedCount,
    int ImportedEntryCount,
    int UpdatedEntryCount,
    int DeletedEntryCount,
    string Status,
    DateTime? LastSuccessfulSyncUtc);
