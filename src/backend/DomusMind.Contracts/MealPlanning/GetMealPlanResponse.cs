namespace DomusMind.Contracts.MealPlanning;

public sealed record MealSlotRecipeDetail(
    Guid RecipeId,
    string Name,
    int? Servings,
    int? PrepTimeMinutes,
    int? TotalTimeMinutes,
    IReadOnlyList<string> AllowedMealTypes);

public sealed record MealSlotDetail(
    string DayOfWeek,
    string MealType,
    string MealSourceType,
    MealSlotRecipeDetail? Recipe,
    string? FreeText,
    string? Notes,
    bool IsOptional,
    bool IsLocked);

public sealed record MealPlanDetail(
    Guid PlanId,
    Guid FamilyId,
    DateOnly WeekStart,
    DateOnly WeekEnd,
    string Status,
    Guid? AppliedTemplateId,
    Guid? ShoppingListId,
    int ShoppingListVersion,
    DateTime? LastDerivedAt,
    IReadOnlyList<MealSlotDetail> Slots);

/// <summary>
/// Returns null MealPlan when no plan exists for the requested week.
/// </summary>
public sealed record GetMealPlanResponse(MealPlanDetail? MealPlan);
