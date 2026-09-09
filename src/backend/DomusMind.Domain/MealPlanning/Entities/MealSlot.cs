using DomusMind.Domain.Abstractions;
using DomusMind.Domain.MealPlanning.Enums;
using DomusMind.Domain.MealPlanning.ValueObjects;

namespace DomusMind.Domain.MealPlanning.Entities;

public sealed class MealSlot : Entity<MealSlotId>
{
    public Enums.DayOfWeek DayOfWeek { get; private set; }

    public MealType MealType { get; private set; }

    public MealPlanId MealPlanId { get; private set; }

    public MealSourceType MealSourceType { get; private set; }

    public RecipeId? RecipeId { get; private set; }

    public string? FreeText { get; private set; }

    public string? Notes { get; private set; }

    public bool IsOptional { get; private set; }

    public bool IsLocked { get; private set; }

    public bool AffectsWholeHousehold { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    // Parameterless constructor for EF Core
    private MealSlot() : base(default!)
    {
        MealPlanId = default!;
    }

    private MealSlot(
        MealSlotId id,
        Enums.DayOfWeek dayOfWeek,
        MealType mealType,
        MealPlanId mealPlanId,
        DateTime createdAtUtc) : base(id)
    {
        DayOfWeek = dayOfWeek;
        MealType = mealType;
        MealPlanId = mealPlanId;
        MealSourceType = MealSourceType.Unplanned;
        IsOptional = false;
        IsLocked = false;
        AffectsWholeHousehold = true;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    internal static MealSlot CreateEmpty(
        MealSlotId id,
        Enums.DayOfWeek dayOfWeek,
        MealType mealType,
        MealPlanId mealPlanId,
        DateTime createdAtUtc)
    {
        return new MealSlot(id, dayOfWeek, mealType, mealPlanId, createdAtUtc);
    }

    /// <summary>
    /// Updates slot content. Caller must validate source-type consistency before calling.
    /// If the slot is locked, pass isLocked=false in the same call to unlock before mutating.
    /// </summary>
    internal void Assign(
        MealSourceType mealSourceType,
        RecipeId? recipeId,
        string? freeText,
        string? notes,
        bool? isOptional,
        bool? isLocked,
        DateTime updatedAtUtc)
    {
        MealSourceType = mealSourceType;
        RecipeId = mealSourceType == MealSourceType.Recipe ? recipeId : null;
        FreeText = mealSourceType == MealSourceType.FreeText ? freeText : null;
        Notes = notes;
        if (isOptional.HasValue) IsOptional = isOptional.Value;
        if (isLocked.HasValue) IsLocked = isLocked.Value;
        UpdatedAtUtc = updatedAtUtc;
    }

    /// <summary>
    /// Copies state from a template slot or another meal slot for plan creation flows.
    /// </summary>
    internal void CopyFrom(
        MealSourceType mealSourceType,
        RecipeId? recipeId,
        string? freeText,
        string? notes,
        bool isOptional,
        bool isLocked,
        DateTime updatedAtUtc)
    {
        MealSourceType = mealSourceType;
        RecipeId = mealSourceType == MealSourceType.Recipe ? recipeId : null;
        FreeText = mealSourceType == MealSourceType.FreeText ? freeText : null;
        Notes = notes;
        IsOptional = isOptional;
        IsLocked = isLocked;
        UpdatedAtUtc = updatedAtUtc;
    }
}
