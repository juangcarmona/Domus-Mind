namespace DomusMind.Domain.Tasks.ValueObjects;

public sealed record RoutineColor
{
    public string Value { get; }

    private RoutineColor(string value) => Value = value;

    public static RoutineColor From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("Routine color cannot be empty.");

        var normalized = value.Trim();

        // keep validation simple for now
        if (!System.Text.RegularExpressions.Regex.IsMatch(normalized, "^#([0-9A-Fa-f]{6})$"))
            throw new InvalidOperationException("Routine color must be a hex color like #AABBCC.");

        return new RoutineColor(normalized.ToUpperInvariant());
    }
}