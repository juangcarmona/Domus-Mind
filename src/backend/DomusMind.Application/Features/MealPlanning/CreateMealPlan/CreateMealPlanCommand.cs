using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.MealPlanning;

namespace DomusMind.Application.Features.MealPlanning.CreateMealPlan;

public sealed record CreateMealPlanCommand(
    Guid MealPlanId,
    Guid FamilyId,
    DateOnly WeekStart,
    Guid? ResponsibilityDomainId,
    Guid RequestedByUserId) : ICommand<CreateMealPlanResponse>;
