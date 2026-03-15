using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Calendar.ValueObjects;

public sealed class EventTitle : ValueObject
{
    public string Value { get; }

    private EventTitle(string value) => Value = value;

    public static EventTitle Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Event title cannot be empty.", nameof(value));

        if (value.Trim().Length > 200)
            throw new ArgumentException("Event title cannot exceed 200 characters.", nameof(value));

        return new EventTitle(value.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
