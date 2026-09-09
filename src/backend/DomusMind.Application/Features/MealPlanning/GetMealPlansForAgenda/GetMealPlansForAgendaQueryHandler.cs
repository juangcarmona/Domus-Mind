using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.MealPlanning;
using DomusMind.Domain.Family;
using DomusMind.Domain.MealPlanning.Entities;
using DomusMind.Domain.MealPlanning.Enums;
using DomusMind.Domain.MealPlanning.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.MealPlanning.GetMealPlansForAgenda;

public sealed class GetMealPlansForAgendaQueryHandler
    : IQueryHandler<GetMealPlansForAgendaQuery, MealPlansForAgendaResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public GetMealPlansForAgendaQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<MealPlansForAgendaResponse> Handle(
        GetMealPlansForAgendaQuery query,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, query.FamilyId, cancellationToken);
        if (!canAccess)
            throw new UnauthorizedAccessException("Access to this family is denied.");

        var familyId = FamilyId.From(query.FamilyId);

        var plan = await _dbContext.Set<MealPlan>()
            .Include(mp => mp.MealSlots)
            .AsNoTracking()
            .FirstOrDefaultAsync(
                mp => mp.FamilyId == familyId && mp.WeekStart == query.WeekStart,
                cancellationToken);

        if (plan is null)
            return new MealPlansForAgendaResponse([]);

        // Resolve recipe names for labeled slots
        var recipeIds = plan.MealSlots
            .Where(s => s.MealSourceType == MealSourceType.Recipe && s.RecipeId.HasValue)
            .Select(s => s.RecipeId!.Value)
            .Distinct()
            .ToList();

        Dictionary<RecipeId, string> recipeNames = [];
        if (recipeIds.Count > 0)
        {
            recipeNames = await _dbContext.Set<Recipe>()
                .AsNoTracking()
                .Where(r => recipeIds.Contains(r.Id))
                .Select(r => new { r.Id, r.Name })
                .ToDictionaryAsync(r => r.Id, r => r.Name, cancellationToken);
        }

        // Return only slots that have content (exclude fully unplanned/empty with no notes)
        var slots = plan.MealSlots
            .Where(s => s.MealSourceType != MealSourceType.Unplanned || !string.IsNullOrWhiteSpace(s.Notes))
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => (int)s.MealType)
            .Select(s =>
            {
                string? label = s.MealSourceType switch
                {
                    MealSourceType.Recipe when s.RecipeId.HasValue =>
                        recipeNames.TryGetValue(s.RecipeId.Value, out var name) ? name : null,
                    MealSourceType.FreeText => s.FreeText,
                    MealSourceType.Leftovers => "Leftovers",
                    MealSourceType.External => "External",
                    _ => null
                };

                return new MealPlanAgendaSlot(
                    s.DayOfWeek.ToString(),
                    s.MealType.ToString(),
                    s.MealSourceType.ToString(),
                    label,
                    s.Notes,
                    s.IsOptional);
            })
            .ToList();

        return new MealPlansForAgendaResponse(slots);
    }
}
