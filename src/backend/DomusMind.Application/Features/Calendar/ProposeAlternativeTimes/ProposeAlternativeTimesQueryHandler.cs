using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Calendar;
using DomusMind.Application.Temporal;
using DomusMind.Contracts.Calendar;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.ValueObjects;
using DomusMind.Domain.Family;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Calendar.ProposeAlternativeTimes;

public sealed class ProposeAlternativeTimesQueryHandler
    : IQueryHandler<ProposeAlternativeTimesQuery, ProposeAlternativeTimesResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public ProposeAlternativeTimesQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<ProposeAlternativeTimesResponse> Handle(
        ProposeAlternativeTimesQuery query,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, query.FamilyId, cancellationToken);
        if (!canAccess)
            throw new CalendarException(CalendarErrorCode.AccessDenied, "Access to this family is denied.");

        var familyId = FamilyId.From(query.FamilyId);

        var targetEvent = await _dbContext.Set<CalendarEvent>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                e => e.Id == CalendarEventId.From(query.EventId) && e.FamilyId == familyId,
                cancellationToken);

        if (targetEvent is null)
            throw new CalendarException(CalendarErrorCode.EventNotFound, "Event not found.");

        // Only timed events can propose alternatives (date-only events have no time conflicts)
        if (!targetEvent.Time.HasTime)
            return new ProposeAlternativeTimesResponse(query.EventId, []);

        var startDt = targetEvent.Time.Date.ToDateTime(targetEvent.Time.Time!.Value);
        TimeSpan duration;
        if (targetEvent.Time.HasRange)
        {
            var endDt = targetEvent.Time.EndDate!.Value.ToDateTime(targetEvent.Time.EndTime!.Value);
            duration = endDt - startDt;
        }
        else
        {
            duration = TimeSpan.FromHours(1);
        }

        // Search within 7 days after the event's current start date
        var searchStartDate = targetEvent.Time.Date.AddDays(1);
        var searchEndDate = searchStartDate.AddDays(7);

        // Load all non-cancelled family events with times in the search window
        var busyEvents = await _dbContext.Set<CalendarEvent>()
            .AsNoTracking()
            .Where(e => e.FamilyId == familyId
                     && e.Status != EventStatus.Cancelled
                     && e.Time.Kind != EventTimeKind.Day
                     && e.Time.Date < searchEndDate
                     && e.Time.Date >= searchStartDate)
            .ToListAsync(cancellationToken);

        // Only consider events that share participants with the target event
        var participantIds = targetEvent.ParticipantIds.Select(p => p.Value).ToHashSet();
        var relevantBusySlots = participantIds.Count == 0
            ? busyEvents
            : busyEvents.Where(e => e.ParticipantIds.Any(p => participantIds.Contains(p.Value))).ToList();

        var maxSuggestions = query.SuggestionCount > 0 ? query.SuggestionCount : 3;
        var suggestions = new List<AlternativeTimeSlot>();
        var candidate = searchStartDate.ToDateTime(new TimeOnly(9, 0)); // Start from 09:00

        while (suggestions.Count < maxSuggestions && DateOnly.FromDateTime(candidate) < searchEndDate)
        {
            var candidateEnd = candidate + duration;

            var hasConflict = relevantBusySlots.Any(e =>
            {
                var eStart = e.Time.Date.ToDateTime(e.Time.Time!.Value);
                var eEnd = e.Time.HasRange
                    ? e.Time.EndDate!.Value.ToDateTime(e.Time.EndTime!.Value)
                    : eStart.AddHours(1);
                return candidate < eEnd && candidateEnd > eStart;
            });

            if (!hasConflict)
            {
                var candDate = DateOnly.FromDateTime(candidate);
                var candTime = TimeOnly.FromDateTime(candidate);
                var candEndDate = DateOnly.FromDateTime(candidateEnd);
                var candEndTime = TimeOnly.FromDateTime(candidateEnd);

                // Strip seconds
                candTime = new TimeOnly(candTime.Hour, candTime.Minute);
                candEndTime = new TimeOnly(candEndTime.Hour, candEndTime.Minute);

                suggestions.Add(new AlternativeTimeSlot(
                    candDate.ToString("yyyy-MM-dd"),
                    candTime.ToString("HH:mm"),
                    candEndDate.ToString("yyyy-MM-dd"),
                    candEndTime.ToString("HH:mm")));
            }

            candidate = candidate.AddHours(1);

            // Skip overnight hours: after 21:00 jump to 09:00 next day
            if (candidate.Hour >= 21)
                candidate = candidate.Date.AddDays(1).AddHours(9);
        }

        return new ProposeAlternativeTimesResponse(query.EventId, suggestions);
    }
}
