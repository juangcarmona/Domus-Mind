namespace DomusMind.Domain.Calendar.ExternalConnections;

/// <summary>
/// Integration storage record for an external calendar event occurrence.
/// Not a domain aggregate — no behavioral lifecycle, no domain events.
/// Populated by sync; projected read-only into MemberAgenda.
/// </summary>
public sealed class ExternalCalendarEntry
{
    public Guid Id { get; init; }
    public Guid ConnectionId { get; init; }
    public Guid FeedId { get; init; }
    public string Provider { get; init; } = default!;
    public string ExternalEventId { get; set; } = default!;
    public string? ICalUId { get; set; }
    public string? SeriesMasterId { get; set; }
    public string Title { get; set; } = default!;
    public DateTime StartsAtUtc { get; set; }
    public DateTime? EndsAtUtc { get; set; }
    public string? OriginalTimezone { get; set; }
    public bool IsAllDay { get; set; }
    public string? Location { get; set; }
    public string? ParticipantSummaryJson { get; set; }
    public string Status { get; set; } = default!;
    public string? RawPayloadHash { get; set; }
    public DateTime? ProviderModifiedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public string? OpenInProviderUrl { get; set; }
    public DateTime LastSeenAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime UpdatedAtUtc { get; set; }
}
