using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Family;
using DomusMind.Domain.MealPlanning.Enums;
using DomusMind.Domain.MealPlanning.Events;
using DomusMind.Domain.MealPlanning.ValueObjects;

namespace DomusMind.Domain.MealPlanning.Entities;

public sealed class WeeklyTemplate : AggregateRoot<WeeklyTemplateId>
{
    public FamilyId FamilyId { get; private set; }

    public string Name { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    private readonly List<MealSlotTemplate> _mealSlotTemplates = new();
    public IReadOnlyList<MealSlotTemplate> MealSlotTemplates => _mealSlotTemplates.AsReadOnly();

    // Parameterless constructor for EF Core
#pragma warning disable CS8618
    private WeeklyTemplate() : base(default!)
    {
    }
#pragma warning restore CS8618

    private WeeklyTemplate(
        WeeklyTemplateId id,
        FamilyId familyId,
        string name,
        DateTime createdAtUtc) : base(id)
    {
        FamilyId = familyId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public static WeeklyTemplate Create(
        WeeklyTemplateId id,
        FamilyId familyId,
        string name,
        DateTime createdAtUtc)
    {
        var template = new WeeklyTemplate(id, familyId, name, createdAtUtc);
        template.RaiseDomainEvent(new WeeklyTemplateCreated(Guid.NewGuid(), id.Value, familyId.Value, name, createdAtUtc));
        return template;
    }

    public void AddSlotTemplate(MealSlotTemplate slotTemplate)
    {
        var duplicate = _mealSlotTemplates.Any(s =>
            s.DayOfWeek == slotTemplate.DayOfWeek && s.MealType == slotTemplate.MealType);

        if (duplicate)
            throw new InvalidOperationException(
                $"A slot template for {slotTemplate.DayOfWeek}/{slotTemplate.MealType} already exists in this template.");

        _mealSlotTemplates.Add(slotTemplate);
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
