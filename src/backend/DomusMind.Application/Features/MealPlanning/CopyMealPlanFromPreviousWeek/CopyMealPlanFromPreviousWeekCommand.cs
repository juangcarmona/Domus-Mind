using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.MealPlanning;

namespace DomusMind.Application.Features.MealPlanning.CopyMealPlanFromPreviousWeek;

public sealed record CopyMealPlanFromPreviousWeekCommand(
    Guid MealPlanId,
    Guid FamilyId,
    DateOnly WeekStart,
    Guid? SourceMealPlanId,
    Guid RequestedByUserId) : ICommand<CopyMealPlanFromPreviousWeekResponse>;
