namespace DomusMind.Contracts.MealPlanning;

public sealed record CreateMealPlanResponse(
    Guid Id,
    Guid FamilyId,
    DateOnly WeekStart,
    DateOnly WeekEnd,
    string Status,
    DateTime CreatedAtUtc,
    bool AlreadyExisted = false);
