namespace DomusMind.Contracts.MealPlanning;

public sealed record UpdateMealSlotRequest(
    string MealSourceType,
    Guid? RecipeId = null,
    string? FreeText = null,
    string? Notes = null,
    bool? IsOptional = null,
    bool? IsLocked = null);
