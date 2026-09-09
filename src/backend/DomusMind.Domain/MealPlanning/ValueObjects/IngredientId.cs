using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.MealPlanning.ValueObjects;

public readonly record struct IngredientId(Guid Value)
{
    public static IngredientId New() => new(Guid.NewGuid());
    public static IngredientId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}