namespace DomusMind.Contracts.MealPlanning;

public sealed record TemplateSlotRequest(
    string DayOfWeek,
    string MealType,
    string MealSourceType,
    Guid? RecipeId = null,
    string? FreeText = null,
    string? Notes = null,
    bool IsOptional = false,
    bool IsLocked = false);

public sealed record CreateWeeklyTemplateRequest(
    Guid TemplateId,
    Guid FamilyId,
    string Name,
    IReadOnlyList<TemplateSlotRequest>? Slots = null);
