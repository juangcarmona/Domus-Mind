namespace DomusMind.Domain.Tasks.ValueObjects;

public sealed record TaskColor
{
    public string Value { get; }

    private TaskColor(string value) => Value = value;

    public static TaskColor From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("Task color cannot be empty.");

        var normalized = value.Trim();

        if (!System.Text.RegularExpressions.Regex.IsMatch(normalized, "^#([0-9A-Fa-f]{6})$"))
            throw new InvalidOperationException("Task color must be a hex color like #AABBCC.");

        return new TaskColor(normalized.ToUpperInvariant());
    }
}
