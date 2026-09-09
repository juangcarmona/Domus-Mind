namespace DomusMind.Contracts.MealPlanning;

public sealed record CopyMealPlanFromPreviousWeekResponse(
    Guid? MealPlanId,
    Guid FamilyId,
    DateOnly WeekStart,
    DateOnly WeekEnd,
    Guid? SourceMealPlanId,
    string? Status,
    int SlotCount,
    bool Success = true,
    string? ErrorCode = null,
    bool AlreadyExisted = false);
