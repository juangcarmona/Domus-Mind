using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.MealPlanning;
using DomusMind.Domain.MealPlanning.Entities;
using DomusMind.Domain.MealPlanning.Enums;
using DomusMind.Domain.MealPlanning.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.MealPlanning.DeleteRecipe;

public sealed class DeleteRecipeCommandHandler : ICommandHandler<DeleteRecipeCommand, DeleteRecipeResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public DeleteRecipeCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<DeleteRecipeResponse> Handle(DeleteRecipeCommand command, CancellationToken cancellationToken)
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

        // Guard: recipe may not be deleted if referenced by an Active meal plan slot
        var referencedByActiveSlot = await (
            from slot in _dbContext.Set<MealSlot>()
            join plan in _dbContext.Set<MealPlan>() on slot.MealPlanId equals plan.Id
            where slot.RecipeId == recipeId
               && slot.MealSourceType == MealSourceType.Recipe
               && plan.Status == MealPlanStatus.Active
            select slot.Id
        ).AnyAsync(cancellationToken);

        if (referencedByActiveSlot)
            throw new InvalidOperationException(
                "This recipe cannot be deleted because it is currently assigned to an active meal plan.");

        recipe.Delete();

        await _eventLogWriter.WriteAsync(recipe.DomainEvents, cancellationToken);

        _dbContext.Set<Recipe>().Remove(recipe);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DeleteRecipeResponse(command.RecipeId, true);
    }
}
