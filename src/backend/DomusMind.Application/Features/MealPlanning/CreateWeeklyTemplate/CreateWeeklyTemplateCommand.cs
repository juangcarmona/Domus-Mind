using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.MealPlanning;

namespace DomusMind.Application.Features.MealPlanning.CreateWeeklyTemplate;

public sealed record CreateWeeklyTemplateCommand(
    Guid TemplateId,
    Guid FamilyId,
    string Name,
    IReadOnlyList<TemplateSlotRequest>? Slots,
    Guid RequestedByUserId) : ICommand<CreateWeeklyTemplateResponse>;
