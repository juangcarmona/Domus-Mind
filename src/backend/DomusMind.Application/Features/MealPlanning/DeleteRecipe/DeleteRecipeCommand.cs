using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.MealPlanning;

namespace DomusMind.Application.Features.MealPlanning.DeleteRecipe;

public sealed record DeleteRecipeCommand(
    Guid RecipeId,
    Guid RequestedByUserId) : ICommand<DeleteRecipeResponse>;
