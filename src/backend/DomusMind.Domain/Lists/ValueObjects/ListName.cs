using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Lists.ValueObjects;

public sealed class ListName : ValueObject
{
    public const int MaxLength = 200;

    public string Value { get; }

    private ListName(string value) => Value = value;

    public static ListName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Shared list name cannot be empty.", nameof(value));

        if (value.Trim().Length > MaxLength)
            throw new ArgumentException($"Shared list name cannot exceed {MaxLength} characters.", nameof(value));

        return new ListName(value.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
