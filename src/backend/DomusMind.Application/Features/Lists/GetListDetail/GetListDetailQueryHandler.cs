using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Lists;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Lists;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Lists.GetListDetail;

public sealed class GetListDetailQueryHandler
    : IQueryHandler<GetListDetailQuery, GetListDetailResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public GetListDetailQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<GetListDetailResponse> Handle(
        GetListDetailQuery query,
        CancellationToken cancellationToken)
    {
        var listId = ListId.From(query.ListId);

        var list = await _dbContext.Set<SharedList>()
            .AsNoTracking()
            .Include(l => l.Items)
            .SingleOrDefaultAsync(l => l.Id == listId, cancellationToken);

        if (list is null)
            throw new ListException(ListErrorCode.ListNotFound, "Shared list not found.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, list.FamilyId.Value, cancellationToken);

        if (!canAccess)
            throw new ListException(ListErrorCode.AccessDenied, "Access to this family is denied.");

        var items = list.Items
            .OrderBy(i => i.Order)
            .Select(i => new ListItemDetail(
                i.Id.Value,
                i.Name.Value,
                i.Checked,
                i.Quantity,
                i.Note,
                i.Order,
                i.Importance,
                i.DueDate,
                i.Reminder,
                i.Repeat,
                i.ItemAreaId,
                i.TargetMemberId))
            .ToList();

        string? linkedEntityDisplayName = null;
        Guid? linkedPlanId = null;
        if (list.LinkedEntityType == "CalendarEvent" && list.LinkedEntityId.HasValue)
        {
            linkedPlanId = list.LinkedEntityId;
            var eventId = CalendarEventId.From(list.LinkedEntityId.Value);
            var calendarEvent = await _dbContext.Set<CalendarEvent>()
                .AsNoTracking()
                .SingleOrDefaultAsync(e => e.Id == eventId, cancellationToken);
            linkedEntityDisplayName = calendarEvent?.Title.Value;
        }

        return new GetListDetailResponse(
            list.Id.Value,
            list.Name.Value,
            list.Kind.Value,
            list.Color,
            list.AreaId?.Value,
            linkedPlanId,
            linkedEntityDisplayName,
            list.UncheckedCount,
            items);
    }
}
