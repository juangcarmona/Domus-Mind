namespace DomusMind.Domain.Family;

public readonly record struct MemberId(Guid Value)
{
    public static MemberId New() => new(Guid.NewGuid());
    public static MemberId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
