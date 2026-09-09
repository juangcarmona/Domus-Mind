using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Lists;
using DomusMind.Domain.Responsibilities;
using DomusMind.Domain.Lists;
using DomusMind.Domain.Lists.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace DomusMind.Application.Features.Lists.UpdateList;

public sealed class UpdateListCommandHandler
    : ICommandHandler<UpdateListCommand, UpdateListResponse>
{
    private static readonly Regex HexColorRegex = new("^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public UpdateListCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<UpdateListResponse> Handle(
        UpdateListCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Name is not null && string.IsNullOrWhiteSpace(command.Name))
            throw new ListException(ListErrorCode.InvalidInput, "List name cannot be empty.");

        if (command.Color is not null && !HexColorRegex.IsMatch(command.Color))
            throw new ListException(ListErrorCode.InvalidInput, "List color must be a hex value in the format #RRGGBB.");

        var listId = ListId.From(command.ListId);

        var list = await _dbContext.Set<SharedList>()
            .SingleOrDefaultAsync(l => l.Id == listId, cancellationToken);

        if (list is null)
            throw new ListException(ListErrorCode.ListNotFound, "List not found.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, list.FamilyId.Value, cancellationToken);

        if (!canAccess)
            throw new ListException(ListErrorCode.AccessDenied, "Access to this family is denied.");

        ListName? newName = command.Name is not null
            ? ListName.Create(command.Name)
            : null;

        var newAreaId = command.AreaId.HasValue
            ? ResponsibilityDomainId.From(command.AreaId.Value)
            : (ResponsibilityDomainId?)null;

        ListKind? newKind = command.Kind is not null
            ? ListKind.Create(command.Kind)
            : null;

        // linkedPlanId maps to LinkedEntityType = "CalendarEvent"
        string? newLinkedEntityType = command.LinkedPlanId.HasValue ? "CalendarEvent" : null;
        Guid? newLinkedEntityId = command.LinkedPlanId;

        list.UpdateMetadata(
            newName,
            newAreaId,
            command.ClearArea,
            newLinkedEntityType,
            newLinkedEntityId,
            command.ClearLinkedPlan,
            newKind,
            DateTime.UtcNow);

        if (command.ClearColor)
            list.SetColor(null, DateTime.UtcNow);
        else if (command.Color is not null)
            list.SetColor(command.Color, DateTime.UtcNow);

        await _eventLogWriter.WriteAsync(list.DomainEvents, cancellationToken);
        list.ClearDomainEvents();
        await _dbContext.SaveChangesAsync(cancellationToken);

        var linkedPlanId = list.LinkedEntityType == "CalendarEvent" ? list.LinkedEntityId : (Guid?)null;

        return new UpdateListResponse(
            list.Id.Value,
            list.Name.Value,
            list.Color,
            list.AreaId?.Value,
            linkedPlanId,
            list.Kind.Value);
    }
}
