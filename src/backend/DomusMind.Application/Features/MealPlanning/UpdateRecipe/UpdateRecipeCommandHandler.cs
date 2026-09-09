using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.MealPlanning;
using DomusMind.Domain.MealPlanning.Entities;
using DomusMind.Domain.MealPlanning.Enums;
using DomusMind.Domain.MealPlanning.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.MealPlanning.UpdateRecipe;

public sealed class UpdateRecipeCommandHandler : ICommandHandler<UpdateRecipeCommand, UpdateRecipeResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public UpdateRecipeCommandHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<UpdateRecipeResponse> Handle(UpdateRecipeCommand command, CancellationToken cancellationToken)
    {
        var recipeId = RecipeId.From(command.RecipeId);

        var recipe = await _dbContext.Set<Recipe>()
            .FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);

        if (recipe is null)
            throw new InvalidOperationException($"Recipe '{command.RecipeId}' not found.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, recipe.FamilyId.Value, cancellationToken);

        if (!canAccess)
            throw new UnauthorizedAccessException("Access to this family is denied.");

        // Enforce name uniqueness within the family (excluding this recipe)
        var nameConflict = await _dbContext.Set<Recipe>()
            .AnyAsync(r => r.FamilyId == recipe.FamilyId
                        && r.Name == command.Name
                        && r.Id != recipeId, cancellationToken);

        if (nameConflict)
            throw new InvalidOperationException($"A recipe named '{command.Name}' already exists in this family.");

        var allowedMealTypes = command.AllowedMealTypes?
            .Select(s => Enum.Parse<MealType>(s, ignoreCase: true))
            .ToList();

        recipe.Update(
            command.Name,
            command.Description,
            command.PrepTimeMinutes,
            command.CookTimeMinutes,
            command.Servings,
            command.IsFavorite,
            allowedMealTypes,
            command.Tags);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateRecipeResponse(
            recipe.Id.Value,
            recipe.FamilyId.Value,
            recipe.Name,
            recipe.TotalTimeMinutes,
            recipe.IsFavorite);
    }
}
