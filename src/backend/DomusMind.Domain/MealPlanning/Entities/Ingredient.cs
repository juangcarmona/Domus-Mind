using DomusMind.Domain.Abstractions;
using DomusMind.Domain.MealPlanning.ValueObjects;

namespace DomusMind.Domain.MealPlanning.Entities;

public sealed class Ingredient : Entity<IngredientId>
{
    public string Name { get; private set; }
    public RecipeId RecipeId { get; private set; }
    public decimal? Quantity { get; private set; }
    public string? Unit { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Ingredient() : base(default!)
    {
        Name = null!;
        RecipeId = default;
    }

    private Ingredient(
        IngredientId id,
        string name,
        RecipeId recipeId,
        decimal? quantity,
        string? unit,
        DateTime createdAtUtc) : base(id)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        RecipeId = recipeId;
        Quantity = quantity;
        Unit = unit;
        CreatedAtUtc = createdAtUtc;
    }

    public static Ingredient Create(
        IngredientId id,
        string name,
        RecipeId recipeId,
        decimal? quantity,
        string? unit,
        DateTime createdAtUtc)
    {
        return new Ingredient(id, name, recipeId, quantity, unit, createdAtUtc);
    }
}
