namespace DomusMind.Contracts.MealPlanning;

public sealed record CreateRecipeResponse(
    Guid Id,
    Guid FamilyId,
    string Name,
    int IngredientCount,
    int? TotalTimeMinutes,
    bool IsFavorite);
