using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Family;
using DomusMind.Domain.MealPlanning.Enums;
using DomusMind.Domain.MealPlanning.Events;
using DomusMind.Domain.MealPlanning.ValueObjects;

namespace DomusMind.Domain.MealPlanning.Entities;

public sealed class Recipe : AggregateRoot<RecipeId>
{
    public FamilyId FamilyId { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public int? PrepTimeMinutes { get; private set; }

    public int? CookTimeMinutes { get; private set; }

    public int? TotalTimeMinutes => PrepTimeMinutes.HasValue && CookTimeMinutes.HasValue
        ? PrepTimeMinutes.Value + CookTimeMinutes.Value
        : null;

    public int? Servings { get; private set; }

    public bool IsFavorite { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    private readonly List<MealType> _allowedMealTypes = new();
    public IReadOnlyList<MealType> AllowedMealTypes => _allowedMealTypes.AsReadOnly();

    private readonly List<string> _tags = new();
    public IReadOnlyList<string> Tags => _tags.AsReadOnly();

    private readonly List<Ingredient> _ingredients = new();
    public IReadOnlyList<Ingredient> Ingredients => _ingredients.AsReadOnly();

    // Parameterless constructor for EF Core
    private Recipe() : base(default!)
    {
        Name = string.Empty;
        FamilyId = default;
    }

    private Recipe(
        RecipeId id,
        FamilyId familyId,
        string name,
        string? description,
        int? prepTimeMinutes,
        int? cookTimeMinutes,
        int? servings,
        bool isFavorite,
        IEnumerable<MealType>? allowedMealTypes,
        IEnumerable<string>? tags,
        DateTime createdAtUtc) : base(id)
    {
        FamilyId = familyId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        PrepTimeMinutes = prepTimeMinutes;
        CookTimeMinutes = cookTimeMinutes;
        Servings = servings;
        IsFavorite = isFavorite;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;

        if (allowedMealTypes is not null)
            _allowedMealTypes.AddRange(allowedMealTypes);

        if (tags is not null)
            _tags.AddRange(tags);
    }

    public static Recipe Create(
        RecipeId id,
        FamilyId familyId,
        string name,
        string? description,
        int? prepTimeMinutes,
        int? cookTimeMinutes,
        int? servings,
        bool isFavorite,
        IEnumerable<MealType>? allowedMealTypes,
        IEnumerable<string>? tags,
        DateTime createdAtUtc)
    {
        var recipe = new Recipe(id, familyId, name, description, prepTimeMinutes, cookTimeMinutes,
            servings, isFavorite, allowedMealTypes, tags, createdAtUtc);
        recipe.RaiseDomainEvent(new RecipeCreated(Guid.NewGuid(), id.Value, familyId.Value, name, createdAtUtc));
        return recipe;
    }

    public void Update(
        string name,
        string? description,
        int? prepTimeMinutes,
        int? cookTimeMinutes,
        int? servings,
        bool isFavorite,
        IEnumerable<MealType>? allowedMealTypes,
        IEnumerable<string>? tags)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        PrepTimeMinutes = prepTimeMinutes;
        CookTimeMinutes = cookTimeMinutes;
        Servings = servings;
        IsFavorite = isFavorite;

        _allowedMealTypes.Clear();
        if (allowedMealTypes is not null)
            _allowedMealTypes.AddRange(allowedMealTypes);

        _tags.Clear();
        if (tags is not null)
            _tags.AddRange(tags);

        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void AddIngredient(Ingredient ingredient)
    {
        if (_ingredients.Any(i => i.Name.Equals(ingredient.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Ingredient '{ingredient.Name}' already exists in this recipe.");

        _ingredients.Add(ingredient);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void RemoveIngredient(string name)
    {
        var ingredient = _ingredients
            .FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (ingredient is null)
            throw new InvalidOperationException($"Ingredient '{name}' not found in this recipe.");

        _ingredients.Remove(ingredient);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateIngredient(string name, decimal? newQuantity, string? newUnit)
    {
        var existing = _ingredients
            .FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (existing is null)
            throw new InvalidOperationException($"Ingredient '{name}' not found in this recipe.");

        _ingredients.Remove(existing);
        _ingredients.Add(Ingredient.Create(existing.Id, existing.Name, existing.RecipeId, newQuantity, newUnit, existing.CreatedAtUtc));
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Delete()
    {
        RaiseDomainEvent(new RecipeDeleted(Guid.NewGuid(), Id.Value, FamilyId.Value, DateTime.UtcNow));
    }
}