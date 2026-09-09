using DomusMind.Domain.Abstractions;
using DomusMind.Domain.MealPlanning.Enums;

namespace DomusMind.Domain.MealPlanning.Events;

public sealed record MealSlotAssigned(
    Guid Id,
    Guid MealPlanId,
    Enums.DayOfWeek DayOfWeek,
    MealType MealType,
    MealSourceType MealSourceType,
    Guid? RecipeId,
    string? FreeText,
    DateTime OccurredAtUtc
) : IDomainEvent;
