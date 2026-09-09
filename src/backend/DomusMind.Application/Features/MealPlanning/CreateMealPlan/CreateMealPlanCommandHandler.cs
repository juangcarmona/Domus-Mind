using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.MealPlanning;
using DomusMind.Domain.Family;
using DomusMind.Domain.MealPlanning.Entities;
using DomusMind.Domain.MealPlanning.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.MealPlanning.CreateMealPlan;

public sealed class CreateMealPlanCommandHandler : ICommandHandler<CreateMealPlanCommand, CreateMealPlanResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public CreateMealPlanCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<CreateMealPlanResponse> Handle(
        CreateMealPlanCommand command,
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

        var existingPlan = await _dbContext.Set<MealPlan>()
            .FirstOrDefaultAsync(mp => mp.FamilyId == familyId && mp.WeekStart == command.WeekStart, cancellationToken);
        if (existingPlan is not null)
        {
            return new CreateMealPlanResponse(
                existingPlan.Id.Value,
                existingPlan.FamilyId.Value,
                existingPlan.WeekStart,
                existingPlan.WeekEnd,
                existingPlan.Status.ToString(),
                existingPlan.CreatedAtUtc,
                AlreadyExisted: true);
        }

        var now = DateTime.UtcNow;
        var mealPlan = MealPlan.Create(planId, familyId, command.WeekStart, true, now);

        _dbContext.Set<MealPlan>().Add(mealPlan);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _eventLogWriter.WriteAsync(mealPlan.DomainEvents, cancellationToken);
        mealPlan.ClearDomainEvents();

        return new CreateMealPlanResponse(
            mealPlan.Id.Value,
            mealPlan.FamilyId.Value,
            mealPlan.WeekStart,
            mealPlan.WeekEnd,
            mealPlan.Status.ToString(),
            mealPlan.CreatedAtUtc);
    }
}
