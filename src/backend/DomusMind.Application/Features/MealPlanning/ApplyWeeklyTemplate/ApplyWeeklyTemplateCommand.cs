using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.MealPlanning;

namespace DomusMind.Application.Features.MealPlanning.ApplyWeeklyTemplate;

public sealed record ApplyWeeklyTemplateCommand(
    Guid MealPlanId,
    Guid FamilyId,
    DateOnly WeekStart,
    Guid TemplateId,
    Guid RequestedByUserId) : ICommand<ApplyWeeklyTemplateResponse>;
