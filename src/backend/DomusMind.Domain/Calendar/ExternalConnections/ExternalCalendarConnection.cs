using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Calendar.ExternalConnections.Events;
using DomusMind.Domain.Family;

namespace DomusMind.Domain.Calendar.ExternalConnections;

/// <summary>
/// Aggregate root for an external calendar provider connection owned by a household member.
/// Phase 1: Microsoft Outlook via delegated auth (Microsoft Graph).
/// </summary>
public sealed class ExternalCalendarConnection : AggregateRoot<ExternalCalendarConnectionId>
{
    private readonly List<ExternalCalendarFeed> _feeds = [];

    public FamilyId FamilyId { get; private set; }
    public MemberId MemberId { get; private set; }
    public ExternalCalendarProvider Provider { get; private set; }
    public string ProviderAccountId { get; private set; }
    public string AccountEmail { get; private set; }
    public string? AccountDisplayLabel { get; private set; }
    public string? TenantId { get; private set; }
    public SyncHorizon Horizon { get; private set; }
    public bool ScheduledRefreshEnabled { get; private set; }
    public int ScheduledRefreshIntervalMinutes { get; private set; }
    public ExternalCalendarConnectionStatus Status { get; private set; }
    public DateTime? LastSuccessfulSyncUtc { get; private set; }
    public DateTime? LastSyncAttemptUtc { get; private set; }
    public DateTime? LastSyncFailureUtc { get; private set; }
    public string? LastErrorCode { get; private set; }
    public string? LastErrorMessage { get; private set; }
    public DateTime? NextScheduledSyncUtc { get; private set; }
    public Guid? SyncLeaseId { get; private set; }
    public DateTime? SyncLeaseExpiresAtUtc { get; private set; }
    public int Version { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<ExternalCalendarFeed> Feeds => _feeds.AsReadOnly();

    private ExternalCalendarConnection(
        ExternalCalendarConnectionId id,
        FamilyId familyId,
        MemberId memberId,
        ExternalCalendarProvider provider,
        string providerAccountId,
        string accountEmail,
        string? accountDisplayLabel,
        string? tenantId,
        SyncHorizon horizon,
        DateTime createdAtUtc)
        : base(id)
    {
        FamilyId = familyId;
        MemberId = memberId;
        Provider = provider;
        ProviderAccountId = providerAccountId;
        AccountEmail = accountEmail;
        AccountDisplayLabel = accountDisplayLabel;
        TenantId = tenantId;
        Horizon = horizon;
        ScheduledRefreshEnabled = true;
        ScheduledRefreshIntervalMinutes = 60;
        Status = ExternalCalendarConnectionStatus.PendingInitialSync;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    // Required by EF Core for materialization (owned SyncHorizon prevents binding constructor params)
    private ExternalCalendarConnection() : base(ExternalCalendarConnectionId.New())
    {
        ProviderAccountId = string.Empty;
        AccountEmail = string.Empty;
        Horizon = SyncHorizon.Default();
    }

    public static ExternalCalendarConnection Connect(
        ExternalCalendarConnectionId id,
        FamilyId familyId,
        MemberId memberId,
        ExternalCalendarProvider provider,
        string providerAccountId,
        string accountEmail,
        string? accountDisplayLabel,
        string? tenantId,
        DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(providerAccountId))
            throw new InvalidOperationException("Provider account ID is required.");

        if (string.IsNullOrWhiteSpace(accountEmail))
            throw new InvalidOperationException("Account email is required.");

        var connection = new ExternalCalendarConnection(
            id, familyId, memberId, provider, providerAccountId, accountEmail,
            accountDisplayLabel, tenantId, SyncHorizon.Default(), createdAtUtc);

        connection.RaiseDomainEvent(new ExternalCalendarConnectionConnected(
            Guid.NewGuid(),
            id.Value,
            memberId.Value,
            ExternalCalendarProviderNames.ToProviderString(provider),
            accountEmail,
            createdAtUtc));

        return connection;
    }

    /// <summary>
    /// Adds a discovered provider calendar as an unselected feed.
    /// Selected state is set separately via Configure.
    /// </summary>
    public void AddOrUpdateFeed(string providerCalendarId, string calendarName, bool isDefault, bool isSelected, DateTime nowUtc)
    {
        var existing = _feeds.FirstOrDefault(f => f.ProviderCalendarId == providerCalendarId);
        if (existing is not null)
        {
            existing.UpdateSelection(calendarName, isSelected, nowUtc);
        }
        else
        {
            _feeds.Add(ExternalCalendarFeed.Create(Id, providerCalendarId, calendarName, isDefault, isSelected, nowUtc));
        }
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>
    /// Applies horizon and refresh-schedule configuration.
    /// Feed graph mutations (add / update / remove) are the handler's responsibility
    /// so that EF can track changes against the already-loaded entities.
    /// The selectedFeedCount is supplied by the caller so the domain event carries
    /// the correct count even when new feeds were added outside _feeds.
    /// </summary>
    public void Configure(
        int forwardHorizonDays,
        bool scheduledRefreshEnabled,
        int scheduledRefreshIntervalMinutes,
        int selectedFeedCount,
        DateTime nowUtc)
    {
        var newHorizon = SyncHorizon.Create(forwardHorizonDays);
        bool horizonChanged = newHorizon.ForwardHorizonDays != Horizon.ForwardHorizonDays;

        Horizon = newHorizon;
        ScheduledRefreshEnabled = scheduledRefreshEnabled;
        ScheduledRefreshIntervalMinutes = scheduledRefreshIntervalMinutes;
        UpdatedAtUtc = nowUtc;

        if (horizonChanged)
        {
            // Invalidate delta state for all feeds loaded into this aggregate instance.
            // Newly-added feeds (added via DbSet in the handler) start with null delta
            // state, so they do not need invalidation.
            foreach (var feed in _feeds)
                feed.InvalidateDeltaState(nowUtc);
        }

        RaiseDomainEvent(new ExternalCalendarConnectionConfigured(
            Guid.NewGuid(),
            Id.Value,
            MemberId.Value,
            selectedFeedCount,
            forwardHorizonDays,
            nowUtc));
    }

    public void MarkSyncing(DateTime nowUtc)
    {
        Status = ExternalCalendarConnectionStatus.Syncing;
        LastSyncAttemptUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    public void RecordSyncSuccess(int imported, int updated, int deleted, DateTime nowUtc)
    {
        Status = ExternalCalendarConnectionStatus.Healthy;
        LastSuccessfulSyncUtc = nowUtc;
        LastSyncFailureUtc = null;
        LastErrorCode = null;
        LastErrorMessage = null;
        NextScheduledSyncUtc = nowUtc.AddMinutes(ScheduledRefreshIntervalMinutes);
        UpdatedAtUtc = nowUtc;
        Version++;
        RaiseDomainEvent(new ExternalCalendarConnectionSyncCompleted(
            Guid.NewGuid(), Id.Value, MemberId.Value, imported, updated, deleted, nowUtc));
    }

    public void RecordSyncFailure(string errorCode, string errorMessage, bool isAuthFailure, DateTime nowUtc)
    {
        Status = isAuthFailure
            ? ExternalCalendarConnectionStatus.AuthExpired
            : ExternalCalendarConnectionStatus.Failed;
        LastSyncFailureUtc = nowUtc;
        LastErrorCode = errorCode;
        LastErrorMessage = errorMessage;
        NextScheduledSyncUtc = nowUtc.AddMinutes(ScheduledRefreshIntervalMinutes);
        UpdatedAtUtc = nowUtc;
        RaiseDomainEvent(new ExternalCalendarConnectionSyncFailed(
            Guid.NewGuid(), Id.Value, MemberId.Value, errorCode, errorMessage, nowUtc));
    }

    public void RecordSyncPartialFailure(
        int imported,
        int updated,
        int deleted,
        string errorCode,
        string errorMessage,
        DateTime nowUtc)
    {
        Status = ExternalCalendarConnectionStatus.PartialFailure;
        LastSuccessfulSyncUtc = nowUtc;
        LastSyncFailureUtc = nowUtc;
        LastErrorCode = errorCode;
        LastErrorMessage = errorMessage;
        NextScheduledSyncUtc = nowUtc.AddMinutes(ScheduledRefreshIntervalMinutes);
        UpdatedAtUtc = nowUtc;
        Version++;

        RaiseDomainEvent(new ExternalCalendarConnectionSyncCompleted(
            Guid.NewGuid(), Id.Value, MemberId.Value, imported, updated, deleted, nowUtc));
        RaiseDomainEvent(new ExternalCalendarConnectionSyncFailed(
            Guid.NewGuid(), Id.Value, MemberId.Value, errorCode, errorMessage, nowUtc));
    }

    public void MarkRehydrating(DateTime nowUtc)
    {
        Status = ExternalCalendarConnectionStatus.Rehydrating;
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Acquires a sync lease. Returns false if already leased.</summary>
    public bool TryAcquireLease(Guid leaseId, TimeSpan leaseDuration, DateTime nowUtc)
    {
        if (SyncLeaseId.HasValue && SyncLeaseExpiresAtUtc > nowUtc)
            return false;

        SyncLeaseId = leaseId;
        SyncLeaseExpiresAtUtc = nowUtc.Add(leaseDuration);
        UpdatedAtUtc = nowUtc;
        return true;
    }

    public void ReleaseLease(Guid leaseId, DateTime nowUtc)
    {
        if (SyncLeaseId == leaseId)
        {
            SyncLeaseId = null;
            SyncLeaseExpiresAtUtc = null;
            UpdatedAtUtc = nowUtc;
        }
    }

    public ExternalCalendarFeed? GetFeed(string providerCalendarId)
        => _feeds.FirstOrDefault(f => f.ProviderCalendarId == providerCalendarId);

    public IReadOnlyCollection<ExternalCalendarFeed> GetSelectedFeeds()
        => _feeds.Where(f => f.IsSelected).ToList().AsReadOnly();

    public void Disconnect(DateTime nowUtc)
    {
        Status = ExternalCalendarConnectionStatus.Disconnected;
        ScheduledRefreshEnabled = false;
        SyncLeaseId = null;
        SyncLeaseExpiresAtUtc = null;
        UpdatedAtUtc = nowUtc;
        RaiseDomainEvent(new ExternalCalendarConnectionDisconnected(
            Guid.NewGuid(),
            Id.Value,
            MemberId.Value,
            ExternalCalendarProviderNames.ToProviderString(Provider),
            nowUtc));
    }
}
