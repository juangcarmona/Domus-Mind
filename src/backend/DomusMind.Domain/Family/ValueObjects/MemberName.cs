using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Family.ValueObjects;

public sealed class MemberName : ValueObject
{
    public string Value { get; }

    private MemberName(string value) => Value = value;

    public static MemberName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Member name cannot be empty.", nameof(value));

        if (value.Trim().Length > 100)
            throw new ArgumentException("Member name cannot exceed 100 characters.", nameof(value));

        return new MemberName(value.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
