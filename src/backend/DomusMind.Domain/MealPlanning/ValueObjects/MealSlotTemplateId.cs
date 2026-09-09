using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.MealPlanning.ValueObjects;

public readonly record struct MealSlotTemplateId(Guid Value)
{
    public static MealSlotTemplateId New() => new(Guid.NewGuid());
    public static MealSlotTemplateId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}