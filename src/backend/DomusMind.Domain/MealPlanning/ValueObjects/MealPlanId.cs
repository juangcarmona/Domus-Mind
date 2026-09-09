namespace DomusMind.Domain.MealPlanning.ValueObjects;

public readonly record struct MealPlanId(Guid Value)
{
    public static MealPlanId New() => new(Guid.NewGuid());
    public static MealPlanId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}