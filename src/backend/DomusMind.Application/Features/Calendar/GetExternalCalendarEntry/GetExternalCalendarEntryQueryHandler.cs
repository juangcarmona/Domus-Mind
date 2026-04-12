using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Calendar;
using DomusMind.Domain.Calendar.ExternalConnections;
using DomusMind.Domain.Family;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Calendar.GetExternalCalendarEntry;

public sealed class GetExternalCalendarEntryQueryHandler
    : IQueryHandler<GetExternalCalendarEntryQuery, GetExternalCalendarEntryResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public GetExternalCalendarEntryQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<GetExternalCalendarEntryResponse> Handle(
        GetExternalCalendarEntryQuery query,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, query.FamilyId, cancellationToken);

        if (!canAccess)
            throw new CalendarException(CalendarErrorCode.AccessDenied, "Access to this family is denied.");

        var memberId = MemberId.From(query.MemberId);

        // Load the entry, verifying it belongs to a connection owned by the requested member.
        var entry = await _dbContext
            .Set<ExternalCalendarEntry>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == query.EntryId && !e.IsDeleted, cancellationToken);

        if (entry is null)
            throw new CalendarException(CalendarErrorCode.EventNotFound, "External calendar entry not found.");

        // Verify the feed's connection belongs to the requested member.
        var connection = await _dbContext
            .Set<ExternalCalendarConnection>()
            .AsNoTracking()
            .Include(c => c.Feeds)
            .FirstOrDefaultAsync(c => c.Id == ExternalCalendarConnectionId.From(entry.ConnectionId)
                                   && c.MemberId == memberId
                                   && c.Status != ExternalCalendarConnectionStatus.Disconnected,
                                   cancellationToken);

        if (connection is null)
            throw new CalendarException(CalendarErrorCode.AccessDenied, "Entry does not belong to the requested member.");

        var feed = connection.Feeds.FirstOrDefault(f => f.Id == entry.FeedId);
        var feedName = feed?.CalendarName;
        var providerLabel = ExternalCalendarProviderNames.ToProviderLabel(connection.Provider);

        var date = DateOnly.FromDateTime(entry.StartsAtUtc).ToString("yyyy-MM-dd");
        var time = entry.IsAllDay ? null : entry.StartsAtUtc.ToString("HH:mm");
        var endDate = entry.EndsAtUtc.HasValue
            ? DateOnly.FromDateTime(entry.EndsAtUtc.Value).ToString("yyyy-MM-dd")
            : null;
        var endTime = entry.IsAllDay || !entry.EndsAtUtc.HasValue
            ? null
            : entry.EndsAtUtc.Value.ToString("HH:mm");

        return new GetExternalCalendarEntryResponse(
            entry.Id,
            entry.Title,
            date,
            time,
            endDate,
            endTime,
            entry.IsAllDay,
            entry.Status,
            entry.Location,
            feedName,
            providerLabel,
            entry.OpenInProviderUrl);
    }
}
