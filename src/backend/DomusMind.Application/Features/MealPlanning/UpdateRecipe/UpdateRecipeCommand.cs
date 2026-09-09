using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.MealPlanning;

namespace DomusMind.Application.Features.MealPlanning.UpdateRecipe;

public sealed record UpdateRecipeCommand(
    Guid RecipeId,
    string Name,
    string? Description,
    int? PrepTimeMinutes,
    int? CookTimeMinutes,
    int? Servings,
    bool IsFavorite,
    IReadOnlyList<string>? AllowedMealTypes,
    IReadOnlyList<string>? Tags,
    Guid RequestedByUserId) : ICommand<UpdateRecipeResponse>;
