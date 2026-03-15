using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Calendar;
using DomusMind.Domain.Calendar;
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

        var duration = targetEvent.EndTime.HasValue
            ? targetEvent.EndTime.Value - targetEvent.StartTime
            : TimeSpan.FromHours(1);

        // Search within 7 days after the event's current start time
        var searchStart = targetEvent.StartTime.Date.AddDays(1);
        var searchEnd = searchStart.AddDays(7);

        // Load all non-cancelled family events in the search window (excluding the target event)
        var busyEvents = await _dbContext.Set<CalendarEvent>()
            .AsNoTracking()
            .Where(e => e.FamilyId == familyId
                     && e.Status != EventStatus.Cancelled
                     && e.StartTime < searchEnd
                     && e.StartTime >= searchStart)
            .ToListAsync(cancellationToken);

        // Only consider events that share participants with the target event
        var participantIds = targetEvent.ParticipantIds.Select(p => p.Value).ToHashSet();
        var relevantBusySlots = participantIds.Count == 0
            ? busyEvents
            : busyEvents.Where(e => e.ParticipantIds.Any(p => participantIds.Contains(p.Value))).ToList();

        var maxSuggestions = query.SuggestionCount > 0 ? query.SuggestionCount : 3;
        var suggestions = new List<AlternativeTimeSlot>();
        var candidate = searchStart.AddHours(9); // Start from 9 AM

        while (suggestions.Count < maxSuggestions && candidate < searchEnd)
        {
            var candidateEnd = candidate + duration;

            var hasConflict = relevantBusySlots.Any(e =>
            {
                var eEnd = e.EndTime ?? e.StartTime.AddHours(1);
                return candidate < eEnd && candidateEnd > e.StartTime;
            });

            if (!hasConflict)
                suggestions.Add(new AlternativeTimeSlot(candidate, candidateEnd));

            candidate = candidate.AddHours(1);

            // Skip overnight hours: after 21:00 jump to 09:00 next day
            if (candidate.Hour >= 21)
                candidate = candidate.Date.AddDays(1).AddHours(9);
        }

        return new ProposeAlternativeTimesResponse(query.EventId, suggestions);
    }
}
