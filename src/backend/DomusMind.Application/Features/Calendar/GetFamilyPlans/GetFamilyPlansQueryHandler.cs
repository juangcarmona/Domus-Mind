using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Calendar;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Family;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Calendar.GetFamilyPlans;

public sealed class GetFamilyPlansQueryHandler
    : IQueryHandler<GetFamilyPlansQuery, FamilyPlansResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public GetFamilyPlansQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<FamilyPlansResponse> Handle(
        GetFamilyPlansQuery query,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, query.FamilyId, cancellationToken);
        if (!canAccess)
            throw new CalendarException(CalendarErrorCode.AccessDenied, "Access to this family is denied.");

        var familyId = FamilyId.From(query.FamilyId);

        var eventsQuery = _dbContext.Set<CalendarEvent>()
            .AsNoTracking()
            .Where(e => e.FamilyId == familyId);

        if (query.From.HasValue)
            eventsQuery = eventsQuery.Where(e => e.StartTime >= query.From.Value);
        if (query.To.HasValue)
            eventsQuery = eventsQuery.Where(e => e.StartTime <= query.To.Value);

        var events = await eventsQuery
            .OrderBy(e => e.StartTime)
            .ToListAsync(cancellationToken);

        // Apply optional member filter in-memory (participants stored as JSON)
        if (query.MemberId.HasValue)
        {
            var memberId = MemberId.From(query.MemberId.Value);
            events = events.Where(e => e.ParticipantIds.Contains(memberId)).ToList();
        }

        var plans = events
            .Select(e => new FamilyPlanItem(
                e.Id.Value,
                e.Title.Value,
                e.StartTime,
                e.EndTime,
                e.Status.ToString(),
                e.ParticipantIds.Select(p => p.Value).ToList()))
            .ToList();

        return new FamilyPlansResponse(plans);
    }
}
