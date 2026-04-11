namespace DomusMind.Contracts.Calendar;

public sealed record MemberAgendaItem(
    string Type,
    string Title,
    DateTime StartsAtUtc,
    DateTime? EndsAtUtc,
    bool AllDay,
    string Status,
    bool IsReadOnly,
    Guid? EventId,
    Guid? TaskId,
    Guid? RoutineId,
    Guid? ConnectionId,
    string? CalendarId,
    string? ExternalEventId,
    string? Provider,
    string? ProviderLabel,
    string? OpenInProviderUrl,
    string? Location,
    string? ParticipantSummary,
    DateTime? SourceLastModifiedUtc,
    Guid? ListId,
    Guid? ListItemId,
    // list-item specific fields — null for all other types
    string? ListName,
    bool? Importance,
    DateOnly? DueDate,
    DateTimeOffset? Reminder);
