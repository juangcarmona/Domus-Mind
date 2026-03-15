using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Calendar;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Family;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Calendar.DetectCalendarConflicts;

public sealed class DetectCalendarConflictsQueryHandler
    : IQueryHandler<DetectCalendarConflictsQuery, CalendarConflictsResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public DetectCalendarConflictsQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<CalendarConflictsResponse> Handle(
        DetectCalendarConflictsQuery query,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, query.FamilyId, cancellationToken);
        if (!canAccess)
            throw new CalendarException(CalendarErrorCode.AccessDenied, "Access to this family is denied.");

        var familyId = FamilyId.From(query.FamilyId);
        var searchTo = query.To ?? query.From.AddDays(7);

        var events = await _dbContext.Set<CalendarEvent>()
            .AsNoTracking()
            .Where(e => e.FamilyId == familyId
                     && e.Status != EventStatus.Cancelled
                     && e.StartTime < searchTo
                     && e.StartTime >= query.From)
            .ToListAsync(cancellationToken);

        var conflicts = new List<CalendarConflict>();

        for (var i = 0; i < events.Count; i++)
        {
            for (var j = i + 1; j < events.Count; j++)
            {
                var a = events[i];
                var b = events[j];

                var aEnd = a.EndTime ?? a.StartTime.AddHours(1);
                var bEnd = b.EndTime ?? b.StartTime.AddHours(1);

                var timesOverlap = a.StartTime < bEnd && aEnd > b.StartTime;
                if (!timesOverlap) continue;

                var sharedParticipants = a.ParticipantIds
                    .Select(p => p.Value)
                    .Intersect(b.ParticipantIds.Select(p => p.Value))
                    .ToList();

                var hasSharedParticipants = sharedParticipants.Count > 0;
                var eitherHasNoParticipants = a.ParticipantIds.Count == 0 || b.ParticipantIds.Count == 0;

                if (!hasSharedParticipants && !eitherHasNoParticipants) continue;

                conflicts.Add(new CalendarConflict(
                    a.Id.Value, a.Title.Value, a.StartTime,
                    b.Id.Value, b.Title.Value, b.StartTime,
                    sharedParticipants));
            }
        }

        return new CalendarConflictsResponse(conflicts);
    }
}
