namespace DomusMind.Infrastructure.BackgroundJobs.Calendar;

public sealed class ExternalCalendarRefreshOptions
{
    public const string SectionName = "ExternalCalendarRefresh";

    /// <summary>How often the worker wakes to check for due connections (seconds).</summary>
    public int WorkerCycleSeconds { get; set; } = 300;

    /// <summary>Default sync interval per connection (minutes).</summary>
    public int DefaultRefreshIntervalMinutes { get; set; } = 60;

    /// <summary>Max jitter added to avoid herd behavior (seconds).</summary>
    public int JitterMaxSeconds { get; set; } = 300;

    /// <summary>How many connections to process per worker cycle.</summary>
    public int BatchSize { get; set; } = 10;
}
