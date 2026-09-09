using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Temporal;
using DomusMind.Contracts.Calendar;
using DomusMind.Contracts.Family;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.ExternalConnections;
using DomusMind.Domain.Family;
using DomusMind.Domain.Lists;
using DomusMind.Domain.Responsibilities;
using DomusMind.Domain.Tasks;
using DomusMind.Domain.Tasks.Enums;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Family.GetWeeklyGrid;

public sealed class GetWeeklyGridQueryHandler
    : IQueryHandler<GetWeeklyGridQuery, WeeklyGridResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public GetWeeklyGridQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<WeeklyGridResponse> Handle(
        GetWeeklyGridQuery query,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId,
            query.FamilyId,
            cancellationToken);

        if (!canAccess)
            throw new FamilyException(FamilyErrorCode.AccessDenied, "Access to this family is denied.");

        var familyId = FamilyId.From(query.FamilyId);

        var weekStart = query.WeekStart;
        var weekEnd = weekStart.AddDays(7);

        var family = await _dbContext.Set<DomusMind.Domain.Family.Family>()
            .AsNoTracking()
            .Include(f => f.Members)
            .SingleOrDefaultAsync(f => f.Id == familyId, cancellationToken);

        if (family is null)
            throw new FamilyException(FamilyErrorCode.FamilyNotFound, "Family not found.");

        var memberNameMap = family.Members.ToDictionary(m => m.Id.Value, m => m.Name.Value);

        var events = await _dbContext.Set<CalendarEvent>()
            .AsNoTracking()
            .Where(e => e.FamilyId == familyId
                     // Load any event whose date range overlaps the requested week.
                     // EndDate: null means single-day / moment - treat as Date == EndDate.
                     && e.Time.Date < weekEnd
                     && (e.Time.EndDate == null ? e.Time.Date >= weekStart : e.Time.EndDate >= weekStart)
                     && e.Status != EventStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var tasks = await _dbContext.Set<HouseholdTask>()
            .AsNoTracking()
            .Where(t => t.FamilyId == familyId
                     && t.Schedule.Kind != Domain.Tasks.ValueObjects.TaskScheduleKind.None
                     && t.Schedule.Date >= weekStart
                     && t.Schedule.Date < weekEnd
                     && t.Status == HouseholdTaskStatus.Pending)
            .ToListAsync(cancellationToken);

        var temporalListItems = await _dbContext
            .Set<SharedList>()
            .AsNoTracking()
            .Where(l => l.FamilyId == familyId)
            .SelectMany(l => l.Items, (l, i) => new
            {
                ListId = l.Id.Value,
                ListName = l.Name.Value,
                ListColor = l.Color,
                ItemId = i.Id.Value,
                Title = i.Name.Value,
                i.Note,
                i.Checked,
                i.Importance,
                i.DueDate,
                i.Reminder,
                i.Repeat,
                i.ItemAreaId,
                i.TargetMemberId,
            })
            .Where(i => i.DueDate.HasValue || i.Reminder.HasValue || i.Repeat != null)
            .ToListAsync(cancellationToken);

        var areaNameMap = await _dbContext.Set<ResponsibilityDomain>()
            .AsNoTracking()
            .Where(a => a.FamilyId == familyId)
            .ToDictionaryAsync(a => a.Id.Value, a => a.Name.Value, cancellationToken);

        var routines = await _dbContext.Set<Routine>()
            .AsNoTracking()
            .Include("_targetMembers")
            .Where(r => r.FamilyId == familyId
                     && r.Status == RoutineStatus.Active)
            .ToListAsync(cancellationToken);

        var memberIds = family.Members.Select(m => m.Id.Value).ToList();
        var memberIdValues = family.Members.Select(m => m.Id).ToList();
        var activeConnections = await _dbContext
            .Set<ExternalCalendarConnection>()
            .AsNoTracking()
            .Include(c => c.Feeds)
            .Where(c => memberIdValues.Contains(c.MemberId) &&
                        c.Status != ExternalCalendarConnectionStatus.Disconnected)
            .ToListAsync(cancellationToken);

        var selectedFeedIds = activeConnections
            .SelectMany(c => c.Feeds.Where(f => f.IsSelected).Select(f => f.Id))
            .ToHashSet();

        var connectionById = activeConnections.ToDictionary(c => c.Id.Value);

        var weekStartUtc = weekStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var weekEndExclusiveUtc = weekEnd.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var externalEntries = selectedFeedIds.Count == 0
            ? []
            : await _dbContext
                .Set<ExternalCalendarEntry>()
                .AsNoTracking()
                .Where(e => selectedFeedIds.Contains(e.FeedId) &&
                            !e.IsDeleted &&
                            e.StartsAtUtc < weekEndExclusiveUtc &&
                            (e.EndsAtUtc == null || e.EndsAtUtc >= weekStartUtc))
                .ToListAsync(cancellationToken);

        var externalEntriesByMember = externalEntries
            .Where(e => connectionById.ContainsKey(e.ConnectionId))
            .GroupBy(e => connectionById[e.ConnectionId].MemberId.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var days = Enumerable.Range(0, 7)
            .Select(i => weekStart.AddDays(i))
            .ToList();

        var householdRoutines = routines.Where(r => r.Scope == RoutineScope.Household).ToList();
        var memberRoutines = routines.Where(r => r.Scope == RoutineScope.Members).ToList();

        var sharedCells = days
            .Select(day =>
            {
                var dayRoutines = householdRoutines
                    .Where(r => r.Schedule.OccursOn(day))
                    .Select(r => new WeeklyGridRoutineItem(
                        r.Id.Value,
                        r.Name.Value,
                        r.Kind.ToString(),
                        r.Color.Value,
                        r.Schedule.Frequency.ToString(),
                        r.Schedule.Time.HasValue ? r.Schedule.Time.Value.ToString("HH:mm") : null,
                        r.Schedule.EndTime.HasValue ? r.Schedule.EndTime.Value.ToString("HH:mm") : null,
                        r.Scope.ToString()))
                    .ToList();

                // Include calendar events that have no participants (household-wide)
                // and whose date range covers this day (multi-day plans appear on each day).
                var sharedEvents = events
                    .Where(e => !e.ParticipantIds.Any()
                             && e.Time.Date <= day
                             && (e.Time.EndDate.HasValue ? e.Time.EndDate.Value >= day : e.Time.Date == day))
                    .Select(e =>
                    {
                        var (date, time, endDate, endTime) = TemporalParser.FormatEventTime(e.Time);
                        return new WeeklyGridEventItem(
                            e.Id.Value,
                            e.Title.Value,
                            date,
                            time,
                            endDate,
                            endTime,
                            e.Status.ToString(),
                            e.Color.Value,
                                [],
                                false,
                                null,
                                null,
                                null,
                                null,
                                null);
                    })
                    .ToList();

                var unassignedTasks = tasks
                    .Where(t => t.AssigneeId == null && t.Schedule.Date == day)
                    .Select(t =>
                    {
                        var (dueDate, dueTime) = TemporalParser.FormatTaskSchedule(t.Schedule);
                        return new WeeklyGridTaskItem(
                            t.Id.Value,
                            t.Title.Value,
                            dueDate,
                            dueTime,
                            t.Status.ToString(),
                            t.Color.Value);
                    })
                    .ToList();

                var dayStartUtc = day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                var dayEndUtcExclusive = day.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

                var dayListItems = temporalListItems
                    .Where(i =>
                        (i.DueDate.HasValue && i.DueDate.Value == day)
                        || (i.Reminder.HasValue
                            && i.Reminder.Value.UtcDateTime >= dayStartUtc
                            && i.Reminder.Value.UtcDateTime < dayEndUtcExclusive)
                        || RepeatExpansion.FiresInWindow(i.Repeat, day, day))
                    .OrderBy(i => i.Checked)
                    .ThenByDescending(i => i.Importance)
                    .ThenBy(i => i.Title)
                    .Select(i => new WeeklyGridListItem(
                        i.ListId,
                        i.ListName,
                        i.ListColor,
                        i.ItemId,
                        i.Title,
                        i.Note,
                        i.Checked,
                        i.Importance,
                        i.DueDate?.ToString("yyyy-MM-dd"),
                        i.Reminder?.ToString("O"),
                        i.Repeat,
                        i.ItemAreaId,
                        i.ItemAreaId.HasValue ? areaNameMap.GetValueOrDefault(i.ItemAreaId.Value) : null,
                        i.TargetMemberId,
                        i.TargetMemberId.HasValue ? memberNameMap.GetValueOrDefault(i.TargetMemberId.Value) : null))
                    .ToList();

                return new WeeklyGridCell(day.ToString("yyyy-MM-dd"), sharedEvents, unassignedTasks, dayRoutines, dayListItems);
            })
            .ToList();

        var memberRows = family.Members
            .OrderBy(m => m.BirthDate.HasValue ? 0 : 1)
            .ThenBy(m => m.BirthDate)
            .ThenBy(m => m.Name.Value)
            .Select(member =>
            {
                var memberExternalEntries = externalEntriesByMember
                    .GetValueOrDefault(member.Id.Value, []);

                var cells = days
                    .Select(day =>
                    {
                        var memberEvents = events
                            .Where(e => e.Time.Date <= day
                                     && (e.Time.EndDate.HasValue ? e.Time.EndDate.Value >= day : e.Time.Date == day)
                                     && e.ParticipantIds.Any(p => p.Value == member.Id.Value))
                            .Select(e =>
                            {
                                var participants = e.ParticipantIds
                                    .Select(p => new ParticipantProjection(
                                        p.Value,
                                        memberNameMap.GetValueOrDefault(p.Value, "?")))
                                    .ToList();

                                var (date, time, endDate, endTime) = TemporalParser.FormatEventTime(e.Time);
                                return new WeeklyGridEventItem(
                                    e.Id.Value,
                                    e.Title.Value,
                                    date,
                                    time,
                                    endDate,
                                    endTime,
                                    e.Status.ToString(),
                                    e.Color.Value,
                                        participants,
                                        false,
                                        null,
                                        null,
                                        null,
                                        null,
                                        null);
                            })
                            .ToList();

                        var memberExternalEvents = memberExternalEntries
                            .Where(entry =>
                            {
                                var startDate = DateOnly.FromDateTime(entry.StartsAtUtc);
                                var endDate = DateOnly.FromDateTime(entry.EndsAtUtc ?? entry.StartsAtUtc);
                                return startDate <= day && endDate >= day;
                            })
                            .OrderBy(entry => entry.StartsAtUtc)
                            .Select(entry =>
                            {
                                connectionById.TryGetValue(entry.ConnectionId, out var conn);
                                var feedName = conn?.Feeds.FirstOrDefault(f => f.Id == entry.FeedId)?.CalendarName;

                                var date = day.ToString("yyyy-MM-dd");
                                var time = entry.IsAllDay ? null : entry.StartsAtUtc.ToString("HH:mm");
                                var endDate = entry.EndsAtUtc.HasValue
                                    ? DateOnly.FromDateTime(entry.EndsAtUtc.Value).ToString("yyyy-MM-dd")
                                    : null;
                                var endTime = entry.IsAllDay || !entry.EndsAtUtc.HasValue
                                    ? null
                                    : entry.EndsAtUtc.Value.ToString("HH:mm");

                                return new WeeklyGridEventItem(
                                    entry.Id,
                                    entry.Title,
                                    date,
                                    time,
                                    endDate,
                                    endTime,
                                    entry.Status,
                                    "#64748B",
                                    [],
                                    true,
                                    "external_calendar",
                                    conn is null ? null : ExternalCalendarProviderNames.ToProviderLabel(conn.Provider),
                                        entry.OpenInProviderUrl,
                                        feedName,
                                        entry.Location);
                            })
                            .ToList();

                        memberEvents.AddRange(memberExternalEvents);
                        memberEvents = memberEvents
                            .OrderBy(e => e.Time is null ? 1 : 0)
                            .ThenBy(e => e.Time)
                            .ThenBy(e => e.Title)
                            .ToList();

                        var memberTasks = tasks
                            .Where(t => t.AssigneeId?.Value == member.Id.Value
                                     && t.Schedule.Date == day)
                            .Select(t =>
                            {
                                var (dueDate, dueTime) = TemporalParser.FormatTaskSchedule(t.Schedule);
                                return new WeeklyGridTaskItem(
                                    t.Id.Value,
                                    t.Title.Value,
                                    dueDate,
                                    dueTime,
                                    t.Status.ToString(),
                                    t.Color.Value);
                            })
                            .ToList();

                        var cellRoutines = memberRoutines
                            .Where(r => r.AppliesTo(member.Id) && r.Schedule.OccursOn(day))
                            .Select(r => new WeeklyGridRoutineItem(
                                r.Id.Value,
                                r.Name.Value,
                                r.Kind.ToString(),
                                r.Color.Value,
                                r.Schedule.Frequency.ToString(),
                                r.Schedule.Time.HasValue ? r.Schedule.Time.Value.ToString("HH:mm") : null,
                                r.Schedule.EndTime.HasValue ? r.Schedule.EndTime.Value.ToString("HH:mm") : null,
                                r.Scope.ToString()))
                            .ToList();

                        return new WeeklyGridCell(
                            day.ToString("yyyy-MM-dd"),
                            memberEvents,
                            memberTasks,
                            cellRoutines,
                            []);
                    })
                    .ToList();

                return new WeeklyGridMember(
                    member.Id.Value,
                    member.Name.Value,
                    member.Role.Value,
                    cells);
            })
            .ToList();

        return new WeeklyGridResponse(
            weekStart.ToString("yyyy-MM-dd"),
            weekEnd.AddDays(-1).ToString("yyyy-MM-dd"),
            memberRows,
            sharedCells);
    }
}