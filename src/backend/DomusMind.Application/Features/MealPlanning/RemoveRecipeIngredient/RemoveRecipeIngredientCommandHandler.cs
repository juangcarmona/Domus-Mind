using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.MealPlanning;
using DomusMind.Domain.MealPlanning.Entities;
using DomusMind.Domain.MealPlanning.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.MealPlanning.RemoveRecipeIngredient;

public sealed class RemoveRecipeIngredientCommandHandler : ICommandHandler<RemoveRecipeIngredientCommand, RemoveRecipeIngredientResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public RemoveRecipeIngredientCommandHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<RemoveRecipeIngredientResponse> Handle(RemoveRecipeIngredientCommand command, CancellationToken cancellationToken)
    {
        var recipeId = RecipeId.From(command.RecipeId);

        var recipe = await _dbContext.Set<Recipe>()
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);

        if (recipe is null)
            throw new InvalidOperationException($"Recipe '{command.RecipeId}' not found.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, recipe.FamilyId.Value, cancellationToken);

        if (!canAccess)
            throw new UnauthorizedAccessException("Access to this family is denied.");

        recipe.RemoveIngredient(command.IngredientName);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RemoveRecipeIngredientResponse(
            command.RecipeId,
            command.IngredientName,
            recipe.Ingredients.Count);
    }
}
