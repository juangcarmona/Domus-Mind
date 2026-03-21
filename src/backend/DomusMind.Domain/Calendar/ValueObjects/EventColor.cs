namespace DomusMind.Domain.Calendar.ValueObjects;

public sealed record EventColor
{
    public string Value { get; }

    private EventColor(string value) => Value = value;

    public static EventColor From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("Event color cannot be empty.");

        var normalized = value.Trim();

        if (!System.Text.RegularExpressions.Regex.IsMatch(normalized, "^#([0-9A-Fa-f]{6})$"))
            throw new InvalidOperationException("Event color must be a hex color like #AABBCC.");

        return new EventColor(normalized.ToUpperInvariant());
    }
}
