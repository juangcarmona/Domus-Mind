using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.SharedLists.ValueObjects;

public sealed class SharedListItemName : ValueObject
{
    public const int MaxLength = 200;

    public string Value { get; }

    private SharedListItemName(string value) => Value = value;

    public static SharedListItemName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Item name cannot be empty.", nameof(value));

        if (value.Trim().Length > MaxLength)
            throw new ArgumentException($"Item name cannot exceed {MaxLength} characters.", nameof(value));

        return new SharedListItemName(value.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
