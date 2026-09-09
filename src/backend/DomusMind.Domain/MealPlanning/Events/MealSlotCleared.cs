using DomusMind.Domain.Abstractions;
using DomusMind.Domain.MealPlanning.Enums;

namespace DomusMind.Domain.MealPlanning.Events;

public sealed record MealSlotCleared(
    Guid Id,
    Guid MealPlanId,
    Enums.DayOfWeek DayOfWeek,
    MealType MealType,
    DateTime OccurredAtUtc
) : IDomainEvent;
