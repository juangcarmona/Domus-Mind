using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Temporal;
using DomusMind.Contracts.Calendar;
using DomusMind.Contracts.Family;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.ExternalConnections;
using DomusMind.Domain.Family;
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

        var routines = await _dbContext.Set<Routine>()
            .AsNoTracking()
            .Include("_targetMembers")
            .Where(r => r.FamilyId == familyId
                     && r.Status == RoutineStatus.Active)
            .ToListAsync(cancellationToken);

        // Query by FamilyId (EF-translatable) and filter to active connections.
        // memberIds.Contains(c.MemberId.Value) is non-translatable because the
        // .Value accessor on the MemberId value object cannot be rendered to SQL
        // by EF Core when the source collection is a local List<Guid>.
        var activeConnections = await _dbContext
            .Set<ExternalCalendarConnection>()
            .AsNoTracking()
            .Include(c => c.Feeds)
            .Where(c => c.FamilyId == familyId &&
                        c.Status != ExternalCalendarConnectionStatus.Disconnected)
            .ToListAsync(cancellationToken);

        var selectedFeedIds = activeConnections
            .SelectMany(c => c.Feeds.Where(f => f.IsSelected).Select(f => f.Id))
            .ToHashSet();

        var feedColorById = activeConnections
            .SelectMany(c => c.Feeds.Where(f => f.IsSelected))
            .ToDictionary(f => f.Id, f => f.ColorHex);

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
                            []);
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

                return new WeeklyGridCell(day.ToString("yyyy-MM-dd"), sharedEvents, unassignedTasks, dayRoutines);
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
                                    participants);
                            })
                            .ToList();

                        var memberExternalEvents = memberExternalEntries
                            .Where(entry =>
                            {
                                // Use the event's original timezone for local-date bucketing.
                                // Without this, a late-evening event stored as UTC would fall
                                // on the next calendar day for households in negative-offset zones.
                                var startDate = LocalDateOf(entry.StartsAtUtc, entry.OriginalTimezone);
                                var endDate = LocalDateOf(entry.EndsAtUtc ?? entry.StartsAtUtc, entry.OriginalTimezone);
                                return startDate <= day && endDate >= day;
                            })
                            .OrderBy(entry => entry.StartsAtUtc)
                            .Select(entry =>
                            {
                                connectionById.TryGetValue(entry.ConnectionId, out var conn);

                                var date = day.ToString("yyyy-MM-dd");
                                // Format start/end times in the event's original local timezone
                                // so the household sees the same hour that was in the invitation.
                                var time = entry.IsAllDay ? null : FormatLocalTime(entry.StartsAtUtc, entry.OriginalTimezone);
                                var endDate = entry.EndsAtUtc.HasValue
                                    ? LocalDateOf(entry.EndsAtUtc.Value, entry.OriginalTimezone).ToString("yyyy-MM-dd")
                                    : null;
                                var endTime = entry.IsAllDay || !entry.EndsAtUtc.HasValue
                                    ? null
                                    : FormatLocalTime(entry.EndsAtUtc.Value, entry.OriginalTimezone);

                                // Use provider-supplied feed color; fall back to a neutral slate.
                                var color = feedColorById.GetValueOrDefault(entry.FeedId) ?? "#64748B";

                                return new WeeklyGridEventItem(
                                    entry.Id,
                                    entry.Title,
                                    date,
                                    time,
                                    endDate,
                                    endTime,
                                    entry.Status,
                                    color,
                                    [],
                                    true,
                                    "external_calendar",
                                    conn is null ? null : ExternalCalendarProviderNames.ToProviderLabel(conn.Provider),
                                    entry.OpenInProviderUrl,
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
                            cellRoutines);
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

    // -------------------------------------------------------------------------
    // Local-time helpers for external calendar entry display
    //
    // ExternalCalendarEntry.StartsAtUtc/EndsAtUtc are UTC coordinates.
    // DomusMind is a local household product — events must be shown in the
    // household's local time. OriginalTimezone carries the IANA or Windows zone
    // ID from the source invitation (e.g. "America/New_York").
    //
    // When OriginalTimezone is null or UTC the UTC value is used directly;
    // this is correct because event times imported without a zone offset
    // (or already in UTC) have no additional conversion required.
    //
    // Microsoft Graph returns Windows timezone IDs (e.g. "Eastern Standard Time")
    // while Linux/macOS uses IANA IDs. ResolveTimeZone handles both platforms by
    // attempting a direct lookup first, then cross-platform conversion via the
    // built-in TimeZoneInfo conversion APIs added in .NET 6.
    // -------------------------------------------------------------------------

    private static TimeZoneInfo? ResolveTimeZone(string id)
    {
        if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var tz))
            return tz;

        // Direct lookup failed — the ID may be a Windows ID on a IANA platform, or vice versa.
        if (TimeZoneInfo.TryConvertWindowsIdToIanaId(id, out var ianaId) &&
            TimeZoneInfo.TryFindSystemTimeZoneById(ianaId, out var tzFromIana))
            return tzFromIana;

        if (TimeZoneInfo.TryConvertIanaIdToWindowsId(id, out var winId) &&
            TimeZoneInfo.TryFindSystemTimeZoneById(winId, out var tzFromWin))
            return tzFromWin;

        return null;
    }

    private static string FormatLocalTime(DateTime utc, string? originalTimezone)
    {
        if (string.IsNullOrWhiteSpace(originalTimezone) ||
            string.Equals(originalTimezone, "UTC", StringComparison.OrdinalIgnoreCase))
        {
            return utc.ToString("HH:mm");
        }

        var tz = ResolveTimeZone(originalTimezone);
        return tz is not null
            ? TimeZoneInfo.ConvertTimeFromUtc(utc, tz).ToString("HH:mm")
            : utc.ToString("HH:mm");
    }

    private static DateOnly LocalDateOf(DateTime utc, string? originalTimezone)
    {
        if (string.IsNullOrWhiteSpace(originalTimezone) ||
            string.Equals(originalTimezone, "UTC", StringComparison.OrdinalIgnoreCase))
        {
            return DateOnly.FromDateTime(utc);
        }

        var tz = ResolveTimeZone(originalTimezone);
        return tz is not null
            ? DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utc, tz))
            : DateOnly.FromDateTime(utc);
    }
}