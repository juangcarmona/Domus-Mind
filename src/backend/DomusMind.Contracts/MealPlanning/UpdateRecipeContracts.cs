namespace DomusMind.Contracts.MealPlanning;

public sealed record UpdateRecipeRequest(
    string Name,
    string? Description,
    int? PrepTimeMinutes,
    int? CookTimeMinutes,
    int? Servings,
    bool IsFavorite,
    IReadOnlyList<string>? AllowedMealTypes,
    IReadOnlyList<string>? Tags);

public sealed record UpdateRecipeResponse(
    Guid Id,
    Guid FamilyId,
    string Name,
    int? TotalTimeMinutes,
    bool IsFavorite);
