using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.SharedLists.ValueObjects;

/// <summary>
/// Describes the intended usage category of a shared list.
/// Open-ended string to avoid coupling the domain to a fixed enum.
/// </summary>
public sealed class SharedListKind : ValueObject
{
    public const int MaxLength = 50;

    public string Value { get; }

    private SharedListKind(string value) => Value = value;

    public static SharedListKind Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Shared list kind cannot be empty.", nameof(value));

        if (value.Trim().Length > MaxLength)
            throw new ArgumentException($"Shared list kind cannot exceed {MaxLength} characters.", nameof(value));

        return new SharedListKind(value.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
