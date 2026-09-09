using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.MealPlanning;
using DomusMind.Domain.MealPlanning.Entities;
using DomusMind.Domain.MealPlanning.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.MealPlanning.UpdateRecipeIngredient;

public sealed class UpdateRecipeIngredientCommandHandler : ICommandHandler<UpdateRecipeIngredientCommand, UpdateRecipeIngredientResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public UpdateRecipeIngredientCommandHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<UpdateRecipeIngredientResponse> Handle(UpdateRecipeIngredientCommand command, CancellationToken cancellationToken)
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

        recipe.UpdateIngredient(command.IngredientName, command.Quantity, command.Unit);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateRecipeIngredientResponse(
            command.RecipeId,
            command.IngredientName,
            command.Quantity,
            command.Unit);
    }
}
