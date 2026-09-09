using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.MealPlanning.ValueObjects;

public readonly record struct MealSlotId(Guid Value)
{
    public static MealSlotId New() => new(Guid.NewGuid());
    public static MealSlotId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}