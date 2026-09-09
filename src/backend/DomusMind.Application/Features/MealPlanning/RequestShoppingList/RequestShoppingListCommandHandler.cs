using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.MealPlanning;
using DomusMind.Domain.Family;
using DomusMind.Domain.Lists;
using DomusMind.Domain.Lists.ValueObjects;
using DomusMind.Domain.MealPlanning.Entities;
using DomusMind.Domain.MealPlanning.Enums;
using DomusMind.Domain.MealPlanning.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.MealPlanning.RequestShoppingList;

public sealed class RequestShoppingListCommandHandler
    : ICommandHandler<RequestShoppingListCommand, RequestShoppingListResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public RequestShoppingListCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<RequestShoppingListResponse> Handle(
        RequestShoppingListCommand command,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, command.FamilyId, cancellationToken);
        if (!canAccess)
            throw new UnauthorizedAccessException("Access to this family is denied.");

        var planId = MealPlanId.From(command.MealPlanId);
        var plan = await _dbContext.Set<MealPlan>()
            .Include(mp => mp.MealSlots)
            .FirstOrDefaultAsync(mp => mp.Id == planId, cancellationToken);

        if (plan is null)
            throw new InvalidOperationException($"Meal plan '{command.MealPlanId}' not found.");

        if (plan.FamilyId != FamilyId.From(command.FamilyId))
            throw new InvalidOperationException("Meal plan does not belong to this family.");

        var recipeSlots = plan.MealSlots
            .Where(s => s.MealSourceType == MealSourceType.Recipe && s.RecipeId.HasValue)
            .Select(s => s.RecipeId!.Value)
            .Distinct()
            .ToList();

        if (recipeSlots.Count == 0)
            throw new InvalidOperationException("Cannot derive a shopping list: no recipe slots are assigned.");

        var recipes = await _dbContext.Set<Recipe>()
            .Include(r => r.Ingredients)
            .AsNoTracking()
            .Where(r => recipeSlots.Contains(r.Id))
            .ToListAsync(cancellationToken);

        // Consolidate ingredients: group by name+unit, sum quantities
        var consolidated = recipes
            .SelectMany(r => r.Ingredients)
            .GroupBy(i => (Name: i.Name.ToLowerInvariant(), Unit: (i.Unit ?? string.Empty).ToLowerInvariant()))
            .Select(g => new
            {
                Name = g.First().Name,
                Unit = g.First().Unit,
                Quantity = g.Any(i => i.Quantity.HasValue)
                    ? g.Where(i => i.Quantity.HasValue).Sum(i => i.Quantity!.Value)
                    : (decimal?)null
            })
            .ToList();

        var now = DateTime.UtcNow;
        var familyId = FamilyId.From(command.FamilyId);

        var listName = !string.IsNullOrWhiteSpace(command.ShoppingListName)
            ? command.ShoppingListName
            : $"Shopping list - week of {plan.WeekStart:yyyy-MM-dd}";

        var listId = ListId.New();
        var sharedList = SharedList.Create(
            listId,
            familyId,
            ListName.Create(listName),
            ListKind.Create("shopping"),
            areaId: null,
            linkedEntityType: "MealPlan",
            linkedEntityId: command.MealPlanId,
            createdAtUtc: now);

        foreach (var item in consolidated)
        {
            var quantity = item.Quantity.HasValue
                ? $"{item.Quantity.Value}{(string.IsNullOrEmpty(item.Unit) ? "" : " " + item.Unit)}"
                : null;

            sharedList.AddItem(ListItemId.New(), ListItemName.Create(item.Name), quantity, null, now);
        }

        _dbContext.Set<SharedList>().Add(sharedList);

        // Record the shopping list reference on the meal plan
        plan.RecordShoppingListCreated(listId.Value, now);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _eventLogWriter.WriteAsync(sharedList.DomainEvents, cancellationToken);
        sharedList.ClearDomainEvents();

        await _eventLogWriter.WriteAsync(plan.DomainEvents, cancellationToken);
        plan.ClearDomainEvents();

        return new RequestShoppingListResponse(
            plan.Id.Value,
            listId.Value,
            listName,
            sharedList.Items.Count);
    }
}
