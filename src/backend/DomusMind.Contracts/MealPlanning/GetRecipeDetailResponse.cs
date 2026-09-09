namespace DomusMind.Contracts.MealPlanning;

public sealed record IngredientDetail(
    string Name,
    decimal? Quantity,
    string? Unit);

public sealed record GetRecipeDetailResponse(
    Guid Id,
    Guid FamilyId,
    string Name,
    string? Description,
    int? PrepTimeMinutes,
    int? CookTimeMinutes,
    int? TotalTimeMinutes,
    int? Servings,
    bool IsFavorite,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> AllowedMealTypes,
    IReadOnlyList<IngredientDetail> Ingredients,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
