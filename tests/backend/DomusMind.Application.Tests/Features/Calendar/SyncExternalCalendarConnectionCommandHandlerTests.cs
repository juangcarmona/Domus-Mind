using DomusMind.Application.Abstractions.Integrations.Calendar;
using DomusMind.Application.Features.Calendar.SyncExternalCalendarConnection;
using DomusMind.Domain.Calendar.ExternalConnections;
using DomusMind.Domain.Family;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DomusMind.Application.Tests.Features.Calendar;

public sealed class SyncExternalCalendarConnectionCommandHandlerTests
{
    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static SyncExternalCalendarConnectionCommandHandler BuildHandler(
        DomusMindDbContext db,
        IExternalCalendarAuthService? authService = null,
        IExternalCalendarProviderClient? providerClient = null,
        IExternalCalendarSyncLeaseService? leaseService = null,
        StubCalendarAuthorizationService? authz = null) =>
        new(db,
            new EventLogWriter(db),
            authz ?? new StubCalendarAuthorizationService(),
            authService ?? new StubSyncAuthService("access-token"),
            providerClient ?? new StubSyncProviderClient(),
            leaseService ?? new StubSyncLeaseService(),
            NullLogger<SyncExternalCalendarConnectionCommandHandler>.Instance);

    private static async Task<(DomusMindDbContext Db, ExternalCalendarConnection Connection, Guid FamilyId, Guid MemberId)>
        SeedConnectionWithFeedsAsync(
            DomusMindDbContext? db = null,
            params (string CalendarId, string Name, bool IsDefault, bool IsSelected)[] feeds)
    {
        db ??= CreateDb();
        var familyId = FamilyId.New();
        var memberId = MemberId.New();
        var connectionId = ExternalCalendarConnectionId.New();
        var now = DateTime.UtcNow;

        var connection = ExternalCalendarConnection.Connect(
            connectionId, familyId, memberId,
            ExternalCalendarProvider.Microsoft,
            "prov-account-id", "user@outlook.com", null, "common", now);

        db.Set<ExternalCalendarConnection>().Add(connection);
        db.SetExternalCalendarConnectionAuthMaterial(connection, "enc-token", "Calendars.Read");
        await db.SaveChangesAsync();
        connection.ClearDomainEvents();

        foreach (var (calendarId, name, isDefault, isSelected) in feeds)
        {
            var feed = ExternalCalendarFeed.Create(connectionId, calendarId, name, isDefault, isSelected, now);
            db.Set<ExternalCalendarFeed>().Add(feed);
        }

        if (feeds.Length > 0)
            await db.SaveChangesAsync();

        db.ChangeTracker.Clear();

        var loaded = await db.Set<ExternalCalendarConnection>()
            .Include(c => c.Feeds)
            .FirstAsync(c => c.Id == connectionId);

        return (db, loaded, familyId.Value, memberId.Value);
    }

    private static SyncExternalCalendarConnectionCommand BuildCommand(
        Guid familyId, Guid memberId, Guid connectionId) =>
        new(familyId, memberId, connectionId, "manual", Guid.NewGuid());

    // -----------------------------------------------------------------------
    // Tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_WhenAllFeedsFail_DoesNotMarkConnectionSuccessful()
    {
        var (db, connection, familyId, memberId) = await SeedConnectionWithFeedsAsync(
            feeds:
            [
                ("cal-1", "Work", true, true),
                ("cal-2", "Personal", false, true)
            ]);

        // Provider throws on every feed sync attempt.
        var providerClient = new StubSyncProviderClient(alwaysThrow: true);
        var handler = BuildHandler(db, providerClient: providerClient);

        var result = await handler.Handle(
            BuildCommand(familyId, memberId, connection.Id.Value),
            CancellationToken.None);

        result.SyncedFeedCount.Should().Be(0);
        result.Status.Should().Be("failed");

        db.ChangeTracker.Clear();
        var updated = await db.Set<ExternalCalendarConnection>()
            .FirstAsync(c => c.Id == connection.Id);

        updated.Status.Should().Be(ExternalCalendarConnectionStatus.Failed);
        updated.LastSuccessfulSyncUtc.Should().BeNull("no feeds synced successfully");
        updated.LastErrorCode.Should().Be("sync_all_feeds_failed");
        updated.LastSyncAttemptUtc.Should().NotBeNull();
        updated.LastSyncFailureUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenOneOfTwoFeedsFails_MarksConnectionPartialFailure()
    {
        var (db, connection, familyId, memberId) = await SeedConnectionWithFeedsAsync(
            feeds:
            [
                ("cal-1", "Work", true, true),
                ("cal-2", "Personal", false, true)
            ]);

        // Provider throws only for cal-2.
        var providerClient = new StubSyncProviderClient(throwForCalendarId: "cal-2");
        var handler = BuildHandler(db, providerClient: providerClient);

        var result = await handler.Handle(
            BuildCommand(familyId, memberId, connection.Id.Value),
            CancellationToken.None);

        result.SyncedFeedCount.Should().Be(1);
        result.Status.Should().Be("partial_failure");

        db.ChangeTracker.Clear();
        var updated = await db.Set<ExternalCalendarConnection>()
            .FirstAsync(c => c.Id == connection.Id);

        updated.Status.Should().Be(ExternalCalendarConnectionStatus.PartialFailure);
        updated.LastSuccessfulSyncUtc.Should().NotBeNull("at least one feed synced");
        updated.LastSyncFailureUtc.Should().NotBeNull("one or more feeds failed");
        updated.LastErrorCode.Should().Be("sync_partial_failure");
    }

    [Fact]
    public async Task Handle_WhenNoSelectedFeeds_MarksConnectionSuccessful()
    {
        // No selected feeds → 0 selected, 0 failed → should still be healthy
        // (nothing to sync is not a failure).
        var (db, connection, familyId, memberId) = await SeedConnectionWithFeedsAsync(
            feeds: [("cal-1", "Work", true, false)]);  // unselected

        var handler = BuildHandler(db);

        var result = await handler.Handle(
            BuildCommand(familyId, memberId, connection.Id.Value),
            CancellationToken.None);

        result.SyncedFeedCount.Should().Be(0);
        result.Status.Should().Be("success");
    }

    [Fact]
    public async Task Handle_WhenSelectedFeedSyncSucceeds_StatusBecomesSuccess()
    {
        var (db, connection, familyId, memberId) = await SeedConnectionWithFeedsAsync(
            feeds: [("cal-1", "Work", true, true)]);

        var handler = BuildHandler(db);

        var result = await handler.Handle(
            BuildCommand(familyId, memberId, connection.Id.Value),
            CancellationToken.None);

        result.SyncedFeedCount.Should().Be(1);
        result.Status.Should().Be("success");

        db.ChangeTracker.Clear();
        var updated = await db.Set<ExternalCalendarConnection>()
            .FirstAsync(c => c.Id == connection.Id);

        updated.Status.Should().Be(ExternalCalendarConnectionStatus.Healthy);
        updated.LastSuccessfulSyncUtc.Should().NotBeNull();
        updated.LastSyncFailureUtc.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenUnexpectedExceptionOccurs_SyncingStateDoesNotPersist()
    {
        var (db, connection, familyId, memberId) = await SeedConnectionWithFeedsAsync(
            feeds: [("cal-1", "Work", true, true)]);

        var throwingAuthService = new ThrowingSyncAuthService();
        var handler = BuildHandler(db, authService: throwingAuthService);

        var act = () => handler.Handle(
            BuildCommand(familyId, memberId, connection.Id.Value),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();

        db.ChangeTracker.Clear();
        var updated = await db.Set<ExternalCalendarConnection>()
            .FirstAsync(c => c.Id == connection.Id);

        updated.Status.Should().Be(ExternalCalendarConnectionStatus.Failed);
        updated.Status.Should().NotBe(ExternalCalendarConnectionStatus.Syncing);
        updated.LastSyncAttemptUtc.Should().NotBeNull();
        updated.LastSyncFailureUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenUnexpectedExceptionOccurs_AlwaysReleasesLease()
    {
        var (db, connection, familyId, memberId) = await SeedConnectionWithFeedsAsync(
            feeds: [("cal-1", "Work", true, true)]);

        var lease = new TrackingSyncLeaseService();
        var handler = BuildHandler(
            db,
            authService: new ThrowingSyncAuthService(),
            leaseService: lease);

        var act = () => handler.Handle(
            BuildCommand(familyId, memberId, connection.Id.Value),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        lease.Released.Should().BeTrue();
        lease.ReleasedConnectionId.Should().Be(connection.Id.Value);
    }

    // -----------------------------------------------------------------------
    // Stubs
    // -----------------------------------------------------------------------

    private sealed class StubSyncAuthService : IExternalCalendarAuthService
    {
        private readonly string? _token;

        public StubSyncAuthService(string? token) => _token = token;

        public Task<ExternalCalendarProviderAccount> ExchangeAuthorizationCodeAsync(
            string authorizationCode, string redirectUri, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<string?> GetAccessTokenAsync(Guid connectionId, CancellationToken cancellationToken = default)
            => Task.FromResult(_token);

        public Task RevokeAsync(Guid connectionId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class ThrowingSyncAuthService : IExternalCalendarAuthService
    {
        public Task<ExternalCalendarProviderAccount> ExchangeAuthorizationCodeAsync(
            string authorizationCode,
            string redirectUri,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<string?> GetAccessTokenAsync(Guid connectionId, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Simulated token acquisition failure.");

        public Task RevokeAsync(Guid connectionId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class StubSyncProviderClient : IExternalCalendarProviderClient
    {
        private readonly bool _alwaysThrow;
        private readonly string? _throwForCalendarId;

        public StubSyncProviderClient(bool alwaysThrow = false, string? throwForCalendarId = null)
        {
            _alwaysThrow = alwaysThrow;
            _throwForCalendarId = throwForCalendarId;
        }

        public Task<IReadOnlyCollection<ExternalCalendarProviderCalendar>> GetCalendarsAsync(
            string accessToken, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<ExternalCalendarProviderCalendar>>([]);

        public async IAsyncEnumerable<ExternalCalendarProviderDeltaPage> GetInitialEventsAsync(
            string accessToken, string calendarId, DateTime windowStartUtc, DateTime windowEndUtc,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (_alwaysThrow || calendarId == _throwForCalendarId)
                throw new InvalidOperationException($"Simulated feed sync failure for {calendarId}.");

            yield return new ExternalCalendarProviderDeltaPage([], "delta-token", true);
            await Task.CompletedTask;
        }

        public async IAsyncEnumerable<ExternalCalendarProviderDeltaPage> GetDeltaEventsAsync(
            string accessToken, string calendarId, string deltaToken,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (_alwaysThrow || calendarId == _throwForCalendarId)
                throw new InvalidOperationException($"Simulated feed sync failure for {calendarId}.");

            yield return new ExternalCalendarProviderDeltaPage([], deltaToken, true);
            await Task.CompletedTask;
        }
    }

    private sealed class StubSyncLeaseService : IExternalCalendarSyncLeaseService
    {
        public Task<Guid?> TryAcquireAsync(Guid connectionId, CancellationToken cancellationToken = default)
            => Task.FromResult<Guid?>(Guid.NewGuid());

        public Task ReleaseAsync(Guid connectionId, Guid leaseId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class TrackingSyncLeaseService : IExternalCalendarSyncLeaseService
    {
        private readonly Guid _leaseId = Guid.NewGuid();

        public bool Released { get; private set; }
        public Guid? ReleasedConnectionId { get; private set; }

        public Task<Guid?> TryAcquireAsync(Guid connectionId, CancellationToken cancellationToken = default)
            => Task.FromResult<Guid?>(_leaseId);

        public Task ReleaseAsync(Guid connectionId, Guid leaseId, CancellationToken cancellationToken = default)
        {
            if (leaseId == _leaseId)
            {
                Released = true;
                ReleasedConnectionId = connectionId;
            }

            return Task.CompletedTask;
        }
    }
}
