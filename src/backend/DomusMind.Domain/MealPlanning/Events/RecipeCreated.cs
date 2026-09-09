using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.MealPlanning.Events;

public sealed record RecipeCreated(
    Guid Id,
    Guid RecipeId,
    Guid FamilyId,
    string Name,
    DateTime CreatedAt
) : IDomainEvent
{
    public DateTime OccurredAtUtc => CreatedAt;
}