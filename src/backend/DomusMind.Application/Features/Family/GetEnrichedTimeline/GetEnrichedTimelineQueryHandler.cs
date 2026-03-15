using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Family;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Family;
using DomusMind.Domain.Tasks;
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
        var now = DateTime.UtcNow;
        var today = now.Date;

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

        // --- Build raw entries ---
        var calendarEntries = calendarEvents.Select(e => new EnrichedTimelineEntry(
            e.Id.Value,
            "CalendarEvent",
            e.Title.Value,
            e.StartTime,
            e.Status.ToString(),
            ComputePriority(e.StartTime, today),
            ComputeGroup(e.StartTime, today),
            e.StartTime.Date < today,
            false,
            null));

        var taskEntries = tasks.Select(t => new EnrichedTimelineEntry(
            t.Id.Value,
            "Task",
            t.Title.Value,
            t.DueDate,
            t.Status.ToString(),
            ComputePriority(t.DueDate, today),
            ComputeGroup(t.DueDate, today),
            t.DueDate.HasValue && t.DueDate.Value.Date < today && t.Status == Domain.Tasks.TaskStatus.Pending,
            !t.AssigneeId.HasValue,
            t.AssigneeId?.Value));

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
            null));

        var allEntries = calendarEntries.Concat(taskEntries).Concat(routineEntries).ToList();

        // --- Apply filters ---
        if (query.TypeFilter is { Count: > 0 })
            allEntries = allEntries.Where(e => query.TypeFilter.Contains(e.EntryType)).ToList();

        if (query.MemberFilter.HasValue)
        {
            var memberId = query.MemberFilter.Value;
            // Keep calendar events where member is a participant, tasks assigned to member, all routines
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
            allEntries = allEntries.Where(e => !e.EffectiveDate.HasValue || e.EffectiveDate.Value >= query.From.Value).ToList();

        if (query.To.HasValue)
            allEntries = allEntries.Where(e => !e.EffectiveDate.HasValue || e.EffectiveDate.Value <= query.To.Value).ToList();

        if (query.StatusFilter is { Count: > 0 })
            allEntries = allEntries.Where(e => query.StatusFilter.Contains(e.Status)).ToList();

        // --- Group and sort ---
        var groupOrder = new[] { "Overdue", "Today", "Tomorrow", "ThisWeek", "Later", "Undated" };

        var groups = allEntries
            .GroupBy(e => e.Group)
            .OrderBy(g => Array.IndexOf(groupOrder, g.Key))
            .Select(g => new TimelineGroup(
                g.Key,
                g.OrderBy(e => e.EffectiveDate.HasValue ? 0 : 1)
                 .ThenBy(e => e.EffectiveDate)
                 .ThenBy(e => e.Title)
                 .ToList()))
            .ToList();

        return new EnrichedTimelineResponse(groups, allEntries.Count);
    }

    private static string ComputePriority(DateTime? effectiveDate, DateTime today)
    {
        if (!effectiveDate.HasValue) return "Low";
        var date = effectiveDate.Value.Date;
        if (date < today) return "High";
        if (date == today) return "High";
        if (date == today.AddDays(1)) return "Medium";
        if (date <= today.AddDays(7)) return "Medium";
        return "Low";
    }

    private static string ComputeGroup(DateTime? effectiveDate, DateTime today)
    {
        if (!effectiveDate.HasValue) return "Undated";
        var date = effectiveDate.Value.Date;
        if (date < today) return "Overdue";
        if (date == today) return "Today";
        if (date == today.AddDays(1)) return "Tomorrow";
        if (date <= today.AddDays(7)) return "ThisWeek";
        return "Later";
    }
}
