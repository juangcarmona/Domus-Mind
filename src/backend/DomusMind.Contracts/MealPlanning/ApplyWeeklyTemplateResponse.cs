namespace DomusMind.Contracts.MealPlanning;

public sealed record ApplyWeeklyTemplateResponse(
    Guid MealPlanId,
    Guid FamilyId,
    DateOnly WeekStart,
    DateOnly WeekEnd,
    Guid AppliedTemplateId,
    int SlotCount,
    string Status,
    bool AlreadyExisted = false);
