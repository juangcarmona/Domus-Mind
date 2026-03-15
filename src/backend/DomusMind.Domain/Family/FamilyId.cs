namespace DomusMind.Domain.Family;

public readonly record struct FamilyId(Guid Value)
{
    public static FamilyId New() => new(Guid.NewGuid());
    public static FamilyId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
