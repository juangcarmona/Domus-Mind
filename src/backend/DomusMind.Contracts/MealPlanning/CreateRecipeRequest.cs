namespace DomusMind.Contracts.MealPlanning;

public sealed record IngredientRequest(
    string Name,
    decimal? Quantity,
    string? Unit);

public sealed record CreateRecipeRequest(
    Guid RecipeId,
    Guid FamilyId,
    string Name,
    string? Description,
    int? PrepTimeMinutes,
    int? CookTimeMinutes,
    int? Servings,
    bool IsFavorite = false,
    IReadOnlyList<string>? AllowedMealTypes = null,
    IReadOnlyList<string>? Tags = null,
    IReadOnlyList<IngredientRequest>? Ingredients = null);
