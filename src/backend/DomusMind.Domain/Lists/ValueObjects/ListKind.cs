using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Lists.ValueObjects;

/// <summary>
/// Describes the intended usage category of a shared list.
/// Open-ended string to avoid coupling the domain to a fixed enum.
/// </summary>
public sealed class ListKind : ValueObject
{
    public const int MaxLength = 50;

    public string Value { get; }

    private ListKind(string value) => Value = value;

    public static ListKind Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Shared list kind cannot be empty.", nameof(value));

        if (value.Trim().Length > MaxLength)
            throw new ArgumentException($"Shared list kind cannot exceed {MaxLength} characters.", nameof(value));

        return new ListKind(value.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
