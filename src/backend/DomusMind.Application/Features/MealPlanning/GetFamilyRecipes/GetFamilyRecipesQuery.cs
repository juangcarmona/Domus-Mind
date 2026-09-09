using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.MealPlanning;
using DomusMind.Domain.MealPlanning.Enums;

namespace DomusMind.Application.Features.MealPlanning.GetFamilyRecipes;

public sealed record GetFamilyRecipesQuery(
    Guid FamilyId,
    Guid RequestedByUserId,
    MealType? MealType = null) : IQuery<GetFamilyRecipesResponse>;
