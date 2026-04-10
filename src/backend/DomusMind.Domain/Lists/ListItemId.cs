namespace DomusMind.Domain.Lists;

public readonly record struct ListItemId(Guid Value)
{
    public static ListItemId New() => new(Guid.NewGuid());
    public static ListItemId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
