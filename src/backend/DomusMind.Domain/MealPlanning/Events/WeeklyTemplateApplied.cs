using DomusMind.Domain.Abstractions;

namespace DomusMind.Domain.MealPlanning.Events;

public sealed record WeeklyTemplateApplied(
    Guid Id,
    Guid TemplateId,
    Guid MealPlanId,
    DateTime OccurredAtUtc
) : IDomainEvent;
