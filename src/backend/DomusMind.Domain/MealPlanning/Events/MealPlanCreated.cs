using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.MealPlanning.Events;

public sealed record MealPlanCreated(
    Guid Id,
    Guid MealPlanId,
    Guid FamilyId,
    DateOnly WeekStart,
    DateTime CreatedAt
) : IDomainEvent
{
    public DateTime OccurredAtUtc => CreatedAt;
}