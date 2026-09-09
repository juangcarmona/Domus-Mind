namespace DomusMind.Domain.Calendar.ExternalConnections;

/// <summary>
/// Defines the bounded window for external calendar sync.
/// History: now - 1 day. Forward: configurable horizon in days.
/// </summary>
public sealed class SyncHorizon
{
    public static readonly int[] AllowedForwardDays = [30, 90, 180, 365];
    public const int DefaultForwardDays = 90;
    public const int HistoryDays = 1;

    public int ForwardHorizonDays { get; }

    private SyncHorizon(int forwardHorizonDays)
    {
        ForwardHorizonDays = forwardHorizonDays;
    }

    // Required by EF Core OwnsOne materialization
    private SyncHorizon() { }

    public static SyncHorizon Default() => new(DefaultForwardDays);

    public static SyncHorizon Create(int forwardHorizonDays)
    {
        if (!Array.Exists(AllowedForwardDays, d => d == forwardHorizonDays))
            throw new InvalidOperationException(
                $"ForwardHorizonDays must be one of {string.Join(", ", AllowedForwardDays)}. Got: {forwardHorizonDays}");

        return new SyncHorizon(forwardHorizonDays);
    }

    public DateTime ComputeWindowStart(DateTime nowUtc) => nowUtc.AddDays(-HistoryDays);
    public DateTime ComputeWindowEnd(DateTime nowUtc) => nowUtc.AddDays(ForwardHorizonDays);
}
