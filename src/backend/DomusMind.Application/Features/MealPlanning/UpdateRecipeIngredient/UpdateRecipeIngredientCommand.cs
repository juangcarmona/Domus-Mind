using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.MealPlanning;

namespace DomusMind.Application.Features.MealPlanning.UpdateRecipeIngredient;

public sealed record UpdateRecipeIngredientCommand(
    Guid RecipeId,
    string IngredientName,
    decimal? Quantity,
    string? Unit,
    Guid RequestedByUserId) : ICommand<UpdateRecipeIngredientResponse>;
