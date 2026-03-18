using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Temporal;
using DomusMind.Contracts.Family;
using DomusMind.Domain.Family;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Family.GetHouseholdTimeline;

public sealed class GetHouseholdTimelineQueryHandler
    : IQueryHandler<GetHouseholdTimelineQuery, HouseholdTimelineResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public GetHouseholdTimelineQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<HouseholdTimelineResponse> Handle(
        GetHouseholdTimelineQuery query,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, query.FamilyId, cancellationToken);

        if (!canAccess)
            throw new FamilyException(FamilyErrorCode.AccessDenied, "Access to this family is denied.");

        var familyId = FamilyId.From(query.FamilyId);

        var calendarEvents = await _dbContext
            .Set<Domain.Calendar.CalendarEvent>()
            .AsNoTracking()
            .Where(e => e.FamilyId == familyId)
            .ToListAsync(cancellationToken);

        var tasks = await _dbContext
            .Set<Domain.Tasks.HouseholdTask>()
            .AsNoTracking()
            .Where(t => t.FamilyId == familyId)
            .ToListAsync(cancellationToken);

        var routines = await _dbContext
            .Set<Domain.Tasks.Routine>()
            .AsNoTracking()
            .Where(r => r.FamilyId == familyId)
            .ToListAsync(cancellationToken);

        var calendarEntries = calendarEvents
            .Select(e =>
            {
                var (date, _, _, _) = TemporalParser.FormatEventTime(e.Time);
                return new HouseholdTimelineEntry(
                    e.Id.Value,
                    "CalendarEvent",
                    e.Title.Value,
                    date,
                    e.Status.ToString());
            });

        var taskEntries = tasks
            .Select(t =>
            {
                var (dueDate, _) = TemporalParser.FormatTaskSchedule(t.Schedule);
                return new HouseholdTimelineEntry(
                    t.Id.Value,
                    "Task",
                    t.Title.Value,
                    dueDate,
                    t.Status.ToString());
            });

        var routineEntries = routines
            .Select(r => new HouseholdTimelineEntry(
                r.Id.Value,
                "Routine",
                r.Name.Value,
                null,
                r.Status.ToString()));

        var entries = calendarEntries
            .Concat(taskEntries)
            .Concat(routineEntries)
            .OrderBy(e => e.EffectiveDate is null ? 1 : 0)
            .ThenBy(e => e.EffectiveDate)
            .ToList();

        return new HouseholdTimelineResponse(entries);
    }
}
