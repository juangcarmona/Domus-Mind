using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.MealPlanning;

namespace DomusMind.Application.Features.MealPlanning.UpdateMealSlot;

public sealed record UpdateMealSlotCommand(
    Guid MealPlanId,
    string DayOfWeek,
    string MealType,
    string MealSourceType,
    Guid? RecipeId,
    string? FreeText,
    string? Notes,
    bool? IsOptional,
    bool? IsLocked,
    Guid RequestedByUserId) : ICommand<UpdateMealSlotResponse>;
