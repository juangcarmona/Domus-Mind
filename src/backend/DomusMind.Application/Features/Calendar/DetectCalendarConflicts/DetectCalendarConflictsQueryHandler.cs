using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Temporal;
using DomusMind.Contracts.Calendar;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.ValueObjects;
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

        // Only events with time can have time conflicts; date-only events don't overlap at the minute level.
        var events = await _dbContext.Set<CalendarEvent>()
            .AsNoTracking()
            .Where(e => e.FamilyId == familyId
                     && e.Status != EventStatus.Cancelled
                     && e.Time.Kind != EventTimeKind.Day
                     && e.Time.Date < searchTo
                     && e.Time.Date >= query.From)
            .ToListAsync(cancellationToken);

        var conflicts = new List<CalendarConflict>();

        for (var i = 0; i < events.Count; i++)
        {
            for (var j = i + 1; j < events.Count; j++)
            {
                var a = events[i];
                var b = events[j];

                var aStart = a.Time.Date.ToDateTime(a.Time.Time!.Value);
                var aEnd = a.Time.HasRange
                    ? a.Time.EndDate!.Value.ToDateTime(a.Time.EndTime!.Value)
                    : aStart.AddHours(1);

                var bStart = b.Time.Date.ToDateTime(b.Time.Time!.Value);
                var bEnd = b.Time.HasRange
                    ? b.Time.EndDate!.Value.ToDateTime(b.Time.EndTime!.Value)
                    : bStart.AddHours(1);

                var timesOverlap = aStart < bEnd && aEnd > bStart;
                if (!timesOverlap) continue;

                var sharedParticipants = a.ParticipantIds
                    .Select(p => p.Value)
                    .Intersect(b.ParticipantIds.Select(p => p.Value))
                    .ToList();

                var hasSharedParticipants = sharedParticipants.Count > 0;
                var eitherHasNoParticipants = a.ParticipantIds.Count == 0 || b.ParticipantIds.Count == 0;

                if (!hasSharedParticipants && !eitherHasNoParticipants) continue;

                var (aDate, aTime, _, _) = TemporalParser.FormatEventTime(a.Time);
                var (bDate, bTime, _, _) = TemporalParser.FormatEventTime(b.Time);

                conflicts.Add(new CalendarConflict(
                    a.Id.Value, a.Title.Value, aDate, aTime,
                    b.Id.Value, b.Title.Value, bDate, bTime,
                    sharedParticipants));
            }
        }

        return new CalendarConflictsResponse(conflicts);
    }
}
