using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.MealPlanning;
using DomusMind.Domain.MealPlanning.Entities;
using DomusMind.Domain.MealPlanning.Enums;
using DomusMind.Domain.MealPlanning.ValueObjects;
using DomainDayOfWeek = DomusMind.Domain.MealPlanning.Enums.DayOfWeek;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.MealPlanning.UpdateMealSlot;

public sealed class UpdateMealSlotCommandHandler : ICommandHandler<UpdateMealSlotCommand, UpdateMealSlotResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public UpdateMealSlotCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<UpdateMealSlotResponse> Handle(
        UpdateMealSlotCommand command,
        CancellationToken cancellationToken)
    {
        var planId = MealPlanId.From(command.MealPlanId);

        var plan = await _dbContext.Set<MealPlan>()
            .Include(mp => mp.MealSlots)
            .FirstOrDefaultAsync(mp => mp.Id == planId, cancellationToken);

        if (plan is null)
            throw new InvalidOperationException($"Meal plan '{command.MealPlanId}' not found.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, plan.FamilyId.Value, cancellationToken);
        if (!canAccess)
            throw new UnauthorizedAccessException("Access to this family is denied.");

        if (!Enum.TryParse<DomainDayOfWeek>(command.DayOfWeek, ignoreCase: true, out var dayOfWeek))
            throw new InvalidOperationException($"Invalid DayOfWeek value: '{command.DayOfWeek}'.");

        if (!Enum.TryParse<MealType>(command.MealType, ignoreCase: true, out var mealType))
            throw new InvalidOperationException($"Invalid MealType value: '{command.MealType}'.");

        if (!Enum.TryParse<MealSourceType>(command.MealSourceType, ignoreCase: true, out var mealSourceType))
            throw new InvalidOperationException($"Invalid MealSourceType value: '{command.MealSourceType}'.");

        RecipeId? recipeId = null;
        if (mealSourceType == MealSourceType.Recipe)
        {
            if (!command.RecipeId.HasValue)
                throw new InvalidOperationException("recipeId is required when mealSourceType is Recipe.");

            recipeId = RecipeId.From(command.RecipeId.Value);

            var recipeExists = await _dbContext.Set<Recipe>()
                .AnyAsync(r => r.Id == recipeId && r.FamilyId == plan.FamilyId, cancellationToken);
            if (!recipeExists)
                throw new InvalidOperationException($"Recipe '{command.RecipeId}' not found or does not belong to this family.");
        }

        var now = DateTime.UtcNow;
        plan.UpdateSlot(dayOfWeek, mealType, mealSourceType, recipeId, command.FreeText,
            command.Notes, command.IsOptional, command.IsLocked, now);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _eventLogWriter.WriteAsync(plan.DomainEvents, cancellationToken);
        plan.ClearDomainEvents();

        var slot = plan.MealSlots.First(s => s.DayOfWeek == dayOfWeek && s.MealType == mealType);

        return new UpdateMealSlotResponse(
            plan.Id.Value,
            slot.DayOfWeek.ToString(),
            slot.MealType.ToString(),
            slot.MealSourceType.ToString(),
            slot.RecipeId?.Value,
            slot.FreeText,
            slot.Notes,
            slot.IsOptional,
            slot.IsLocked);
    }
}
