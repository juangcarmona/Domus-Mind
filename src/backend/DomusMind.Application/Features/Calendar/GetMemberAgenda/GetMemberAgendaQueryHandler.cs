using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Calendar;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.ExternalConnections;
using DomusMind.Domain.Family;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Calendar.GetMemberAgenda;

public sealed class GetMemberAgendaQueryHandler
    : IQueryHandler<GetMemberAgendaQuery, MemberAgendaResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public GetMemberAgendaQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<MemberAgendaResponse> Handle(
        GetMemberAgendaQuery query,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, query.FamilyId, cancellationToken);

        if (!canAccess)
            throw new CalendarException(CalendarErrorCode.AccessDenied, "Access to this family is denied.");

        var memberId = MemberId.From(query.MemberId);
        var now = DateTime.UtcNow;

        DateTime windowStart = query.From is not null
            ? DateTime.SpecifyKind(DateTime.Parse(query.From), DateTimeKind.Utc)
            : now.Date;

        DateTime windowEnd = query.To is not null
            ? DateTime.SpecifyKind(DateTime.Parse(query.To), DateTimeKind.Utc).AddDays(1).AddSeconds(-1)
            : now.Date.AddDays(1).AddSeconds(-1);

        var items = new List<MemberAgendaItem>();

        // Native events where member is a participant.
        // ParticipantIds is stored as JSON, so we load all family events and filter in memory.
        var familyEvents = await _dbContext
            .Set<CalendarEvent>()
            .AsNoTracking()
            .Where(e => e.FamilyId == FamilyId.From(query.FamilyId) &&
                        e.Status != EventStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var events = familyEvents.Where(e => e.ParticipantIds.Contains(memberId)).ToList();

        foreach (var evt in events)
        {
            var startsAt = evt.Time.Time.HasValue
                ? evt.Time.Date.ToDateTime(evt.Time.Time.Value, DateTimeKind.Utc)
                : evt.Time.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var endsAt = evt.Time.EndDate.HasValue
                ? (evt.Time.EndTime.HasValue
                    ? (DateTime?)evt.Time.EndDate.Value.ToDateTime(evt.Time.EndTime.Value, DateTimeKind.Utc)
                    : evt.Time.EndDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc))
                : null;

            if (startsAt > windowEnd || (endsAt.HasValue && endsAt.Value < windowStart))
                continue;

            items.Add(new MemberAgendaItem(
                "event",
                evt.Title.Value,
                startsAt,
                endsAt,
                !evt.Time.Time.HasValue,
                evt.Status.ToString().ToLowerInvariant(),
                false,
                evt.Id.Value,
                null, null, null, null, null, null, null, null,
                null, null, null));
        }

        // External calendar entries
        var activeConnectionIds = await _dbContext
            .Set<ExternalCalendarConnection>()
            .AsNoTracking()
            .Where(c => c.MemberId == memberId && c.Status != ExternalCalendarConnectionStatus.Disconnected)
            .Select(c => c.Id.Value)
            .ToListAsync(cancellationToken);

        if (activeConnectionIds.Count > 0)
        {
            // Load feeds for these connections
            var feedsByConnection = await _dbContext
                .Set<ExternalCalendarConnection>()
                .AsNoTracking()
                .Include(c => c.Feeds)
                .Where(c => activeConnectionIds.Contains(c.Id.Value))
                .ToListAsync(cancellationToken);

            var selectedFeedIds = feedsByConnection
                .SelectMany(c => c.Feeds.Where(f => f.IsSelected).Select(f => f.Id))
                .ToHashSet();

            // Build lookup for provider info
            var connectionLookup = feedsByConnection.ToDictionary(c => c.Id.Value);

            var externalEntries = await _dbContext
                .Set<ExternalCalendarEntry>()
                .AsNoTracking()
                .Where(e => selectedFeedIds.Contains(e.FeedId) &&
                            !e.IsDeleted &&
                            e.StartsAtUtc <= windowEnd &&
                            (e.EndsAtUtc == null || e.EndsAtUtc >= windowStart))
                .OrderBy(e => e.StartsAtUtc)
                .ToListAsync(cancellationToken);

            foreach (var entry in externalEntries)
            {
                connectionLookup.TryGetValue(entry.ConnectionId, out var conn);
                items.Add(new MemberAgendaItem(
                    "external-calendar-entry",
                    entry.Title,
                    entry.StartsAtUtc,
                    entry.EndsAtUtc,
                    entry.IsAllDay,
                    entry.Status,
                    true,
                    null, null, null,
                    entry.ConnectionId,
                    null,
                    entry.ExternalEventId,
                    entry.Provider,
                    conn is not null ? ExternalCalendarProviderNames.ToProviderLabel(conn.Provider) : null,
                    entry.OpenInProviderUrl,
                    entry.Location,
                    entry.ParticipantSummaryJson,
                    entry.ProviderModifiedAtUtc));
            }
        }

        var sortedItems = items
            .OrderBy(i => i.StartsAtUtc)
            .ThenBy(i => i.AllDay ? 0 : 1)
            .ToList()
            .AsReadOnly();

        var mode = query.From is not null || query.To is not null ? "range" : "day";

        return new MemberAgendaResponse(
            query.MemberId,
            mode,
            windowStart,
            windowEnd,
            sortedItems);
    }
}
