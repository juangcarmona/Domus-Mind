using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.MealPlanning.Events;

public sealed record WeeklyTemplateCreated(
    Guid Id,
    Guid TemplateId,
    Guid FamilyId,
    string Name,
    DateTime CreatedAt
) : IDomainEvent
{
    public DateTime OccurredAtUtc => CreatedAt;
}