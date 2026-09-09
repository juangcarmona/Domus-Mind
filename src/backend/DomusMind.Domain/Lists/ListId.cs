namespace DomusMind.Domain.Lists;

public readonly record struct ListId(Guid Value)
{
    public static ListId New() => new(Guid.NewGuid());
    public static ListId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
