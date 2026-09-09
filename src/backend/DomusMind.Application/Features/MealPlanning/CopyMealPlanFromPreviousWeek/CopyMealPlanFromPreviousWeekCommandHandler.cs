using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.MealPlanning;
using DomusMind.Domain.Family;
using DomusMind.Domain.MealPlanning.Entities;
using DomusMind.Domain.MealPlanning.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.MealPlanning.CopyMealPlanFromPreviousWeek;

public sealed class CopyMealPlanFromPreviousWeekCommandHandler
    : ICommandHandler<CopyMealPlanFromPreviousWeekCommand, CopyMealPlanFromPreviousWeekResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public CopyMealPlanFromPreviousWeekCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<CopyMealPlanFromPreviousWeekResponse> Handle(
        CopyMealPlanFromPreviousWeekCommand command,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, command.FamilyId, cancellationToken);
        if (!canAccess)
            throw new UnauthorizedAccessException("Access to this family is denied.");

        var familyId = FamilyId.From(command.FamilyId);

        var planId = MealPlanId.From(command.MealPlanId);
        var duplicate = await _dbContext.Set<MealPlan>()
            .AnyAsync(mp => mp.Id == planId, cancellationToken);
        if (duplicate)
            throw new InvalidOperationException($"A meal plan with id '{command.MealPlanId}' already exists.");

        var existingTargetPlan = await _dbContext.Set<MealPlan>()
            .Include(mp => mp.MealSlots)
            .FirstOrDefaultAsync(mp => mp.FamilyId == familyId && mp.WeekStart == command.WeekStart, cancellationToken);
        if (existingTargetPlan is not null)
        {
            return new CopyMealPlanFromPreviousWeekResponse(
                existingTargetPlan.Id.Value,
                existingTargetPlan.FamilyId.Value,
                existingTargetPlan.WeekStart,
                existingTargetPlan.WeekEnd,
                null,
                existingTargetPlan.Status.ToString(),
                existingTargetPlan.MealSlots.Count,
                AlreadyExisted: true);
        }

        MealPlan? sourcePlan;
        if (command.SourceMealPlanId.HasValue)
        {
            var sourceId = MealPlanId.From(command.SourceMealPlanId.Value);
            sourcePlan = await _dbContext.Set<MealPlan>()
                .Include(mp => mp.MealSlots)
                .FirstOrDefaultAsync(mp => mp.Id == sourceId, cancellationToken);

            if (sourcePlan is null)
                return new CopyMealPlanFromPreviousWeekResponse(
                    null, command.FamilyId, command.WeekStart, command.WeekStart.AddDays(6),
                    null, null, 0, Success: false, ErrorCode: "NoPreviousPlan");

            if (sourcePlan.FamilyId != familyId)
                throw new InvalidOperationException("Source plan does not belong to this family.");
        }
        else
        {
            // Resolve the immediately preceding week
            var previousWeekStart = command.WeekStart.AddDays(-7);
            sourcePlan = await _dbContext.Set<MealPlan>()
                .Include(mp => mp.MealSlots)
                .FirstOrDefaultAsync(
                    mp => mp.FamilyId == familyId && mp.WeekStart == previousWeekStart,
                    cancellationToken);

            if (sourcePlan is null)
                return new CopyMealPlanFromPreviousWeekResponse(
                    null, command.FamilyId, command.WeekStart, command.WeekStart.AddDays(6),
                    null, null, 0, Success: false, ErrorCode: "NoPreviousPlan");
        }

        var now = DateTime.UtcNow;
        var newPlan = MealPlan.CopyFromPlan(planId, familyId, command.WeekStart, sourcePlan, now);

        _dbContext.Set<MealPlan>().Add(newPlan);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _eventLogWriter.WriteAsync(newPlan.DomainEvents, cancellationToken);
        newPlan.ClearDomainEvents();

        return new CopyMealPlanFromPreviousWeekResponse(
            newPlan.Id.Value,
            newPlan.FamilyId.Value,
            newPlan.WeekStart,
            newPlan.WeekEnd,
            sourcePlan.Id.Value,
            newPlan.Status.ToString(),
            newPlan.MealSlots.Count);
    }
}
