using DomusMind.Domain.Abstractions;
using DomusMind.Domain.MealPlanning.Enums;
using DomusMind.Domain.MealPlanning.ValueObjects;

namespace DomusMind.Domain.MealPlanning.Entities;

public sealed class MealSlotTemplate : Entity<MealSlotTemplateId>
{
    public Enums.DayOfWeek DayOfWeek { get; private set; }

    public MealType MealType { get; private set; }

    public WeeklyTemplateId WeeklyTemplateId { get; private set; }

    public MealSourceType MealSourceType { get; private set; }

    public RecipeId? RecipeId { get; private set; }

    public string? FreeText { get; private set; }

    public string? Notes { get; private set; }

    public bool IsOptional { get; private set; }

    public bool IsLocked { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    private MealSlotTemplate() : base(default!)
    {
        WeeklyTemplateId = default!;
    }

    private MealSlotTemplate(
        MealSlotTemplateId id,
        Enums.DayOfWeek dayOfWeek,
        MealType mealType,
        WeeklyTemplateId weeklyTemplateId,
        MealSourceType mealSourceType,
        RecipeId? recipeId,
        string? freeText,
        string? notes,
        bool isOptional,
        bool isLocked,
        DateTime createdAtUtc) : base(id)
    {
        DayOfWeek = dayOfWeek;
        MealType = mealType;
        WeeklyTemplateId = weeklyTemplateId;
        MealSourceType = mealSourceType;
        RecipeId = mealSourceType == MealSourceType.Recipe ? recipeId : null;
        FreeText = mealSourceType == MealSourceType.FreeText ? freeText : null;
        Notes = notes;
        IsOptional = isOptional;
        IsLocked = isLocked;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public static MealSlotTemplate Create(
        MealSlotTemplateId id,
        Enums.DayOfWeek dayOfWeek,
        MealType mealType,
        WeeklyTemplateId weeklyTemplateId,
        MealSourceType mealSourceType,
        RecipeId? recipeId,
        string? freeText,
        string? notes,
        bool isOptional,
        bool isLocked,
        DateTime createdAtUtc)
    {
        return new MealSlotTemplate(id, dayOfWeek, mealType, weeklyTemplateId, mealSourceType,
            recipeId, freeText, notes, isOptional, isLocked, createdAtUtc);
    }
}
