namespace DomusMind.Contracts.MealPlanning;

public sealed record CreateMealPlanRequest(
    Guid MealPlanId,
    Guid FamilyId,
    DateOnly WeekStart,
    Guid? ResponsibilityDomainId = null);
