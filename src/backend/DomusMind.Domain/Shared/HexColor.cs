namespace DomusMind.Domain.Shared;

public sealed record HexColor
{
    public string Value { get; }

    private HexColor(string value) => Value = value;

    public static HexColor From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("Color cannot be empty.");

        var normalized = value.Trim();

        if (!System.Text.RegularExpressions.Regex.IsMatch(normalized, "^#([0-9A-Fa-f]{6})$"))
            throw new InvalidOperationException("Color must be a hex color like #AABBCC.");

        return new HexColor(normalized.ToUpperInvariant());
    }
}
