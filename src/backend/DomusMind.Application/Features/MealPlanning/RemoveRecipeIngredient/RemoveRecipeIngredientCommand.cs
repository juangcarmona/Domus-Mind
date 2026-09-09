using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.MealPlanning;

namespace DomusMind.Application.Features.MealPlanning.RemoveRecipeIngredient;

public sealed record RemoveRecipeIngredientCommand(
    Guid RecipeId,
    string IngredientName,
    Guid RequestedByUserId) : ICommand<RemoveRecipeIngredientResponse>;
