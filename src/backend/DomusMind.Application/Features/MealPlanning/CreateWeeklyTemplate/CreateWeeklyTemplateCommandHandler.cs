using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.MealPlanning;
using DomusMind.Domain.Family;
using DomusMind.Domain.MealPlanning.Entities;
using DomusMind.Domain.MealPlanning.Enums;
using DomusMind.Domain.MealPlanning.ValueObjects;
using DomainDayOfWeek = DomusMind.Domain.MealPlanning.Enums.DayOfWeek;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.MealPlanning.CreateWeeklyTemplate;

public sealed class CreateWeeklyTemplateCommandHandler
    : ICommandHandler<CreateWeeklyTemplateCommand, CreateWeeklyTemplateResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public CreateWeeklyTemplateCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<CreateWeeklyTemplateResponse> Handle(
        CreateWeeklyTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, command.FamilyId, cancellationToken);
        if (!canAccess)
            throw new UnauthorizedAccessException("Access to this family is denied.");

        var familyId = FamilyId.From(command.FamilyId);

        var templateId = WeeklyTemplateId.From(command.TemplateId);
        var duplicate = await _dbContext.Set<WeeklyTemplate>()
            .AnyAsync(t => t.Id == templateId, cancellationToken);
        if (duplicate)
            throw new InvalidOperationException($"A weekly template with id '{command.TemplateId}' already exists.");

        var nameExists = await _dbContext.Set<WeeklyTemplate>()
            .AnyAsync(t => t.FamilyId == familyId && t.Name == command.Name, cancellationToken);
        if (nameExists)
            throw new InvalidOperationException($"A weekly template named '{command.Name}' already exists in this family.");

        var now = DateTime.UtcNow;
        var template = WeeklyTemplate.Create(templateId, familyId, command.Name, now);

        if (command.Slots is { Count: > 0 })
        {
            var seenKeys = new HashSet<(string, string)>();
            foreach (var slotRequest in command.Slots)
            {
                if (!Enum.TryParse<DomainDayOfWeek>(slotRequest.DayOfWeek, ignoreCase: true, out var day))
                    throw new InvalidOperationException($"Invalid DayOfWeek: '{slotRequest.DayOfWeek}'.");
                if (!Enum.TryParse<MealType>(slotRequest.MealType, ignoreCase: true, out var mealType))
                    throw new InvalidOperationException($"Invalid MealType: '{slotRequest.MealType}'.");
                if (!Enum.TryParse<MealSourceType>(slotRequest.MealSourceType, ignoreCase: true, out var sourceType))
                    throw new InvalidOperationException($"Invalid MealSourceType: '{slotRequest.MealSourceType}'.");

                var key = (slotRequest.DayOfWeek.ToLowerInvariant(), slotRequest.MealType.ToLowerInvariant());
                if (!seenKeys.Add(key))
                    throw new InvalidOperationException($"Duplicate slot {slotRequest.DayOfWeek}/{slotRequest.MealType} in request.");

                RecipeId? recipeId = null;
                if (sourceType == MealSourceType.Recipe)
                {
                    if (!slotRequest.RecipeId.HasValue)
                        throw new InvalidOperationException("recipeId required for Recipe source type.");
                    recipeId = RecipeId.From(slotRequest.RecipeId.Value);
                    var recipeExists = await _dbContext.Set<Recipe>()
                        .AnyAsync(r => r.Id == recipeId && r.FamilyId == familyId, cancellationToken);
                    if (!recipeExists)
                        throw new InvalidOperationException($"Recipe '{slotRequest.RecipeId}' not found or does not belong to this family.");
                }

                var slot = MealSlotTemplate.Create(
                    MealSlotTemplateId.New(), day, mealType, templateId,
                    sourceType, recipeId, slotRequest.FreeText, slotRequest.Notes,
                    slotRequest.IsOptional, slotRequest.IsLocked, now);

                template.AddSlotTemplate(slot);
            }
        }

        _dbContext.Set<WeeklyTemplate>().Add(template);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _eventLogWriter.WriteAsync(template.DomainEvents, cancellationToken);
        template.ClearDomainEvents();

        return new CreateWeeklyTemplateResponse(
            template.Id.Value,
            template.FamilyId.Value,
            template.Name,
            template.MealSlotTemplates.Count);
    }
}
