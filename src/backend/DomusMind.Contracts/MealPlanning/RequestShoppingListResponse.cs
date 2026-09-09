namespace DomusMind.Contracts.MealPlanning;

public sealed record RequestShoppingListResponse(
    Guid MealPlanId,
    Guid ShoppingListId,
    string ShoppingListName,
    int ItemCount);
