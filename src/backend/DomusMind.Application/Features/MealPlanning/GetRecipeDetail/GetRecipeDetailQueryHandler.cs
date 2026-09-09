using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.MealPlanning;
using DomusMind.Domain.MealPlanning.Entities;
using DomusMind.Domain.MealPlanning.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.MealPlanning.GetRecipeDetail;

public sealed class GetRecipeDetailQueryHandler : IQueryHandler<GetRecipeDetailQuery, GetRecipeDetailResponse?>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public GetRecipeDetailQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<GetRecipeDetailResponse?> Handle(GetRecipeDetailQuery query, CancellationToken cancellationToken)
    {
        var recipeId = RecipeId.From(query.RecipeId);

        var recipe = await _dbContext.Set<Recipe>()
            .AsNoTracking()
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);

        if (recipe is null)
            return null;

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, recipe.FamilyId.Value, cancellationToken);

        if (!canAccess)
            throw new UnauthorizedAccessException("Access to this family is denied.");

        return new GetRecipeDetailResponse(
            recipe.Id.Value,
            recipe.FamilyId.Value,
            recipe.Name,
            recipe.Description,
            recipe.PrepTimeMinutes,
            recipe.CookTimeMinutes,
            recipe.TotalTimeMinutes,
            recipe.Servings,
            recipe.IsFavorite,
            recipe.Tags,
            recipe.AllowedMealTypes.Select(m => m.ToString()).ToList(),
            recipe.Ingredients.Select(i => new IngredientDetail(i.Name, i.Quantity, i.Unit)).ToList(),
            recipe.CreatedAtUtc,
            recipe.UpdatedAtUtc);
    }
}
