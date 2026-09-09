using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.MealPlanning;

namespace DomusMind.Application.Features.MealPlanning.CreateRecipe;

public sealed record CreateRecipeCommand(
    Guid RecipeId,
    Guid FamilyId,
    string Name,
    string? Description,
    int? PrepTimeMinutes,
    int? CookTimeMinutes,
    int? Servings,
    bool IsFavorite,
    IReadOnlyList<string>? AllowedMealTypes,
    IReadOnlyList<string>? Tags,
    IReadOnlyList<IngredientRequest>? Ingredients,
    Guid RequestedByUserId) : ICommand<CreateRecipeResponse>;

