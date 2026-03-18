using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Tasks.ValueObjects;

public enum TaskScheduleKind { None, DueDate, DueDateTime }

/// <summary>
/// First-class temporal schedule for a household task.
/// None  = no schedule set.
/// DueDate = date-only deadline (no implied time).
/// DueDateTime = exact date + time deadline (minute precision, no seconds).
/// </summary>
public sealed class TaskSchedule : ValueObject
{
    public TaskScheduleKind Kind { get; }

    /// <summary>Optional due date. Null when Kind is None.</summary>
    public DateOnly? Date { get; }

    /// <summary>Optional due time (minute precision). Only for DueDateTime.</summary>
    public TimeOnly? Time { get; }

    private TaskSchedule(TaskScheduleKind kind, DateOnly? date, TimeOnly? time)
    {
        Kind = kind;
        Date = date;
        Time = time;
    }

    /// <summary>No schedule – task has no due date.</summary>
    public static TaskSchedule NoSchedule()
        => new(TaskScheduleKind.None, null, null);

    /// <summary>Task is due on a specific date (no implied time).</summary>
    public static TaskSchedule WithDueDate(DateOnly date)
        => new(TaskScheduleKind.DueDate, date, null);

    /// <summary>Task is due at an exact date and time (minute precision).</summary>
    public static TaskSchedule WithDueDateTime(DateOnly date, TimeOnly time)
    {
        if (time.Second != 0 || time.Millisecond != 0)
            throw new ArgumentException("Due time must have minute-level precision (no seconds).", nameof(time));

        return new(TaskScheduleKind.DueDateTime, date, time);
    }

    public bool HasSchedule => Kind != TaskScheduleKind.None;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Kind;
        yield return Date;
        yield return Time;
    }

#pragma warning disable CS8618
    // EF Core parameterless constructor
    private TaskSchedule() { }
#pragma warning restore CS8618
}
