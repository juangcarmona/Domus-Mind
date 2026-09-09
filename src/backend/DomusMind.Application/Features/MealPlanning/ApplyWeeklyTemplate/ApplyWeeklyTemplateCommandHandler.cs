using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.MealPlanning;
using DomusMind.Domain.Family;
using DomusMind.Domain.MealPlanning.Entities;
using DomusMind.Domain.MealPlanning.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.MealPlanning.ApplyWeeklyTemplate;

public sealed class ApplyWeeklyTemplateCommandHandler
    : ICommandHandler<ApplyWeeklyTemplateCommand, ApplyWeeklyTemplateResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public ApplyWeeklyTemplateCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<ApplyWeeklyTemplateResponse> Handle(
        ApplyWeeklyTemplateCommand command,
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
            .Include(mp => mp.MealSlots)
            .FirstOrDefaultAsync(mp => mp.FamilyId == familyId && mp.WeekStart == command.WeekStart, cancellationToken);
        if (existingPlan is not null)
        {
            return new ApplyWeeklyTemplateResponse(
                existingPlan.Id.Value,
                existingPlan.FamilyId.Value,
                existingPlan.WeekStart,
                existingPlan.WeekEnd,
                command.TemplateId,
                existingPlan.MealSlots.Count,
                existingPlan.Status.ToString(),
                AlreadyExisted: true);
        }

        var templateId = WeeklyTemplateId.From(command.TemplateId);
        var template = await _dbContext.Set<WeeklyTemplate>()
            .Include(t => t.MealSlotTemplates)
            .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);
        if (template is null)
            throw new InvalidOperationException($"Weekly template '{command.TemplateId}' not found.");
        if (template.FamilyId != familyId)
            throw new InvalidOperationException("Template does not belong to this family.");

        var now = DateTime.UtcNow;
        var mealPlan = MealPlan.CreateFromTemplate(planId, familyId, command.WeekStart, template, now);

        _dbContext.Set<MealPlan>().Add(mealPlan);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _eventLogWriter.WriteAsync(mealPlan.DomainEvents, cancellationToken);
        mealPlan.ClearDomainEvents();

        return new ApplyWeeklyTemplateResponse(
            mealPlan.Id.Value,
            mealPlan.FamilyId.Value,
            mealPlan.WeekStart,
            mealPlan.WeekEnd,
            template.Id.Value,
            mealPlan.MealSlots.Count,
            mealPlan.Status.ToString());
    }
}
