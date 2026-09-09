using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.MealPlanning;

namespace DomusMind.Application.Features.MealPlanning.GetMealPlan;

/// <summary>
/// Lookup by planId XOR by familyId+weekStart. One must be provided.
/// </summary>
public sealed record GetMealPlanQuery(
    Guid? MealPlanId,
    Guid? FamilyId,
    DateOnly? WeekStart,
    Guid RequestedByUserId) : IQuery<GetMealPlanResponse>;
