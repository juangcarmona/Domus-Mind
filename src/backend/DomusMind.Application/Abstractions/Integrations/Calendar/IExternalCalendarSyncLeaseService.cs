namespace DomusMind.Application.Abstractions.Integrations.Calendar;

/// <summary>
/// Acquires and releases database-backed sync leases to prevent concurrent sync per connection.
/// </summary>
public interface IExternalCalendarSyncLeaseService
{
    /// <summary>
    /// Attempts to acquire a lease on the connection row.
    /// Returns the lease ID if acquired, null if already leased.
    /// </summary>
    Task<Guid?> TryAcquireAsync(Guid connectionId, CancellationToken cancellationToken = default);

    /// <summary>Releases a previously acquired lease.</summary>
    Task ReleaseAsync(Guid connectionId, Guid leaseId, CancellationToken cancellationToken = default);
}
