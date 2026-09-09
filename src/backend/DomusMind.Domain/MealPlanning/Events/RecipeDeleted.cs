using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.MealPlanning.Events;

public sealed record RecipeDeleted(
    Guid Id,
    Guid RecipeId,
    Guid FamilyId,
    DateTime DeletedAt
) : IDomainEvent
{
    public DateTime OccurredAtUtc => DeletedAt;
}
