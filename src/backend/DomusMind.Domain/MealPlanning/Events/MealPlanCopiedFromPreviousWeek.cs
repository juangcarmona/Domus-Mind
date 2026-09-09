using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.MealPlanning.Events;

public sealed record MealPlanCopiedFromPreviousWeek(
    Guid Id,
    Guid SourceMealPlanId,
    Guid TargetMealPlanId,
    DateTime OccurredAtUtc
) : IDomainEvent;
