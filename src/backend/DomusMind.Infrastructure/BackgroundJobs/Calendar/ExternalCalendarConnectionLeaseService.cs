using DomusMind.Application.Abstractions.Integrations.Calendar;
using DomusMind.Domain.Calendar.ExternalConnections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DomusMind.Infrastructure.BackgroundJobs.Calendar;

/// <summary>
/// Database-backed sync lease to prevent concurrent sync on the same connection.
/// Safe when multiple app instances run simultaneously.
/// </summary>
public sealed class ExternalCalendarConnectionLeaseService : IExternalCalendarSyncLeaseService
{
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(10);

    private readonly DomusMind.Infrastructure.Persistence.DomusMindDbContext _dbContext;
    private readonly ILogger<ExternalCalendarConnectionLeaseService> _logger;

    public ExternalCalendarConnectionLeaseService(
        DomusMind.Infrastructure.Persistence.DomusMindDbContext dbContext,
        ILogger<ExternalCalendarConnectionLeaseService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Guid?> TryAcquireAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        var id = ExternalCalendarConnectionId.From(connectionId);
        var connection = await _dbContext
            .Set<ExternalCalendarConnection>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (connection is null)
            return null;

        var now = DateTime.UtcNow;
        var leaseId = Guid.NewGuid();

        if (!connection.TryAcquireLease(leaseId, LeaseDuration, now))
        {
            _logger.LogDebug("Sync lease already held for connection {ConnectionId}", connectionId);
            return null;
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return leaseId;
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another instance acquired the lease concurrently
            return null;
        }
    }

    public async Task ReleaseAsync(Guid connectionId, Guid leaseId, CancellationToken cancellationToken = default)
    {
        var id = ExternalCalendarConnectionId.From(connectionId);
        var connection = await _dbContext
            .Set<ExternalCalendarConnection>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (connection is null)
            return;

        connection.ReleaseLease(leaseId, DateTime.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
