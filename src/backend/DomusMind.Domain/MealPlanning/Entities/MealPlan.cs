using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Family;
using DomusMind.Domain.MealPlanning.Enums;
using DomusMind.Domain.MealPlanning.Events;
using DomusMind.Domain.MealPlanning.ValueObjects;

namespace DomusMind.Domain.MealPlanning.Entities;

public sealed class MealPlan : AggregateRoot<MealPlanId>
{
    public FamilyId FamilyId { get; private set; }

    public DateOnly WeekStart { get; private set; }

    public DateOnly WeekEnd => WeekStart.AddDays(6);

    public MealPlanStatus Status { get; private set; }

    public WeeklyTemplateId? AppliedTemplateId { get; private set; }

    public Guid? ShoppingListId { get; private set; }

    public int ShoppingListVersion { get; private set; }

    public DateTime? LastDerivedAt { get; private set; }

    public bool AffectsWholeHousehold { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    private readonly List<MealSlot> _mealSlots = new();
    public IReadOnlyList<MealSlot> MealSlots => _mealSlots.AsReadOnly();

    // Parameterless constructor for EF Core
    private MealPlan() : base(default!)
    {
        FamilyId = default;
    }

    private MealPlan(
        MealPlanId id,
        FamilyId familyId,
        DateOnly weekStart,
        bool affectsWholeHousehold,
        DateTime createdAtUtc) : base(id)
    {
        FamilyId = familyId;
        WeekStart = weekStart;
        Status = MealPlanStatus.Draft;
        AffectsWholeHousehold = affectsWholeHousehold;
        ShoppingListVersion = 0;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;

        MaterializeSlots(id, createdAtUtc);
    }

    private void MaterializeSlots(MealPlanId planId, DateTime createdAtUtc)
    {
        var allDays = Enum.GetValues<Enums.DayOfWeek>();
        var allMealTypes = new[]
        {
            MealType.Breakfast,
            MealType.MidMorningSnack,
            MealType.Lunch,
            MealType.AfternoonSnack,
            MealType.Dinner
        };

        foreach (var day in allDays)
        {
            foreach (var mealType in allMealTypes)
            {
                _mealSlots.Add(MealSlot.CreateEmpty(
                    MealSlotId.New(), day, mealType, planId, createdAtUtc));
            }
        }
    }

    public static MealPlan Create(
        MealPlanId id,
        FamilyId familyId,
        DateOnly weekStart,
        bool affectsWholeHousehold,
        DateTime createdAtUtc)
    {
        var mealPlan = new MealPlan(id, familyId, weekStart, affectsWholeHousehold, createdAtUtc);
        mealPlan.RaiseDomainEvent(new MealPlanCreated(Guid.NewGuid(), id.Value, familyId.Value, weekStart, createdAtUtc));
        return mealPlan;
    }

    public static MealPlan CreateFromTemplate(
        MealPlanId id,
        FamilyId familyId,
        DateOnly weekStart,
        WeeklyTemplate template,
        DateTime createdAtUtc)
    {
        var mealPlan = new MealPlan(id, familyId, weekStart, true, createdAtUtc);
        mealPlan.AppliedTemplateId = template.Id;

        foreach (var templateSlot in template.MealSlotTemplates)
        {
            var slot = mealPlan._mealSlots.FirstOrDefault(
                s => s.DayOfWeek == templateSlot.DayOfWeek && s.MealType == templateSlot.MealType);

            slot?.CopyFrom(
                templateSlot.MealSourceType,
                templateSlot.RecipeId,
                templateSlot.FreeText,
                templateSlot.Notes,
                templateSlot.IsOptional,
                templateSlot.IsLocked,
                createdAtUtc);
        }

        mealPlan.RaiseDomainEvent(new MealPlanCreated(Guid.NewGuid(), id.Value, familyId.Value, weekStart, createdAtUtc));
        mealPlan.RaiseDomainEvent(new WeeklyTemplateApplied(Guid.NewGuid(), template.Id.Value, id.Value, createdAtUtc));
        return mealPlan;
    }

    public static MealPlan CopyFromPlan(
        MealPlanId id,
        FamilyId familyId,
        DateOnly weekStart,
        MealPlan sourcePlan,
        DateTime createdAtUtc)
    {
        var mealPlan = new MealPlan(id, familyId, weekStart, sourcePlan.AffectsWholeHousehold, createdAtUtc);

        foreach (var sourceSlot in sourcePlan.MealSlots)
        {
            var slot = mealPlan._mealSlots.FirstOrDefault(
                s => s.DayOfWeek == sourceSlot.DayOfWeek && s.MealType == sourceSlot.MealType);

            slot?.CopyFrom(
                sourceSlot.MealSourceType,
                sourceSlot.RecipeId,
                sourceSlot.FreeText,
                sourceSlot.Notes,
                sourceSlot.IsOptional,
                sourceSlot.IsLocked,
                createdAtUtc);
        }

        mealPlan.RaiseDomainEvent(new MealPlanCreated(Guid.NewGuid(), id.Value, familyId.Value, weekStart, createdAtUtc));
        mealPlan.RaiseDomainEvent(new MealPlanCopiedFromPreviousWeek(Guid.NewGuid(), sourcePlan.Id.Value, id.Value, createdAtUtc));
        return mealPlan;
    }

    public void UpdateSlot(
        Enums.DayOfWeek dayOfWeek,
        MealType mealType,
        MealSourceType mealSourceType,
        RecipeId? recipeId,
        string? freeText,
        string? notes,
        bool? isOptional,
        bool? isLocked,
        DateTime updatedAtUtc)
    {
        if (Status == MealPlanStatus.Completed)
            throw new InvalidOperationException("Cannot modify a completed meal plan.");

        var slot = _mealSlots.FirstOrDefault(s => s.DayOfWeek == dayOfWeek && s.MealType == mealType)
            ?? throw new InvalidOperationException($"Slot {dayOfWeek}/{mealType} not found.");

        // Must pass isLocked=false to unlock before mutating
        if (slot.IsLocked && isLocked != false)
            throw new InvalidOperationException($"Slot {dayOfWeek}/{mealType} is locked. Pass isLocked=false to unlock.");

        if (mealSourceType == MealSourceType.Recipe && recipeId is null)
            throw new InvalidOperationException("recipeId is required when mealSourceType is Recipe.");

        if (mealSourceType == MealSourceType.FreeText && string.IsNullOrWhiteSpace(freeText))
            throw new InvalidOperationException("freeText is required when mealSourceType is FreeText.");

        slot.Assign(mealSourceType, recipeId, freeText, notes, isOptional, isLocked, updatedAtUtc);
        UpdatedAtUtc = updatedAtUtc;

        if (mealSourceType == MealSourceType.Unplanned)
            RaiseDomainEvent(new MealSlotCleared(Guid.NewGuid(), Id.Value, dayOfWeek, mealType, updatedAtUtc));
        else
            RaiseDomainEvent(new MealSlotAssigned(Guid.NewGuid(), Id.Value, dayOfWeek, mealType, mealSourceType, recipeId?.Value, freeText, updatedAtUtc));
    }

    public void RecordShoppingListCreated(Guid shoppingListId, DateTime now)
    {
        ShoppingListId = shoppingListId;
        ShoppingListVersion++;
        LastDerivedAt = now;
        UpdatedAtUtc = now;
    }

    public void RequestShoppingList(DateTime now)
    {
        var hasRecipeSlot = _mealSlots.Any(s => s.MealSourceType == MealSourceType.Recipe && s.RecipeId.HasValue);
        if (!hasRecipeSlot)
            throw new InvalidOperationException("Cannot derive a shopping list: no recipe slots are assigned.");

        ShoppingListVersion++;
        LastDerivedAt = now;
        UpdatedAtUtc = now;

        RaiseDomainEvent(new ShoppingListRequested(Guid.NewGuid(), Id.Value, FamilyId.Value, now));
    }
}
