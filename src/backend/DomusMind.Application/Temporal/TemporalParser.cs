using DomusMind.Domain.Calendar.ValueObjects;
using DomusMind.Domain.Tasks.ValueObjects;

namespace DomusMind.Application.Temporal;

/// <summary>
/// Parses and formats ISO temporal strings for use in application commands and responses.
/// Domain stores: LocalDate → YYYY-MM-DD, LocalTime → HH:mm.
/// Formatting is a UI concern; this layer only validates and round-trips ISO values.
/// </summary>
internal static class TemporalParser
{
    private const string DateFormat = "yyyy-MM-dd";
    private const string TimeFormat = "HH:mm";

    // ── EventTime ──────────────────────────────────────────────────────────────

    public static EventTime ParseEventTime(
        string date,
        string? time,
        string? endDate,
        string? endTime)
    {
        var parsedDate = ParseDate(date, nameof(date));

        if (time is null)
        {
            // Date-only: either a single day or a multi-day range (no time component).
            if (endDate is not null)
            {
                var parsedDayEndDate = ParseDate(endDate, nameof(endDate));
                return EventTime.DayRange(parsedDate, parsedDayEndDate);
            }
            return EventTime.Day(parsedDate);
        }

        var parsedTime = ParseTime(time, nameof(time));

        if (endDate is null)
            return EventTime.Moment(parsedDate, parsedTime);

        var parsedEndDate = ParseDate(endDate, nameof(endDate));
        var parsedEndTime = endTime is not null
            ? ParseTime(endTime, nameof(endTime))
            : throw new ArgumentException("EndTime is required when EndDate is provided.", nameof(endTime));

        return EventTime.Range(parsedDate, parsedTime, parsedEndDate, parsedEndTime);
    }

    public static (string Date, string? Time, string? EndDate, string? EndTime) FormatEventTime(EventTime eventTime)
    {
        var date = eventTime.Date.ToString(DateFormat);
        var time = eventTime.Time.HasValue ? eventTime.Time.Value.ToString(TimeFormat) : null;
        var endDate = eventTime.EndDate.HasValue ? eventTime.EndDate.Value.ToString(DateFormat) : null;
        var endTime = eventTime.EndTime.HasValue ? eventTime.EndTime.Value.ToString(TimeFormat) : null;

        return (date, time, endDate, endTime);
    }

    // ── TaskSchedule ───────────────────────────────────────────────────────────

    public static TaskSchedule ParseTaskSchedule(string? dueDate, string? dueTime)
    {
        if (dueDate is null)
            return TaskSchedule.NoSchedule();

        var parsedDate = ParseDate(dueDate, nameof(dueDate));

        if (dueTime is null)
            return TaskSchedule.WithDueDate(parsedDate);

        var parsedTime = ParseTime(dueTime, nameof(dueTime));
        return TaskSchedule.WithDueDateTime(parsedDate, parsedTime);
    }

    public static (string? DueDate, string? DueTime) FormatTaskSchedule(TaskSchedule schedule)
    {
        var date = schedule.Date.HasValue ? schedule.Date.Value.ToString(DateFormat) : null;
        var time = schedule.Time.HasValue ? schedule.Time.Value.ToString(TimeFormat) : null;
        return (date, time);
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private static DateOnly ParseDate(string value, string paramName)
    {
        if (!DateOnly.TryParseExact(value, DateFormat, out var result))
            throw new FormatException($"'{paramName}' must be a valid date in YYYY-MM-DD format. Got: '{value}'.");
        return result;
    }

    private static TimeOnly ParseTime(string value, string paramName)
    {
        if (!TimeOnly.TryParseExact(value, TimeFormat, out var result))
            throw new FormatException($"'{paramName}' must be a valid time in HH:mm format. Got: '{value}'.");
        return result;
    }
}
