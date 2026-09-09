namespace DomusMind.Contracts.Calendar;

public sealed record SyncMemberExternalCalendarConnectionsResponse(
    Guid MemberId,
    int RequestedConnectionCount,
    int AcceptedConnectionCount,
    int SkippedConnectionCount,
    string Status);
