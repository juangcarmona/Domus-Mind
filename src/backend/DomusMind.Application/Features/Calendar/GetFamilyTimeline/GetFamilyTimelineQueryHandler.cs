using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Calendar;
using DomusMind.Application.Temporal;
using DomusMind.Contracts.Calendar;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Family;
using DomusMind.Domain.Lists;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Calendar.GetFamilyTimeline;

public sealed class GetFamilyTimelineQueryHandler
    : IQueryHandler<GetFamilyTimelineQuery, FamilyTimelineResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public GetFamilyTimelineQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<FamilyTimelineResponse> Handle(
        GetFamilyTimelineQuery query,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, query.FamilyId, cancellationToken);

        if (!canAccess)
            throw new CalendarException(CalendarErrorCode.AccessDenied, "Access to this family is denied.");

        var familyId = FamilyId.From(query.FamilyId);

        var eventsQuery = _dbContext
            .Set<Domain.Calendar.CalendarEvent>()
            .AsNoTracking()
            .Where(e => e.FamilyId == familyId);

        if (query.From.HasValue)
            eventsQuery = eventsQuery.Where(e => e.Time.Date >= query.From.Value);

        if (query.To.HasValue)
            eventsQuery = eventsQuery.Where(e => e.Time.Date <= query.To.Value);

        var events = await eventsQuery
            .OrderBy(e => e.Time.Date)
            .ThenBy(e => e.Time.Time)
            .ToListAsync(cancellationToken);

        var familyForNames = await _dbContext
            .Set<Domain.Family.Family>()
            .AsNoTracking()
            .Include(f => f.Members)
            .SingleOrDefaultAsync(f => f.Id == familyId, cancellationToken);

        var memberNameMap = familyForNames?.Members
            .ToDictionary(m => m.Id.Value, m => m.Name.Value)
            ?? new Dictionary<Guid, string>();

        var items = events
            .Select(e =>
            {
                var participants = e.ParticipantIds
                    .Select(p => new ParticipantProjection(
                        p.Value,
                        memberNameMap.GetValueOrDefault(p.Value, "?")))
                    .ToList();
                var (date, time, endDate, endTime) = TemporalParser.FormatEventTime(e.Time);
                return new FamilyTimelineEventItem(
                    e.Id.Value,
                    e.Title.Value,
                    date,
                    time,
                    endDate,
                    endTime,
                    e.Status.ToString(),
                    e.Color.Value,
                    e.AreaId?.Value,
                    e.ParticipantIds.Select(p => p.Value).ToList(),
                    participants);
            })
            .ToList();

        // Temporal list items with a due date in the requested window
        var listItemsQuery = _dbContext
            .Set<SharedList>()
            .AsNoTracking()
            .Where(l => l.FamilyId == familyId)
            .SelectMany(l => l.Items, (l, i) => new
            {
                ListId = l.Id.Value,
                ListName = l.Name.Value,
                ItemId = i.Id.Value,
                ItemName = i.Name.Value,
                i.DueDate,
                i.Reminder,
                i.Checked
            })
            .Where(i => i.DueDate.HasValue);

        if (query.From.HasValue)
            listItemsQuery = listItemsQuery.Where(i => i.DueDate >= query.From.Value);

        if (query.To.HasValue)
            listItemsQuery = listItemsQuery.Where(i => i.DueDate <= query.To.Value);

        var listItems = await listItemsQuery
            .OrderBy(i => i.DueDate)
            .ThenBy(i => i.Reminder)
            .ToListAsync(cancellationToken);

        var timelineListItems = listItems
            .Select(i => new FamilyTimelineListItem(
                i.ListId,
                i.ListName,
                i.ItemId,
                i.ItemName,
                i.DueDate?.ToString("yyyy-MM-dd"),
                i.Reminder?.ToString("o"),
                i.Checked))
            .ToList();

        return new FamilyTimelineResponse(items, timelineListItems);
    }
}
