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
using DomusMind.Domain.Tasks.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Family.GetEnrichedTimeline;

public sealed class GetEnrichedTimelineQueryHandler
    : IQueryHandler<GetEnrichedTimelineQuery, EnrichedTimelineResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public GetEnrichedTimelineQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<EnrichedTimelineResponse> Handle(
        GetEnrichedTimelineQuery query,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, query.FamilyId, cancellationToken);
        if (!canAccess)
            throw new FamilyException(FamilyErrorCode.AccessDenied, "Access to this family is denied.");

        var familyId = FamilyId.From(query.FamilyId);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // --- Load all three sources ---
        var calendarEvents = await _dbContext.Set<CalendarEvent>()
            .AsNoTracking()
            .Where(e => e.FamilyId == familyId)
            .ToListAsync(cancellationToken);

        var tasks = await _dbContext.Set<HouseholdTask>()
            .AsNoTracking()
            .Where(t => t.FamilyId == familyId)
            .ToListAsync(cancellationToken);

        var routines = await _dbContext.Set<Routine>()
            .AsNoTracking()
            .Where(r => r.FamilyId == familyId)
            .ToListAsync(cancellationToken);

        // Resolve member names for participant projection
        var familyForNames = await _dbContext
            .Set<Domain.Family.Family>()
            .AsNoTracking()
            .Include(f => f.Members)
            .SingleOrDefaultAsync(f => f.Id == familyId, cancellationToken);

        var memberNameMap = familyForNames?.Members
            .ToDictionary(m => m.Id.Value, m => m.Name.Value)
            ?? new Dictionary<Guid, string>();

        // --- Build raw entries ---
        var calendarEntries = calendarEvents.Select(e =>
        {
            var participants = e.ParticipantIds
                .Select(p => new ParticipantProjection(
                    p.Value,
                    memberNameMap.GetValueOrDefault(p.Value, "?")))
                .ToList();
            var (date, _, _, _) = TemporalParser.FormatEventTime(e.Time);
            return new EnrichedTimelineEntry(
                e.Id.Value,
                "CalendarEvent",
                e.Title.Value,
                date,
                e.Status.ToString(),
                ComputePriority(e.Time.Date, today),
                ComputeGroup(e.Time.Date, today),
                e.Time.Date < today,
                false,
                null,
                participants.Count > 0 ? participants : null,
                e.Color.Value);
        });

        var taskEntries = tasks.Select(t =>
        {
            var (dueDate, _) = TemporalParser.FormatTaskSchedule(t.Schedule);
            return new EnrichedTimelineEntry(
                t.Id.Value,
                "Task",
                t.Title.Value,
                dueDate,
                t.Status.ToString(),
                ComputePriority(t.Schedule.Date, today),
                ComputeGroup(t.Schedule.Date, today),
                t.Schedule.Date.HasValue && t.Schedule.Date.Value < today && t.Status == HouseholdTaskStatus.Pending,
                !t.AssigneeId.HasValue,
                t.AssigneeId?.Value,
                null,
                t.Color.Value);
        });

        var routineEntries = routines.Select(r => new EnrichedTimelineEntry(
            r.Id.Value,
            "Routine",
            r.Name.Value,
            null,
            r.Status.ToString(),
            "Low",
            "Undated",
            false,
            false,
            null,
            null,
            r.Color.Value));

        var allEntries = calendarEntries.Concat(taskEntries).Concat(routineEntries).ToList();

        // --- Apply filters ---
        if (query.TypeFilter is { Count: > 0 })
            allEntries = allEntries.Where(e => query.TypeFilter.Contains(e.EntryType)).ToList();

        if (query.MemberFilter.HasValue)
        {
            var memberId = query.MemberFilter.Value;
            var memberCalendarEventIds = calendarEvents
                .Where(e => e.ParticipantIds.Any(p => p.Value == memberId))
                .Select(e => e.Id.Value)
                .ToHashSet();

            allEntries = allEntries.Where(e =>
                (e.EntryType == "CalendarEvent" && memberCalendarEventIds.Contains(e.EntryId)) ||
                (e.EntryType == "Task" && e.AssigneeId == memberId) ||
                e.EntryType == "Routine").ToList();
        }

        if (query.From.HasValue)
            allEntries = allEntries.Where(e => e.EffectiveDate is null || string.Compare(e.EffectiveDate, query.From.Value.ToString("yyyy-MM-dd"), StringComparison.Ordinal) >= 0).ToList();

        if (query.To.HasValue)
            allEntries = allEntries.Where(e => e.EffectiveDate is null || string.Compare(e.EffectiveDate, query.To.Value.ToString("yyyy-MM-dd"), StringComparison.Ordinal) <= 0).ToList();

        if (query.StatusFilter is { Count: > 0 })
            allEntries = allEntries.Where(e => query.StatusFilter.Contains(e.Status)).ToList();

        // --- Group and sort ---
        var groupOrder = new[] { "Overdue", "Today", "Tomorrow", "ThisWeek", "Later", "Undated" };

        var groups = allEntries
            .GroupBy(e => e.Group)
            .OrderBy(g => Array.IndexOf(groupOrder, g.Key))
            .Select(g => new TimelineGroup(
                g.Key,
                g.OrderBy(e => e.EffectiveDate is null ? 1 : 0)
                 .ThenBy(e => e.EffectiveDate)
                 .ThenBy(e => e.Title)
                 .ToList()))
            .ToList();

        return new EnrichedTimelineResponse(groups, allEntries.Count);
    }

    private static string ComputePriority(DateOnly? effectiveDate, DateOnly today)
    {
        if (!effectiveDate.HasValue) return "Low";
        var date = effectiveDate.Value;
        if (date < today) return "High";
        if (date == today) return "High";
        if (date == today.AddDays(1)) return "Medium";
        if (date <= today.AddDays(7)) return "Medium";
        return "Low";
    }

    private static string ComputeGroup(DateOnly? effectiveDate, DateOnly today)
    {
        if (!effectiveDate.HasValue) return "Undated";
        var date = effectiveDate.Value;
        if (date < today) return "Overdue";
        if (date == today) return "Today";
        if (date == today.AddDays(1)) return "Tomorrow";
        if (date <= today.AddDays(7)) return "ThisWeek";
        return "Later";
    }
}
