namespace DomusMind.Contracts.MealPlanning;

public sealed record CopyMealPlanFromPreviousWeekRequest(
    Guid MealPlanId,
    Guid FamilyId,
    DateOnly WeekStart,
    Guid? SourceMealPlanId = null);
