using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.MealPlanning.DeleteRecipe;
using DomusMind.Application.Features.MealPlanning.GetFamilyRecipes;
using DomusMind.Domain.Family;
using DomusMind.Domain.MealPlanning.Entities;
using DomusMind.Domain.MealPlanning.Enums;
using DomusMind.Domain.MealPlanning.ValueObjects;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.MealPlanning;

public sealed class DeleteRecipeCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static DeleteRecipeCommandHandler BuildHandler(
        DomusMindDbContext db,
        IFamilyAuthorizationService? auth = null)
        => new(db, new EventLogWriter(db), auth ?? new StubMealPlanningAuthService());

    private static Recipe CreateRecipe(FamilyId familyId, string name = "Pasta")
        => Recipe.Create(RecipeId.New(), familyId, name, null, null, null, null, false, null, null, DateTime.UtcNow);

    [Fact]
    public async Task Handle_DeletesRecipeWithNoActivePlanReference()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var recipe = CreateRecipe(familyId);
        db.Set<Recipe>().Add(recipe);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var result = await handler.Handle(
            new DeleteRecipeCommand(recipe.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Deleted.Should().BeTrue();
        db.Set<Recipe>().Any(r => r.Id == recipe.Id).Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ThrowsWhenRecipeNotFound()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var act = async () => await handler.Handle(
            new DeleteRecipeCommand(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_ThrowsWhenAccessDenied()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var recipe = CreateRecipe(familyId);
        db.Set<Recipe>().Add(recipe);
        await db.SaveChangesAsync();

        var auth = new StubMealPlanningAuthService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = async () => await handler.Handle(
            new DeleteRecipeCommand(recipe.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}

public sealed class GetFamilyRecipesMealTypeFilterTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static GetFamilyRecipesQueryHandler BuildHandler(DomusMindDbContext db)
        => new(db, new StubMealPlanningAuthService());

    private static Recipe CreateRecipe(
        FamilyId familyId,
        string name,
        IEnumerable<MealType>? allowedMealTypes = null)
        => Recipe.Create(RecipeId.New(), familyId, name, null, null, null, null, false, allowedMealTypes, null, DateTime.UtcNow);

    [Fact]
    public async Task Handle_NoFilter_ReturnsAllRecipes()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        db.Set<Recipe>().AddRange(
            CreateRecipe(familyId, "Pasta", [MealType.Dinner]),
            CreateRecipe(familyId, "Oatmeal", [MealType.Breakfast]),
            CreateRecipe(familyId, "Anything"));
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var result = await handler.Handle(
            new GetFamilyRecipesQuery(familyId.Value, Guid.NewGuid(), null),
            CancellationToken.None);

        result.Recipes.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithMealTypeFilter_ExcludesIncompatibleRecipes()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        db.Set<Recipe>().AddRange(
            CreateRecipe(familyId, "Pasta", [MealType.Dinner]),
            CreateRecipe(familyId, "Oatmeal", [MealType.Breakfast]),
            CreateRecipe(familyId, "Anything")); // no restriction
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var result = await handler.Handle(
            new GetFamilyRecipesQuery(familyId.Value, Guid.NewGuid(), MealType.Dinner),
            CancellationToken.None);

        result.Recipes.Select(r => r.Name).Should().BeEquivalentTo(["Pasta", "Anything"]);
    }

    [Fact]
    public async Task Handle_WithMealTypeFilter_RecipesWithNoRestrictionAlwaysIncluded()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        db.Set<Recipe>().Add(CreateRecipe(familyId, "Anything")); // empty allowedMealTypes
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var result = await handler.Handle(
            new GetFamilyRecipesQuery(familyId.Value, Guid.NewGuid(), MealType.Breakfast),
            CancellationToken.None);

        result.Recipes.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithMealTypeFilter_EmptyResultWhenNoMatch()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        db.Set<Recipe>().Add(CreateRecipe(familyId, "Pasta", [MealType.Dinner]));
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var result = await handler.Handle(
            new GetFamilyRecipesQuery(familyId.Value, Guid.NewGuid(), MealType.Breakfast),
            CancellationToken.None);

        result.Recipes.Should().BeEmpty();
    }
}

internal sealed class StubMealPlanningAuthService : IFamilyAuthorizationService
{
    public bool CanAccess { get; set; } = true;

    public Task<bool> CanAccessFamilyAsync(Guid userId, Guid familyId, CancellationToken cancellationToken = default)
        => Task.FromResult(CanAccess);
}
