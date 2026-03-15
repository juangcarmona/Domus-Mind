namespace DomusMind.Domain.Tasks;

public readonly record struct RoutineId(Guid Value)
{
    public static RoutineId New() => new(Guid.NewGuid());
    public static RoutineId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
