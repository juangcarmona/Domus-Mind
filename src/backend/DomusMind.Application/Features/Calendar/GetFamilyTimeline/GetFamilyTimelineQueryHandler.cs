using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Calendar;
using DomusMind.Contracts.Calendar;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Family;
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
            eventsQuery = eventsQuery.Where(e => e.StartTime >= query.From.Value);

        if (query.To.HasValue)
            eventsQuery = eventsQuery.Where(e => e.StartTime <= query.To.Value);

        var events = await eventsQuery
            .OrderBy(e => e.StartTime)
            .ToListAsync(cancellationToken);

        var items = events
            .Select(e => new FamilyTimelineEventItem(
                e.Id.Value,
                e.Title.Value,
                e.StartTime,
                e.EndTime,
                e.Status.ToString(),
                e.ParticipantIds.Select(p => p.Value).ToList()))
            .ToList();

        return new FamilyTimelineResponse(items);
    }
}
