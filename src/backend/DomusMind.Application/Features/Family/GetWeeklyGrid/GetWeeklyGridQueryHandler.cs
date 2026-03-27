using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Temporal;
using DomusMind.Contracts.Calendar;
using DomusMind.Contracts.Family;
using DomusMind.Domain.Calendar;
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
}