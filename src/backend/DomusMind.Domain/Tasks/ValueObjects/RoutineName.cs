using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Tasks.ValueObjects;

public sealed class RoutineName : ValueObject
{
    public string Value { get; }

    private RoutineName(string value) => Value = value;

    public static RoutineName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Routine name cannot be empty.", nameof(value));

        if (value.Trim().Length > 200)
            throw new ArgumentException("Routine name cannot exceed 200 characters.", nameof(value));

        return new RoutineName(value.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
