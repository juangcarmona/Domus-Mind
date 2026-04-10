using DomusMind.Application.Abstractions.Integrations.Calendar;
using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Calendar;
using DomusMind.Domain.Calendar.ExternalConnections;
using DomusMind.Domain.Family;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DomusMind.Application.Features.Calendar.GetExternalCalendarConnectionDetail;

public sealed class GetExternalCalendarConnectionDetailQueryHandler
    : IQueryHandler<GetExternalCalendarConnectionDetailQuery, ExternalCalendarConnectionDetailResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;
    private readonly IExternalCalendarAuthService _authService;
    private readonly IExternalCalendarProviderClient _providerClient;
    private readonly ILogger<GetExternalCalendarConnectionDetailQueryHandler> _logger;

    public GetExternalCalendarConnectionDetailQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService,
        IExternalCalendarAuthService authService,
        IExternalCalendarProviderClient providerClient,
        ILogger<GetExternalCalendarConnectionDetailQueryHandler> logger)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
        _authService = authService;
        _providerClient = providerClient;
        _logger = logger;
    }

    public async Task<ExternalCalendarConnectionDetailResponse> Handle(
        GetExternalCalendarConnectionDetailQuery query,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, query.FamilyId, cancellationToken);

        if (!canAccess)
            throw new CalendarException(CalendarErrorCode.AccessDenied, "Access to this family is denied.");

        var connectionId = ExternalCalendarConnectionId.From(query.ConnectionId);
        var memberId = MemberId.From(query.MemberId);

        var connection = await _dbContext
            .Set<ExternalCalendarConnection>()
            .AsNoTracking()
            .Include(c => c.Feeds)
            .FirstOrDefaultAsync(c => c.Id == connectionId && c.MemberId == memberId, cancellationToken);

        if (connection is null || connection.Status == ExternalCalendarConnectionStatus.Disconnected)
            throw new CalendarException(CalendarErrorCode.ConnectionNotFound, "Connection not found.");

        var feedResponses = connection.Feeds.Select(f => new ExternalCalendarFeedResponse(
            f.ProviderCalendarId, f.CalendarName, f.IsSelected,
            f.LastSuccessfulSyncUtc, f.WindowStartUtc, f.WindowEndUtc)).ToList();

        var importedEntryCount = await _dbContext
            .Set<ExternalCalendarEntry>()
            .AsNoTracking()
            .CountAsync(e => e.ConnectionId == connection.Id.Value && !e.IsDeleted, cancellationToken);

        // Attempt to refresh available calendars list
        IReadOnlyCollection<AvailableExternalCalendarResponse> availableCalendars = [];
        var accessToken = await _authService.GetAccessTokenAsync(connection.Id.Value, cancellationToken);
        if (accessToken is not null)
        {
            try
            {
                var providerCalendars = await _providerClient.GetCalendarsAsync(accessToken, cancellationToken);
                var selectedIds = connection.Feeds.Where(f => f.IsSelected).Select(f => f.ProviderCalendarId).ToHashSet();
                availableCalendars = providerCalendars.Select(c => new AvailableExternalCalendarResponse(
                    c.CalendarId, c.CalendarName, c.IsDefault, selectedIds.Contains(c.CalendarId))).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to refresh available calendars for connection {ConnectionId}", query.ConnectionId);
            }
        }

        return new ExternalCalendarConnectionDetailResponse(
            connection.Id.Value,
            connection.MemberId.Value,
            ExternalCalendarProviderNames.ToProviderString(connection.Provider),
            ExternalCalendarProviderNames.ToProviderLabel(connection.Provider),
            connection.AccountEmail,
            connection.AccountDisplayLabel,
            connection.TenantId,
            connection.Horizon.ForwardHorizonDays,
            connection.ScheduledRefreshEnabled,
            connection.ScheduledRefreshIntervalMinutes,
            connection.LastSuccessfulSyncUtc,
            connection.LastSyncAttemptUtc,
            connection.LastSyncFailureUtc,
            ExternalCalendarConnectionStatusNames.ToStatusString(connection.Status),
            connection.Status == ExternalCalendarConnectionStatus.Syncing,
            importedEntryCount,
            feedResponses,
            availableCalendars,
            connection.LastErrorCode,
            connection.LastErrorMessage);
    }
}
