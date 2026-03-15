using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Tasks.ValueObjects;

public sealed class TaskTitle : ValueObject
{
    public string Value { get; }

    private TaskTitle(string value) => Value = value;

    public static TaskTitle Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Task title cannot be empty.", nameof(value));

        if (value.Trim().Length > 200)
            throw new ArgumentException("Task title cannot exceed 200 characters.", nameof(value));

        return new TaskTitle(value.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
