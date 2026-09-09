using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.MealPlanning;

namespace DomusMind.Application.Features.MealPlanning.GetMealPlansForAgenda;

public sealed record GetMealPlansForAgendaQuery(
    Guid FamilyId,
    DateOnly WeekStart,
    Guid RequestedByUserId) : IQuery<MealPlansForAgendaResponse>;