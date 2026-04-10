using DomusMind.Application.Abstractions.Integrations.Calendar;
using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Calendar;
using DomusMind.Domain.Calendar.ExternalConnections;
using DomusMind.Domain.Family;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DomusMind.Application.Features.Calendar.SyncExternalCalendarConnection;

public sealed class SyncExternalCalendarConnectionCommandHandler
    : ICommandHandler<SyncExternalCalendarConnectionCommand, SyncExternalCalendarConnectionResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;
    private readonly IExternalCalendarAuthService _authService;
    private readonly IExternalCalendarProviderClient _providerClient;
    private readonly IExternalCalendarSyncLeaseService _leaseService;
    private readonly ILogger<SyncExternalCalendarConnectionCommandHandler> _logger;

    public SyncExternalCalendarConnectionCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService,
        IExternalCalendarAuthService authService,
        IExternalCalendarProviderClient providerClient,
        IExternalCalendarSyncLeaseService leaseService,
        ILogger<SyncExternalCalendarConnectionCommandHandler> logger)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
        _authService = authService;
        _providerClient = providerClient;
        _leaseService = leaseService;
        _logger = logger;
    }

    public async Task<SyncExternalCalendarConnectionResponse> Handle(
        SyncExternalCalendarConnectionCommand command,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, command.FamilyId, cancellationToken);

        if (!canAccess)
            throw new CalendarException(CalendarErrorCode.AccessDenied, "Access to this family is denied.");

        var connectionId = ExternalCalendarConnectionId.From(command.ConnectionId);
        var memberId = MemberId.From(command.MemberId);

        var connection = await _dbContext
            .Set<ExternalCalendarConnection>()
            .Include(c => c.Feeds)
            .FirstOrDefaultAsync(c => c.Id == connectionId && c.MemberId == memberId, cancellationToken);

        if (connection is null)
            throw new CalendarException(CalendarErrorCode.ConnectionNotFound, "Connection not found.");

        if (connection.Status == ExternalCalendarConnectionStatus.Disconnected)
            throw new CalendarException(CalendarErrorCode.ConnectionNotFound, "Connection has been disconnected.");

        // Acquire lease
        var leaseId = await _leaseService.TryAcquireAsync(command.ConnectionId, cancellationToken);
        if (leaseId is null)
            throw new CalendarException(CalendarErrorCode.ConnectionSyncInProgress,
                "Sync is already in progress for this connection.");

        _logger.LogInformation(
            "External calendar sync started. ConnectionId={ConnectionId}, MemberId={MemberId}, Reason={Reason}",
            connection.Id.Value,
            connection.MemberId.Value,
            command.Reason ?? "unspecified");

        Exception? syncException = null;
        try
        {
            return await ExecuteSyncAsync(connection, leaseId.Value, command, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            syncException = ex;

            // Ensure the connection never remains in "syncing" after an exception path.
            var failedAt = DateTime.UtcNow;
            connection.RecordSyncFailure(
                "sync_unhandled_exception",
                "Sync failed due to an unexpected error.",
                false,
                failedAt);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                await _eventLogWriter.WriteAsync(connection.DomainEvents, cancellationToken);
                connection.ClearDomainEvents();
            }
            catch (Exception persistenceEx) when (persistenceEx is not OperationCanceledException)
            {
                _logger.LogError(
                    persistenceEx,
                    "Failed to persist terminal failed sync state for connection {ConnectionId}",
                    connection.Id.Value);
            }

            _logger.LogError(
                ex,
                "External calendar sync failed. ConnectionId={ConnectionId}, MemberId={MemberId}",
                connection.Id.Value,
                connection.MemberId.Value);

            throw;
        }
        finally
        {
            try
            {
                await _leaseService.ReleaseAsync(command.ConnectionId, leaseId.Value, cancellationToken);
            }
            catch (Exception releaseEx) when (releaseEx is not OperationCanceledException)
            {
                _logger.LogError(
                    releaseEx,
                    "Failed to release sync lease for connection {ConnectionId} (LeaseId={LeaseId})",
                    command.ConnectionId,
                    leaseId.Value);

                // If there is no primary sync exception, propagate lease release failure.
                if (syncException is null)
                    throw;
            }
        }
    }

    private async Task<SyncExternalCalendarConnectionResponse> ExecuteSyncAsync(
        ExternalCalendarConnection connection,
        Guid leaseId,
        SyncExternalCalendarConnectionCommand command,
        CancellationToken cancellationToken)
    {
        _ = leaseId;
        var now = DateTime.UtcNow;
        connection.MarkSyncing(now);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var accessToken = await _authService.GetAccessTokenAsync(connection.Id.Value, cancellationToken);
        if (accessToken is null)
        {
            connection.RecordSyncFailure("auth_expired", "Access token could not be refreshed.", true, DateTime.UtcNow);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _eventLogWriter.WriteAsync(connection.DomainEvents, cancellationToken);
            connection.ClearDomainEvents();
            throw new CalendarException(CalendarErrorCode.ConnectionAuthExpired,
                "Provider token expired. Please reconnect the account.");
        }

        var selectedFeeds = connection.GetSelectedFeeds();
        _logger.LogInformation(
            "Sync selected feeds resolved. ConnectionId={ConnectionId}, SelectedFeedCount={SelectedFeedCount}",
            connection.Id.Value,
            selectedFeeds.Count);

        int totalImported = 0, totalUpdated = 0, totalDeleted = 0;
        int syncedFeedCount = 0;
        int failedFeedCount = 0;

        foreach (var feed in selectedFeeds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                _logger.LogInformation(
                    "Feed sync started. ConnectionId={ConnectionId}, ProviderCalendarId={ProviderCalendarId}",
                    connection.Id.Value,
                    feed.ProviderCalendarId);

                var (imported, updated, deleted, syncMode) = await SyncFeedAsync(
                    connection, feed, accessToken, now, cancellationToken);

                totalImported += imported;
                totalUpdated += updated;
                totalDeleted += deleted;
                syncedFeedCount++;

                _logger.LogInformation(
                    "Feed sync completed. ConnectionId={ConnectionId}, ProviderCalendarId={ProviderCalendarId}, SyncMode={SyncMode}, Imported={Imported}, Updated={Updated}, Deleted={Deleted}",
                    connection.Id.Value,
                    feed.ProviderCalendarId,
                    syncMode,
                    imported,
                    updated,
                    deleted);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                failedFeedCount++;
                _logger.LogWarning(
                    ex,
                    "Feed sync failed. ConnectionId={ConnectionId}, ProviderCalendarId={ProviderCalendarId}",
                    connection.Id.Value,
                    feed.ProviderCalendarId);
            }
        }

        var completedAt = DateTime.UtcNow;

        bool hasSelectedFeeds = selectedFeeds.Count > 0;
        bool allFeedsFailed = hasSelectedFeeds && syncedFeedCount == 0;
        bool partialFeedFailure = hasSelectedFeeds && syncedFeedCount > 0 && failedFeedCount > 0;

        if (allFeedsFailed)
        {
            _logger.LogWarning(
                "All {Count} selected feed(s) failed to sync for connection {ConnectionId}.",
                selectedFeeds.Count, connection.Id.Value);
            connection.RecordSyncFailure("sync_all_feeds_failed", "All selected calendar feeds failed to sync.", false, completedAt);
        }
        else if (partialFeedFailure)
        {
            connection.RecordSyncPartialFailure(
                totalImported,
                totalUpdated,
                totalDeleted,
                "sync_partial_failure",
                "One or more selected calendar feeds failed to sync.",
                completedAt);
        }
        else
        {
            connection.RecordSyncSuccess(totalImported, totalUpdated, totalDeleted, completedAt);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _eventLogWriter.WriteAsync(connection.DomainEvents, cancellationToken);
        connection.ClearDomainEvents();

        _logger.LogInformation(
            "External calendar sync completed. ConnectionId={ConnectionId}, MemberId={MemberId}, Outcome={Outcome}, SelectedFeedCount={SelectedFeedCount}, SyncedFeedCount={SyncedFeedCount}, FailedFeedCount={FailedFeedCount}, Imported={Imported}, Updated={Updated}, Deleted={Deleted}",
            connection.Id.Value,
            connection.MemberId.Value,
            ExternalCalendarConnectionStatusNames.ToStatusString(connection.Status),
            selectedFeeds.Count,
            syncedFeedCount,
            failedFeedCount,
            totalImported,
            totalUpdated,
            totalDeleted);

        return new SyncExternalCalendarConnectionResponse(
            connection.Id.Value,
            selectedFeeds.Count,
            syncedFeedCount,
            totalImported,
            totalUpdated,
            totalDeleted,
            ExternalCalendarConnectionStatusNames.ToStatusString(connection.Status),
            connection.LastSuccessfulSyncUtc);
    }

    private async Task<(int Imported, int Updated, int Deleted, string SyncMode)> SyncFeedAsync(
        ExternalCalendarConnection connection,
        ExternalCalendarFeed feed,
        string accessToken,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var windowStart = connection.Horizon.ComputeWindowStart(now);
        var windowEnd = connection.Horizon.ComputeWindowEnd(now);
        feed.RecordSyncWindow(windowStart, windowEnd, now);

        int imported = 0, updated = 0, deleted = 0;
        bool useInitialSync = feed.LastDeltaToken is null;
        string syncMode = useInitialSync ? "initial" : "delta";

        _logger.LogInformation(
            "Feed sync mode resolved. ConnectionId={ConnectionId}, ProviderCalendarId={ProviderCalendarId}, SyncMode={SyncMode}",
            connection.Id.Value,
            feed.ProviderCalendarId,
            syncMode);

        IAsyncEnumerable<ExternalCalendarProviderDeltaPage> pages;
        if (useInitialSync)
        {
            pages = _providerClient.GetInitialEventsAsync(
                accessToken, feed.ProviderCalendarId, windowStart, windowEnd, cancellationToken);
        }
        else
        {
            pages = _providerClient.GetDeltaEventsAsync(
                accessToken, feed.ProviderCalendarId, feed.LastDeltaToken!, cancellationToken);
        }

        string? lastDeltaToken = null;
        await foreach (var page in pages.WithCancellation(cancellationToken))
        {
            foreach (var evt in page.Events)
            {
                var (i, u, d) = await ApplyEventAsync(connection, feed, evt, now, cancellationToken);
                imported += i;
                updated += u;
                deleted += d;
            }

            if (page.NextDeltaToken is not null)
                lastDeltaToken = page.NextDeltaToken;
        }

        if (lastDeltaToken is not null)
            feed.RecordDeltaToken(lastDeltaToken, now);

        feed.RecordSuccessfulSync(now);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return (imported, updated, deleted, syncMode);
    }

    private async Task<(int Imported, int Updated, int Deleted)> ApplyEventAsync(
        ExternalCalendarConnection connection,
        ExternalCalendarFeed feed,
        ExternalCalendarProviderEvent evt,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var existing = await _dbContext
            .Set<ExternalCalendarEntry>()
            .FirstOrDefaultAsync(e => e.FeedId == feed.Id && e.ExternalEventId == evt.ExternalEventId, cancellationToken);

        if (evt.IsDeleted)
        {
            if (existing is not null && !existing.IsDeleted)
            {
                existing.IsDeleted = true;
                existing.UpdatedAtUtc = now;
                return (0, 0, 1);
            }
            return (0, 0, 0);
        }

        if (existing is null)
        {
            var entry = new ExternalCalendarEntry
            {
                Id = Guid.NewGuid(),
                ConnectionId = connection.Id.Value,
                FeedId = feed.Id,
                Provider = ExternalCalendarProviderNames.ToProviderString(connection.Provider),
                ExternalEventId = evt.ExternalEventId,
                ICalUId = evt.ICalUId,
                SeriesMasterId = evt.SeriesMasterId,
                Title = evt.Title,
                StartsAtUtc = evt.StartsAtUtc,
                EndsAtUtc = evt.EndsAtUtc,
                IsAllDay = evt.IsAllDay,
                Location = evt.Location,
                ParticipantSummaryJson = evt.ParticipantSummaryJson,
                Status = evt.Status,
                OpenInProviderUrl = evt.OpenInProviderUrl,
                ProviderModifiedAtUtc = evt.ProviderModifiedAtUtc,
                IsDeleted = false,
                LastSeenAtUtc = now,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
            _dbContext.Set<ExternalCalendarEntry>().Add(entry);
            return (1, 0, 0);
        }

        // Update existing
        existing.Title = evt.Title;
        existing.StartsAtUtc = evt.StartsAtUtc;
        existing.EndsAtUtc = evt.EndsAtUtc;
        existing.IsAllDay = evt.IsAllDay;
        existing.Location = evt.Location;
        existing.ParticipantSummaryJson = evt.ParticipantSummaryJson;
        existing.Status = evt.Status;
        existing.OpenInProviderUrl = evt.OpenInProviderUrl;
        existing.ProviderModifiedAtUtc = evt.ProviderModifiedAtUtc;
        existing.IsDeleted = false;
        existing.LastSeenAtUtc = now;
        existing.UpdatedAtUtc = now;
        return (0, 1, 0);
    }
}
