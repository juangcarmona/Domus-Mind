namespace DomusMind.Infrastructure.Persistence;

public sealed class EventLogEntry
{
    public Guid EventId { get; init; }

    public string EventType { get; init; } = default!;

    public string AggregateType { get; init; } = default!;

    public string AggregateId { get; init; } = default!;

    public string Module { get; init; } = default!;

    public DateTime OccurredAtUtc { get; init; }

    public int Version { get; init; }

    public string PayloadJson { get; init; } = default!;

    public string? CorrelationId { get; init; }

    public string? CausationId { get; init; }
}