using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Calendar;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Family;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Calendar.SuggestEventParticipants;

public sealed class SuggestEventParticipantsQueryHandler
    : IQueryHandler<SuggestEventParticipantsQuery, SuggestEventParticipantsResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public SuggestEventParticipantsQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<SuggestEventParticipantsResponse> Handle(
        SuggestEventParticipantsQuery query,
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

        var family = await _dbContext.Set<Domain.Family.Family>()
            .AsNoTracking()
            .Include(f => f.Members)
            .SingleOrDefaultAsync(f => f.Id == familyId, cancellationToken);

        if (family is null)
            throw new CalendarException(CalendarErrorCode.AccessDenied, "Family not found.");

        var currentParticipants = targetEvent.ParticipantIds.Select(p => p.Value).ToHashSet();
        var candidates = family.Members.Where(m => !currentParticipants.Contains(m.Id.Value)).ToList();

        // Count participation frequency across all family calendar events
        var allEvents = await _dbContext.Set<CalendarEvent>()
            .AsNoTracking()
            .Where(e => e.FamilyId == familyId)
            .ToListAsync(cancellationToken);

        var participationCounts = candidates.ToDictionary(
            m => m.Id.Value,
            m => allEvents.Count(e => e.ParticipantIds.Any(p => p.Value == m.Id.Value)));

        var suggestions = candidates
            .OrderByDescending(m => participationCounts[m.Id.Value])
            .Select(m => new ParticipantSuggestion(m.Id.Value, m.Name.Value, participationCounts[m.Id.Value]))
            .ToList();

        return new SuggestEventParticipantsResponse(query.EventId, suggestions);
    }
}
