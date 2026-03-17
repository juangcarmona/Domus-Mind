using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
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

        var weekStart = DateTime.SpecifyKind(query.WeekStart.Date, DateTimeKind.Utc);
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
                     && e.StartTime >= weekStart
                     && e.StartTime < weekEnd
                     && e.Status != EventStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var tasks = await _dbContext.Set<HouseholdTask>()
            .AsNoTracking()
            .Where(t => t.FamilyId == familyId
                     && t.DueDate.HasValue
                     && t.DueDate.Value >= weekStart
                     && t.DueDate.Value < weekEnd
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
                var cellDate = DateOnly.FromDateTime(day);
                var dayRoutines = householdRoutines
                    .Where(r => r.Schedule.OccursOn(cellDate))
                    .Select(r => new WeeklyGridRoutineItem(
                        r.Id.Value,
                        r.Name.Value,
                        r.Kind.ToString(),
                        r.Color.Value,
                        r.Schedule.Frequency.ToString(),
                        r.Schedule.Time,
                        r.Scope.ToString()))
                    .ToList();

                return new WeeklyGridCell(day, [], [], dayRoutines);
            })
            .ToList();

        var memberRows = family.Members
            .Select(member =>
            {
                var cells = days
                    .Select(day =>
                    {
                        var memberEvents = events
                            .Where(e => e.StartTime.Date == day.Date
                                     && e.ParticipantIds.Any(p => p.Value == member.Id.Value))
                            .Select(e =>
                            {
                                var participants = e.ParticipantIds
                                    .Select(p => new ParticipantProjection(
                                        p.Value,
                                        memberNameMap.GetValueOrDefault(p.Value, "?")))
                                    .ToList();

                                return new WeeklyGridEventItem(
                                    e.Id.Value,
                                    e.Title.Value,
                                    e.StartTime,
                                    e.EndTime,
                                    e.Status.ToString(),
                                    participants);
                            })
                            .ToList();

                        var memberTasks = tasks
                            .Where(t => t.AssigneeId?.Value == member.Id.Value
                                     && t.DueDate.HasValue
                                     && t.DueDate.Value.Date == day.Date)
                            .Select(t => new WeeklyGridTaskItem(
                                t.Id.Value,
                                t.Title.Value,
                                t.DueDate,
                                t.Status.ToString()))
                            .ToList();

                        var cellDate = DateOnly.FromDateTime(day);

                        var cellRoutines = memberRoutines
                            .Where(r => r.AppliesTo(member.Id) && r.Schedule.OccursOn(cellDate))
                            .Select(r => new WeeklyGridRoutineItem(
                                r.Id.Value,
                                r.Name.Value,
                                r.Kind.ToString(),
                                r.Color.Value,
                                r.Schedule.Frequency.ToString(),
                                r.Schedule.Time,
                                r.Scope.ToString()))
                            .ToList();

                        return new WeeklyGridCell(
                            day,
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
            weekStart,
            weekEnd.AddDays(-1),
            memberRows,
            sharedCells);
    }
}