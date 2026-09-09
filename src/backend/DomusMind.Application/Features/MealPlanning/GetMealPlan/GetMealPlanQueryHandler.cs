using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.MealPlanning;
using DomusMind.Domain.Family;
using DomusMind.Domain.MealPlanning.Entities;
using DomusMind.Domain.MealPlanning.Enums;
using DomusMind.Domain.MealPlanning.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.MealPlanning.GetMealPlan;

public sealed class GetMealPlanQueryHandler : IQueryHandler<GetMealPlanQuery, GetMealPlanResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public GetMealPlanQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<GetMealPlanResponse> Handle(GetMealPlanQuery query, CancellationToken cancellationToken)
    {
        MealPlan? plan;

        if (query.MealPlanId.HasValue)
        {
            var planId = MealPlanId.From(query.MealPlanId.Value);
            plan = await _dbContext.Set<MealPlan>()
                .Include(mp => mp.MealSlots)
                .AsNoTracking()
                .FirstOrDefaultAsync(mp => mp.Id == planId, cancellationToken);

            if (plan is null) return new GetMealPlanResponse(null);

            var canAccess = await _authorizationService.CanAccessFamilyAsync(
                query.RequestedByUserId, plan.FamilyId.Value, cancellationToken);
            if (!canAccess)
                throw new UnauthorizedAccessException("Access to this family is denied.");
        }
        else if (query.FamilyId.HasValue && query.WeekStart.HasValue)
        {
            var canAccess = await _authorizationService.CanAccessFamilyAsync(
                query.RequestedByUserId, query.FamilyId.Value, cancellationToken);
            if (!canAccess)
                throw new UnauthorizedAccessException("Access to this family is denied.");

            var familyId = FamilyId.From(query.FamilyId.Value);
            plan = await _dbContext.Set<MealPlan>()
                .Include(mp => mp.MealSlots)
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    mp => mp.FamilyId == familyId && mp.WeekStart == query.WeekStart.Value,
                    cancellationToken);

            if (plan is null) return new GetMealPlanResponse(null);
        }
        else
        {
            throw new InvalidOperationException("Either MealPlanId or (FamilyId + WeekStart) must be provided.");
        }

        var recipeIds = plan.MealSlots
            .Where(s => s.MealSourceType == MealSourceType.Recipe && s.RecipeId.HasValue)
            .Select(s => s.RecipeId!.Value)
            .Distinct()
            .ToList();

        var recipeMap = new Dictionary<RecipeId, Recipe>();
        if (recipeIds.Count > 0)
        {
            recipeMap = await _dbContext.Set<Recipe>()
                .AsNoTracking()
                .Where(r => recipeIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, cancellationToken);
        }

        var orderedMealTypes = new[]
        {
            MealType.Breakfast, MealType.MidMorningSnack, MealType.Lunch,
            MealType.AfternoonSnack, MealType.Dinner
        };

        var slots = plan.MealSlots
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => Array.IndexOf(orderedMealTypes, s.MealType))
            .Select(s =>
            {
                MealSlotRecipeDetail? recipeDetail = null;
                if (s.MealSourceType == MealSourceType.Recipe && s.RecipeId.HasValue
                    && recipeMap.TryGetValue(s.RecipeId.Value, out var r))
                {
                    recipeDetail = new MealSlotRecipeDetail(
                        r.Id.Value,
                        r.Name,
                        r.Servings,
                        r.PrepTimeMinutes,
                        r.TotalTimeMinutes,
                        r.AllowedMealTypes.Select(t => t.ToString()).ToList());
                }

                return new MealSlotDetail(
                    s.DayOfWeek.ToString(),
                    s.MealType.ToString(),
                    s.MealSourceType.ToString(),
                    recipeDetail,
                    s.FreeText,
                    s.Notes,
                    s.IsOptional,
                    s.IsLocked);
            })
            .ToList();

        var detail = new MealPlanDetail(
            plan.Id.Value,
            plan.FamilyId.Value,
            plan.WeekStart,
            plan.WeekEnd,
            plan.Status.ToString(),
            plan.AppliedTemplateId?.Value,
            plan.ShoppingListId,
            plan.ShoppingListVersion,
            plan.LastDerivedAt,
            slots);

        return new GetMealPlanResponse(detail);
    }
}
