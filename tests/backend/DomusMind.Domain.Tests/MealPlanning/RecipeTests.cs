using DomusMind.Domain.Family;
using DomusMind.Domain.MealPlanning.Entities;
using DomusMind.Domain.MealPlanning.Enums;
using DomusMind.Domain.MealPlanning.Events;
using DomusMind.Domain.MealPlanning.ValueObjects;
using FluentAssertions;

namespace DomusMind.Domain.Tests.MealPlanning;

public sealed class RecipeTests
{
    private static Recipe BuildRecipe(string name = "Pasta Bolognese")
    {
        return Recipe.Create(
            RecipeId.New(),
            FamilyId.New(),
            name,
            description: null,
            prepTimeMinutes: null,
            cookTimeMinutes: null,
            servings: null,
            isFavorite: false,
            allowedMealTypes: null,
            tags: null,
            createdAtUtc: DateTime.UtcNow);
    }

    private static Ingredient BuildIngredient(RecipeId recipeId, string name = "Flour")
        => Ingredient.Create(IngredientId.New(), name, recipeId, quantity: null, unit: null, DateTime.UtcNow);

    // ── Recipe.Update ──────────────────────────────────────────────────────────

    [Fact]
    public void Update_ChangesName()
    {
        var recipe = BuildRecipe("Old Name");
        recipe.Update("New Name", null, null, null, null, false, null, null);
        recipe.Name.Should().Be("New Name");
    }

    [Fact]
    public void Update_ChangesAllOptionalFields()
    {
        var recipe = BuildRecipe();
        recipe.Update(
            "New Name",
            "A description",
            prepTimeMinutes: 10,
            cookTimeMinutes: 20,
            servings: 4,
            isFavorite: true,
            allowedMealTypes: [MealType.Dinner],
            tags: ["Italian"]);

        recipe.Description.Should().Be("A description");
        recipe.PrepTimeMinutes.Should().Be(10);
        recipe.CookTimeMinutes.Should().Be(20);
        recipe.TotalTimeMinutes.Should().Be(30);
        recipe.Servings.Should().Be(4);
        recipe.IsFavorite.Should().BeTrue();
        recipe.AllowedMealTypes.Should().ContainSingle().Which.Should().Be(MealType.Dinner);
        recipe.Tags.Should().ContainSingle().Which.Should().Be("Italian");
    }

    [Fact]
    public void Update_ClearsAllowedMealTypesWhenPassedNull()
    {
        var recipe = BuildRecipe();
        recipe.Update("Name", null, null, null, null, false, allowedMealTypes: [MealType.Dinner], tags: null);
        recipe.Update("Name", null, null, null, null, false, allowedMealTypes: null, tags: null);
        recipe.AllowedMealTypes.Should().BeEmpty();
    }

    [Fact]
    public void Update_RefreshesUpdatedAtUtc()
    {
        var recipe = BuildRecipe();
        var before = DateTime.UtcNow;
        recipe.Update("Name", null, null, null, null, false, null, null);
        recipe.UpdatedAtUtc.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Update_ThrowsWhenNameIsNull()
    {
        var recipe = BuildRecipe();
        var act = () => recipe.Update(null!, null, null, null, null, false, null, null);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── Recipe.RemoveIngredient ────────────────────────────────────────────────

    [Fact]
    public void RemoveIngredient_RemovesExistingIngredient()
    {
        var recipe = BuildRecipe();
        recipe.AddIngredient(BuildIngredient(recipe.Id, "Flour"));
        recipe.RemoveIngredient("Flour");
        recipe.Ingredients.Should().BeEmpty();
    }

    [Fact]
    public void RemoveIngredient_IsCaseInsensitive()
    {
        var recipe = BuildRecipe();
        recipe.AddIngredient(BuildIngredient(recipe.Id, "Flour"));
        recipe.RemoveIngredient("flour");
        recipe.Ingredients.Should().BeEmpty();
    }

    [Fact]
    public void RemoveIngredient_ThrowsWhenNotFound()
    {
        var recipe = BuildRecipe();
        var act = () => recipe.RemoveIngredient("NonExistent");
        act.Should().Throw<InvalidOperationException>().WithMessage("*NonExistent*");
    }

    // ── Recipe.UpdateIngredient ────────────────────────────────────────────────

    [Fact]
    public void UpdateIngredient_ChangesQuantityAndUnit()
    {
        var recipe = BuildRecipe();
        recipe.AddIngredient(Ingredient.Create(IngredientId.New(), "Flour", recipe.Id, null, null, DateTime.UtcNow));

        recipe.UpdateIngredient("Flour", newQuantity: 200, newUnit: "g");

        var updated = recipe.Ingredients.Single(i => i.Name == "Flour");
        updated.Quantity.Should().Be(200);
        updated.Unit.Should().Be("g");
    }

    [Fact]
    public void UpdateIngredient_ThrowsWhenIngredientNotFound()
    {
        var recipe = BuildRecipe();
        var act = () => recipe.UpdateIngredient("NonExistent", 100, "g");
        act.Should().Throw<InvalidOperationException>().WithMessage("*NonExistent*");
    }

    // ── Recipe.Delete ──────────────────────────────────────────────────────────

    [Fact]
    public void Delete_RaisesRecipeDeletedEvent()
    {
        var recipe = BuildRecipe();
        recipe.Delete();
        recipe.DomainEvents.Should().ContainSingle(e => e is RecipeDeleted);
    }

    [Fact]
    public void Delete_RecipeDeletedEvent_ContainsCorrectIds()
    {
        var recipe = BuildRecipe();
        recipe.Delete();
        var evt = recipe.DomainEvents.OfType<RecipeDeleted>().Single();
        evt.RecipeId.Should().Be(recipe.Id.Value);
        evt.FamilyId.Should().Be(recipe.FamilyId.Value);
    }
}
