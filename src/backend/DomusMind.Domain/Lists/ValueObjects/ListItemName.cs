using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Lists.ValueObjects;

public sealed class ListItemName : ValueObject
{
    public const int MaxLength = 200;

    public string Value { get; }

    private ListItemName(string value) => Value = value;

    public static ListItemName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Item name cannot be empty.", nameof(value));

        if (value.Trim().Length > MaxLength)
            throw new ArgumentException($"Item name cannot exceed {MaxLength} characters.", nameof(value));

        return new ListItemName(value.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
