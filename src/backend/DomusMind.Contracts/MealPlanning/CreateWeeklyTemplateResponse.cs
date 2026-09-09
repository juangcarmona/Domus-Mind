namespace DomusMind.Contracts.MealPlanning;

public sealed record CreateWeeklyTemplateResponse(
    Guid TemplateId,
    Guid FamilyId,
    string Name,
    int SlotCount);
