using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.MealPlanning;
using DomusMind.Domain.Family;
using DomusMind.Domain.MealPlanning.Entities;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.MealPlanning.GetFamilyRecipes;

public sealed class GetFamilyRecipesQueryHandler : IQueryHandler<GetFamilyRecipesQuery, GetFamilyRecipesResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public GetFamilyRecipesQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<GetFamilyRecipesResponse> Handle(GetFamilyRecipesQuery query, CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, query.FamilyId, cancellationToken);

        if (!canAccess)
            throw new InvalidOperationException("Access to this family is denied.");

        var familyId = FamilyId.From(query.FamilyId);

        var recipesQuery = _dbContext.Set<Recipe>()
            .AsNoTracking()
            .Where(r => r.FamilyId == familyId);

        // When a meal type filter is provided, return recipes that are compatible:
        // either no restriction (AllowedMealTypes is empty) or the list contains the requested meal type.
        if (query.MealType.HasValue)
        {
            var mealType = query.MealType.Value;
            recipesQuery = recipesQuery.Where(r =>
                !r.AllowedMealTypes.Any() || r.AllowedMealTypes.Contains(mealType));
        }

        var recipes = await recipesQuery
            .Include(r => r.Ingredients)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        var items = recipes.Select(r => new FamilyRecipeItem(
                r.Id.Value,
                r.FamilyId.Value,
                r.Name,
                r.Description,
                r.PrepTimeMinutes,
                r.CookTimeMinutes,
                r.PrepTimeMinutes.HasValue && r.CookTimeMinutes.HasValue
                    ? r.PrepTimeMinutes.Value + r.CookTimeMinutes.Value
                    : (int?)null,
                r.Servings,
                r.IsFavorite,
                r.Tags,
                r.AllowedMealTypes.Select(m => m.ToString()).ToList(),
                r.Ingredients.Count,
                r.CreatedAtUtc))
            .ToList();

        return new GetFamilyRecipesResponse(items);
    }
}
