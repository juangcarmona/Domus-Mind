namespace DomusMind.Domain.SharedLists;

public readonly record struct SharedListId(Guid Value)
{
    public static SharedListId New() => new(Guid.NewGuid());
    public static SharedListId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
