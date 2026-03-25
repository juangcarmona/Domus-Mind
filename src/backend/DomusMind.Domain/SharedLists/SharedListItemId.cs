namespace DomusMind.Domain.SharedLists;

public readonly record struct SharedListItemId(Guid Value)
{
    public static SharedListItemId New() => new(Guid.NewGuid());
    public static SharedListItemId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
