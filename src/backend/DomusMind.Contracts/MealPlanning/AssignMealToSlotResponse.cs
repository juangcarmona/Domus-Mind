namespace DomusMind.Contracts.MealPlanning;

public sealed record UpdateMealSlotResponse(
    Guid MealPlanId,
    string DayOfWeek,
    string MealType,
    string MealSourceType,
    Guid? RecipeId,
    string? FreeText,
    string? Notes,
    bool IsOptional,
    bool IsLocked);
