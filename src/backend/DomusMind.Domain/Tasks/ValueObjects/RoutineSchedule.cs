using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Tasks.Enums;

namespace DomusMind.Domain.Tasks.ValueObjects;

public sealed class RoutineSchedule : ValueObject
{
    public RoutineFrequency Frequency { get; }
    public IReadOnlyCollection<DayOfWeek> DaysOfWeek { get; }
    public IReadOnlyCollection<int> DaysOfMonth { get; }
    public int? MonthOfYear { get; }
    public TimeOnly? Time { get; }
    public TimeOnly? EndTime { get; }

    private RoutineSchedule(
        RoutineFrequency frequency,
        IReadOnlyCollection<DayOfWeek> daysOfWeek,
        IReadOnlyCollection<int> daysOfMonth,
        int? monthOfYear,
        TimeOnly? time,
        TimeOnly? endTime)
    {
        Frequency = frequency;
        DaysOfWeek = daysOfWeek;
        DaysOfMonth = daysOfMonth;
        MonthOfYear = monthOfYear;
        Time = time;
        EndTime = endTime;
    }

    private static void ValidateTimes(TimeOnly? time, TimeOnly? endTime)
    {
        if (endTime.HasValue && !time.HasValue)
            throw new InvalidOperationException("End time requires a start time.");

        if (endTime.HasValue && time.HasValue && endTime.Value <= time.Value)
            throw new InvalidOperationException("End time must be after start time.");
    }

    public static RoutineSchedule Daily(TimeOnly? time = null, TimeOnly? endTime = null)
    {
        ValidateTimes(time, endTime);
        return new RoutineSchedule(RoutineFrequency.Daily, Array.Empty<DayOfWeek>(), Array.Empty<int>(), null, time, endTime);
    }

    public static RoutineSchedule Weekly(IEnumerable<DayOfWeek> daysOfWeek, TimeOnly? time = null, TimeOnly? endTime = null)
    {
        var values = daysOfWeek.Distinct().ToArray();
        if (values.Length == 0)
            throw new InvalidOperationException("Weekly routine must define at least one day of week.");

        ValidateTimes(time, endTime);
        return new RoutineSchedule(RoutineFrequency.Weekly, values, Array.Empty<int>(), null, time, endTime);
    }

    public static RoutineSchedule Monthly(IEnumerable<int> daysOfMonth, TimeOnly? time = null, TimeOnly? endTime = null)
    {
        var values = daysOfMonth.Distinct().OrderBy(x => x).ToArray();
        if (values.Length == 0 || values.Any(x => x < 1 || x > 31))
            throw new InvalidOperationException("Monthly routine must define valid day(s) of month.");

        ValidateTimes(time, endTime);
        return new RoutineSchedule(RoutineFrequency.Monthly, Array.Empty<DayOfWeek>(), values, null, time, endTime);
    }

    public static RoutineSchedule Yearly(int monthOfYear, IEnumerable<int> daysOfMonth, TimeOnly? time = null, TimeOnly? endTime = null)
    {
        var values = daysOfMonth.Distinct().OrderBy(x => x).ToArray();

        if (monthOfYear < 1 || monthOfYear > 12)
            throw new InvalidOperationException("Yearly routine must define a valid month.");

        if (values.Length == 0 || values.Any(x => x < 1 || x > 31))
            throw new InvalidOperationException("Yearly routine must define valid day(s) of month.");

        ValidateTimes(time, endTime);
        return new RoutineSchedule(RoutineFrequency.Yearly, Array.Empty<DayOfWeek>(), values, monthOfYear, time, endTime);
    }

    public bool OccursOn(DateOnly date)
    {
        return Frequency switch
        {
            RoutineFrequency.Daily => true,
            RoutineFrequency.Weekly => DaysOfWeek.Contains(date.DayOfWeek),
            RoutineFrequency.Monthly => DaysOfMonth.Contains(date.Day),
            RoutineFrequency.Yearly => MonthOfYear == date.Month && DaysOfMonth.Contains(date.Day),
            _ => false
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Frequency;
        foreach (var day in DaysOfWeek) yield return day;
        foreach (var day in DaysOfMonth) yield return day;
        yield return MonthOfYear;
        yield return Time;
        yield return EndTime;
    }

#pragma warning disable CS8618
    private RoutineSchedule() { }
#pragma warning restore CS8618
}