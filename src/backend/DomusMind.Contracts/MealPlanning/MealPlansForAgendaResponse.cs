namespace DomusMind.Contracts.MealPlanning;

public sealed record MealPlanAgendaSlot(
    string DayOfWeek,
    string MealType,
    string MealSourceType,
    string? Label,
    string? Notes,
    bool IsOptional);

public sealed record MealPlansForAgendaResponse(
    IReadOnlyList<MealPlanAgendaSlot> Slots);
