namespace DomusMind.Contracts.MealPlanning;

public sealed record RequestShoppingListRequest(
    Guid FamilyId,
    string? ShoppingListName = null);
