using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Features.Calendar.SyncExternalCalendarConnection;
using DomusMind.Domain.Calendar.ExternalConnections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DomusMind.Infrastructure.BackgroundJobs.Calendar;

/// <summary>
/// Hosted background service that periodically syncs all eligible external calendar connections.
/// Wakes every WorkerCycleSeconds, queries for connections where
/// ScheduledRefreshEnabled = true and NextScheduledSyncUtc &lt;= now.
/// </summary>
public sealed class ExternalCalendarRefreshWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ExternalCalendarRefreshOptions _options;
    private readonly ILogger<ExternalCalendarRefreshWorker> _logger;
    private readonly Random _rng = new();

    public ExternalCalendarRefreshWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<ExternalCalendarRefreshOptions> options,
        ILogger<ExternalCalendarRefreshWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExternalCalendarRefreshWorker started");

        // Brief startup delay so other services initialize first
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueConnectionsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "ExternalCalendarRefreshWorker encountered an error in the main loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.WorkerCycleSeconds), stoppingToken);
        }

        _logger.LogInformation("ExternalCalendarRefreshWorker stopped");
    }

    private async Task ProcessDueConnectionsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<DomusMind.Infrastructure.Persistence.DomusMindDbContext>();

        var now = DateTime.UtcNow;

        var dueConnections = await dbContext
            .Set<ExternalCalendarConnection>()
            .AsNoTracking()
            .Where(c => c.ScheduledRefreshEnabled &&
                        c.Status != ExternalCalendarConnectionStatus.Disconnected &&
                        c.Status != ExternalCalendarConnectionStatus.AuthExpired &&
                        (c.NextScheduledSyncUtc == null || c.NextScheduledSyncUtc <= now) &&
                        (c.SyncLeaseId == null || c.SyncLeaseExpiresAtUtc <= now))
            .Take(_options.BatchSize)
            .Select(c => new { c.Id.Value, c.FamilyId, c.MemberId })
            .ToListAsync(cancellationToken);

        if (dueConnections.Count == 0)
            return;

        _logger.LogInformation("ExternalCalendarRefreshWorker processing {Count} due connections", dueConnections.Count);

        var dispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

        foreach (var conn in dueConnections)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Small random inter-connection delay (0–5 s) to stagger DB and network I/O
            // within a batch and avoid thundering-herd when multiple connections are due.
            // JitterMaxSeconds governs the full connection-schedule jitter; this is
            // intentionally capped at 5 s to keep batch processing responsive.
            var batchJitterMs = _rng.Next(0, 5_001);
            if (batchJitterMs > 0)
                await Task.Delay(batchJitterMs, cancellationToken);

            try
            {
                await dispatcher.Dispatch(
                    new SyncExternalCalendarConnectionCommand(
                        conn.FamilyId.Value,
                        conn.MemberId.Value,
                        conn.Value,
                        "scheduled",
                        Guid.Empty),
                    cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Background sync failed for connection {ConnectionId}", conn.Value);
            }
        }
    }
}
