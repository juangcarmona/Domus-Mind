using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Family.ValueObjects;

public sealed class FamilyName : ValueObject
{
    public string Value { get; }

    private FamilyName(string value) => Value = value;

    public static FamilyName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Family name cannot be empty.", nameof(value));

        if (value.Trim().Length > 100)
            throw new ArgumentException("Family name cannot exceed 100 characters.", nameof(value));

        return new FamilyName(value.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
