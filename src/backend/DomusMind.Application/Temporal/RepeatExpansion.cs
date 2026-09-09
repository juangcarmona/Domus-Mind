namespace DomusMind.Application.Temporal;

/// <summary>
/// Evaluates whether a list-item repeat rule fires within a date window.
/// Repeat format: "Daily" | "Weekly:0,1,2" | "Monthly" | "Yearly"
/// Day-of-week encoding: 0=Sunday … 6=Saturday (matches .NET DayOfWeek cast to int).
/// </summary>
internal static class RepeatExpansion
{
    /// <summary>
    /// Returns the dates within [windowStart, windowEnd] on which the repeat pattern fires.
    /// Used to project repeat-only items (no dueDate, no reminder) into an Agenda window.
    /// </summary>
    public static IEnumerable<DateOnly> GetFireDates(string? repeat, DateOnly windowStart, DateOnly windowEnd)
    {
        if (repeat is null) yield break;

        var sep = repeat.IndexOf(':');
        var freq = sep >= 0 ? repeat[..sep] : repeat;
        var payload = sep >= 0 ? repeat[(sep + 1)..] : null;

        switch (freq)
        {
            case "Daily":
                for (var d = windowStart; d <= windowEnd; d = d.AddDays(1))
                    yield return d;
                break;

            case "Weekly" when payload is not null:
                var targetDays = payload.Split(',')
                    .Select(s => int.TryParse(s.Trim(), out var n) ? (int?)n : null)
                    .Where(n => n.HasValue)
                    .Select(n => n!.Value)
                    .ToHashSet();
                for (var d = windowStart; d <= windowEnd; d = d.AddDays(1))
                {
                    if (targetDays.Contains((int)d.DayOfWeek))
                        yield return d;
                }
                break;

            case "Monthly":
                // No dueDate anchor — fire on the same calendar day as windowStart each month.
                for (var d = windowStart; d <= windowEnd; d = d.AddDays(1))
                {
                    if (d.Day == windowStart.Day)
                        yield return d;
                }
                // Ensure single-day windows always get at least one entry.
                if (windowStart == windowEnd)
                    yield return windowStart;
                break;

            case "Yearly":
                // No dueDate anchor — surface once at the start of the window (V1 simplification).
                yield return windowStart;
                break;
        }
    }

    /// <summary>
    /// Returns true if the repeat pattern fires at least once in [windowStart, windowEnd].
    /// </summary>
    public static bool FiresInWindow(string? repeat, DateOnly windowStart, DateOnly windowEnd)
        => GetFireDates(repeat, windowStart, windowEnd).Any();
}
