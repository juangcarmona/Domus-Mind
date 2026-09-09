using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.MealPlanning.Events;

public sealed record ShoppingListRequested(
    Guid Id,
    Guid MealPlanId,
    Guid FamilyId,
    DateTime OccurredAtUtc
) : IDomainEvent;
