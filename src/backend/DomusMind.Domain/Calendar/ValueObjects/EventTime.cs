using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Calendar.ValueObjects;

public enum EventTimeKind { Day, DayRange, Moment, Range }

/// <summary>
/// First-class temporal value for a calendar event.
/// A Day carries only a date (no implicit midnight).
/// A Moment carries a date + time (minute precision, no seconds).
/// A Range carries start date+time and end date+time.
/// </summary>
public sealed class EventTime : ValueObject
{
    public EventTimeKind Kind { get; }

    /// <summary>The event date (or range start date).</summary>
    public DateOnly Date { get; }

    /// <summary>Optional start time (minute precision). Null for Day events.</summary>
    public TimeOnly? Time { get; }

    /// <summary>Optional end date. Only for Range events.</summary>
    public DateOnly? EndDate { get; }

    /// <summary>Optional end time. Only for Range events.</summary>
    public TimeOnly? EndTime { get; }

    private EventTime(EventTimeKind kind, DateOnly date, TimeOnly? time, DateOnly? endDate, TimeOnly? endTime)
    {
        Kind = kind;
        Date = date;
        Time = time;
        EndDate = endDate;
        EndTime = endTime;
    }

    /// <summary>Creates a date-only event covering a single day (no time component).</summary>
    public static EventTime Day(DateOnly date)
        => new(EventTimeKind.Day, date, null, null, null);

    /// <summary>Creates a date-only event spanning multiple days (no time component).</summary>
    public static EventTime DayRange(DateOnly startDate, DateOnly endDate)
    {
        if (endDate < startDate)
            throw new ArgumentException("End date must not be before start date.", nameof(endDate));
        return new(EventTimeKind.DayRange, startDate, null, endDate, null);
    }

    /// <summary>Creates an event with an exact date and time (minute precision).</summary>
    public static EventTime Moment(DateOnly date, TimeOnly time)
    {
        if (time.Second != 0 || time.Millisecond != 0)
            throw new ArgumentException("Event time must have minute-level precision (no seconds).", nameof(time));

        return new(EventTimeKind.Moment, date, time, null, null);
    }

    /// <summary>Creates an event with a start and end date+time (minute precision).</summary>
    public static EventTime Range(DateOnly startDate, TimeOnly startTime, DateOnly endDate, TimeOnly endTime)
    {
        if (startTime.Second != 0 || startTime.Millisecond != 0)
            throw new ArgumentException("Start time must have minute-level precision (no seconds).", nameof(startTime));

        if (endTime.Second != 0 || endTime.Millisecond != 0)
            throw new ArgumentException("End time must have minute-level precision (no seconds).", nameof(endTime));

        var start = startDate.ToDateTime(startTime);
        var end = endDate.ToDateTime(endTime);

        if (end <= start)
            throw new InvalidOperationException("End time must be after start time.");

        return new(EventTimeKind.Range, startDate, startTime, endDate, endTime);
    }

    public bool HasTime => Kind != EventTimeKind.Day && Kind != EventTimeKind.DayRange;
    public bool HasRange => Kind == EventTimeKind.Range || Kind == EventTimeKind.DayRange;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Kind;
        yield return Date;
        yield return Time;
        yield return EndDate;
        yield return EndTime;
    }

#pragma warning disable CS8618
    // EF Core parameterless constructor
    private EventTime() { }
#pragma warning restore CS8618
}
