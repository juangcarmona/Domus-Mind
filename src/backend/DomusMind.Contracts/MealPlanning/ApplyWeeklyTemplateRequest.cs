namespace DomusMind.Contracts.MealPlanning;

public sealed record ApplyWeeklyTemplateRequest(
    Guid MealPlanId,
    Guid FamilyId,
    DateOnly WeekStart,
    Guid TemplateId);
