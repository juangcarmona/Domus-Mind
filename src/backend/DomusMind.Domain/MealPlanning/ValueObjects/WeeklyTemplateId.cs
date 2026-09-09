using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.MealPlanning.ValueObjects;

public readonly record struct WeeklyTemplateId(Guid Value)
{
    public static WeeklyTemplateId New() => new(Guid.NewGuid());
    public static WeeklyTemplateId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}