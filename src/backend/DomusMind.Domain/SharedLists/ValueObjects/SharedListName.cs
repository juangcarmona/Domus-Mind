using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.SharedLists.ValueObjects;

public sealed class SharedListName : ValueObject
{
    public const int MaxLength = 200;

    public string Value { get; }

    private SharedListName(string value) => Value = value;

    public static SharedListName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Shared list name cannot be empty.", nameof(value));

        if (value.Trim().Length > MaxLength)
            throw new ArgumentException($"Shared list name cannot exceed {MaxLength} characters.", nameof(value));

        return new SharedListName(value.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
