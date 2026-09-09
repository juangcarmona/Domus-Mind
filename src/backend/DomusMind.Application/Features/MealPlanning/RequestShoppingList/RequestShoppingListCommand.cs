using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.MealPlanning;

namespace DomusMind.Application.Features.MealPlanning.RequestShoppingList;

public sealed record RequestShoppingListCommand(
    Guid MealPlanId,
    Guid FamilyId,
    string? ShoppingListName,
    Guid RequestedByUserId) : ICommand<RequestShoppingListResponse>;
