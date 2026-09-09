using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.MealPlanning;
using DomusMind.Domain.Family;
using DomusMind.Domain.MealPlanning.Entities;
using DomusMind.Domain.MealPlanning.Enums;
using DomusMind.Domain.MealPlanning.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.MealPlanning.CreateRecipe;

public sealed class CreateRecipeCommandHandler : ICommandHandler<CreateRecipeCommand, CreateRecipeResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public CreateRecipeCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<CreateRecipeResponse> Handle(
        CreateRecipeCommand command,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, command.FamilyId, cancellationToken);
        if (!canAccess)
            throw new UnauthorizedAccessException("Access to this family is denied.");

        var familyId = FamilyId.From(command.FamilyId);

        var nameExists = await _dbContext.Set<Recipe>()
            .AnyAsync(r => r.FamilyId == familyId && r.Name == command.Name, cancellationToken);
        if (nameExists)
            throw new InvalidOperationException($"A recipe named '{command.Name}' already exists in this family.");

        var allowedMealTypes = command.AllowedMealTypes?
            .Select(s => Enum.Parse<MealType>(s, ignoreCase: true))
            .ToList();

        var now = DateTime.UtcNow;
        var recipeId = RecipeId.From(command.RecipeId);

        var recipe = Recipe.Create(
            recipeId,
            familyId,
            command.Name,
            command.Description,
            command.PrepTimeMinutes,
            command.CookTimeMinutes,
            command.Servings,
            command.IsFavorite,
            allowedMealTypes,
            command.Tags,
            now);

        if (command.Ingredients is { Count: > 0 })
        {
            var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var ing in command.Ingredients)
            {
                if (!seenNames.Add(ing.Name))
                    throw new InvalidOperationException($"Duplicate ingredient name '{ing.Name}' in request.");

                recipe.AddIngredient(Ingredient.Create(
                    IngredientId.New(),
                    ing.Name,
                    recipeId,
                    ing.Quantity,
                    ing.Unit,
                    now));
            }
        }

        _dbContext.Set<Recipe>().Add(recipe);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _eventLogWriter.WriteAsync(recipe.DomainEvents, cancellationToken);
        recipe.ClearDomainEvents();

        return new CreateRecipeResponse(
            recipe.Id.Value,
            recipe.FamilyId.Value,
            recipe.Name,
            recipe.Ingredients.Count,
            recipe.TotalTimeMinutes,
            recipe.IsFavorite);
    }
}
