using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.Family.ValueObjects;

public sealed class MemberRole : ValueObject
{
    public static readonly MemberRole Adult = new("Adult");
    public static readonly MemberRole Child = new("Child");
    public static readonly MemberRole Caregiver = new("Caregiver");

    private static readonly HashSet<string> ValidRoles =
        new(StringComparer.OrdinalIgnoreCase) { "Adult", "Child", "Caregiver" };

    public string Value { get; }

    private MemberRole(string value) => Value = value;

    public static MemberRole Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Member role cannot be empty.", nameof(value));

        if (!ValidRoles.TryGetValue(value, out var canonical))
            throw new ArgumentException(
                $"'{value}' is not a valid member role. Valid roles: {string.Join(", ", ValidRoles)}.",
                nameof(value));

        return new MemberRole(canonical);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
