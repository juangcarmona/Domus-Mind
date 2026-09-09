namespace DomusMind.Contracts.MealPlanning;

public sealed record AddRecipeIngredientRequest(
    string Name,
    decimal? Quantity,
    string? Unit);

public sealed record AddRecipeIngredientResponse(
    Guid RecipeId,
    string IngredientName,
    int IngredientCount);

public sealed record UpdateRecipeIngredientRequest(
    decimal? Quantity,
    string? Unit);

public sealed record UpdateRecipeIngredientResponse(
    Guid RecipeId,
    string IngredientName,
    decimal? Quantity,
    string? Unit);

public sealed record RemoveRecipeIngredientResponse(
    Guid RecipeId,
    string IngredientName,
    int IngredientCount);
