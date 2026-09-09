using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.MealPlanning;

namespace DomusMind.Application.Features.MealPlanning.GetRecipeDetail;

public sealed record GetRecipeDetailQuery(
    Guid RecipeId,
    Guid RequestedByUserId) : IQuery<GetRecipeDetailResponse?>;
