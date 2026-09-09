using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Calendar;
using DomusMind.Domain.Calendar.ExternalConnections;
using DomusMind.Domain.Family;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Calendar.GetMemberExternalCalendarConnections;

public sealed class GetMemberExternalCalendarConnectionsQueryHandler
    : IQueryHandler<GetMemberExternalCalendarConnectionsQuery, IReadOnlyCollection<ExternalCalendarConnectionSummaryResponse>>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public GetMemberExternalCalendarConnectionsQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<IReadOnlyCollection<ExternalCalendarConnectionSummaryResponse>> Handle(
        GetMemberExternalCalendarConnectionsQuery query,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, query.FamilyId, cancellationToken);

        if (!canAccess)
            throw new CalendarException(CalendarErrorCode.AccessDenied, "Access to this family is denied.");

        var memberId = MemberId.From(query.MemberId);

        var connections = await _dbContext
            .Set<ExternalCalendarConnection>()
            .AsNoTracking()
            .Include(c => c.Feeds)
            .Where(c => c.MemberId == memberId && c.Status != ExternalCalendarConnectionStatus.Disconnected)
            .OrderBy(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var connectionIds = connections.Select(c => c.Id.Value).ToList();
        var importedEntryCounts = connectionIds.Count == 0
            ? new Dictionary<Guid, int>()
            : await _dbContext
                .Set<ExternalCalendarEntry>()
                .AsNoTracking()
                .Where(e => connectionIds.Contains(e.ConnectionId) && !e.IsDeleted)
                .GroupBy(e => e.ConnectionId)
                .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);

        return connections.Select(c => new ExternalCalendarConnectionSummaryResponse(
            c.Id.Value,
            c.MemberId.Value,
            ExternalCalendarProviderNames.ToProviderString(c.Provider),
            ExternalCalendarProviderNames.ToProviderLabel(c.Provider),
            c.AccountEmail,
            c.AccountDisplayLabel,
            c.GetSelectedFeeds().Count,
            c.Horizon.ForwardHorizonDays,
            c.ScheduledRefreshEnabled,
            c.ScheduledRefreshIntervalMinutes,
            c.LastSuccessfulSyncUtc,
            c.LastSyncAttemptUtc,
            c.LastSyncFailureUtc,
            ExternalCalendarConnectionStatusNames.ToStatusString(c.Status),
            c.Status == ExternalCalendarConnectionStatus.Syncing,
            importedEntryCounts.GetValueOrDefault(c.Id.Value, 0),
            c.LastErrorCode,
            c.LastErrorMessage)).ToList().AsReadOnly();
    }
}
