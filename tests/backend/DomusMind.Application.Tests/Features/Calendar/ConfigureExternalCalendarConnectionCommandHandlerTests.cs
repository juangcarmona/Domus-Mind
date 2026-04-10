using DomusMind.Application.Features.Calendar;
using DomusMind.Application.Features.Calendar.ConfigureExternalCalendarConnection;
using DomusMind.Domain.Calendar.ExternalConnections;
using DomusMind.Domain.Family;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DomusMind.Application.Tests.Features.Calendar;

public sealed class ConfigureExternalCalendarConnectionCommandHandlerTests
{
    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ConfigureExternalCalendarConnectionCommandHandler BuildHandler(
        DomusMindDbContext db,
        StubCalendarAuthorizationService? auth = null)
        => new(
            db,
            new EventLogWriter(db),
            auth ?? new StubCalendarAuthorizationService(),
            NullLogger<ConfigureExternalCalendarConnectionCommandHandler>.Instance);

    /// <summary>
    /// Seeds a connection with optionally pre-existing feeds into the DB and
    /// returns the DB and the fully-tracked connection.
    /// </summary>
    private static async Task<(DomusMindDbContext Db, ExternalCalendarConnection Connection, Guid FamilyId, Guid MemberId)>
        SeedConnectionAsync(
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

        // Detach everything; re-load fresh so the handler starts from a clean tracked state.
        db.ChangeTracker.Clear();

        var loaded = await db.Set<ExternalCalendarConnection>()
            .Include(c => c.Feeds)
            .FirstAsync(c => c.Id == connectionId);

        return (db, loaded, familyId.Value, memberId.Value);
    }

    private static ConfigureExternalCalendarConnectionCommand BuildCommand(
        Guid familyId,
        Guid memberId,
        Guid connectionId,
        IEnumerable<(string CalendarId, string Name, bool IsSelected)> selections,
        int horizonDays = 90,
        bool refreshEnabled = true,
        int refreshInterval = 60)
    {
        var sel = selections
            .Select(s => (s.CalendarId, CalendarName: s.Name, s.IsSelected, (string?)null))
            .ToList()
            .AsReadOnly();

        return new ConfigureExternalCalendarConnectionCommand(
            familyId,
            memberId,
            connectionId,
            sel,
            horizonDays,
            refreshEnabled,
            refreshInterval,
            Guid.NewGuid());
    }

    // -----------------------------------------------------------------------
    // Tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_UpdatesExistingFeedSelectionInPlace_NoNewRows()
    {
        var (db, connection, familyId, memberId) = await SeedConnectionAsync(
            feeds: ("cal-1", "Work", true, false));

        var handler = BuildHandler(db);

        var result = await handler.Handle(
            BuildCommand(familyId, memberId, connection.Id.Value,
                [("cal-1", "Work", true)]),
            CancellationToken.None);

        result.SelectedCalendarCount.Should().Be(1);

        var feeds = await db.Set<ExternalCalendarFeed>()
            .Where(f => f.ConnectionId == connection.Id)
            .ToListAsync();

        feeds.Should().HaveCount(1);
        feeds[0].IsSelected.Should().BeTrue();
        feeds[0].ProviderCalendarId.Should().Be("cal-1");
    }

    [Fact]
    public async Task Handle_TogglesCalendarSelection_DeselectsAndSelects()
    {
        var (db, connection, familyId, memberId) = await SeedConnectionAsync(
            feeds:
            [
                ("cal-1", "Work", true, true),    // currently selected → will be deselected
                ("cal-2", "Personal", false, false) // currently unselected → will be selected
            ]);

        var handler = BuildHandler(db);

        await handler.Handle(
            BuildCommand(familyId, memberId, connection.Id.Value,
            [
                ("cal-1", "Work",     isSelected: false),
                ("cal-2", "Personal", isSelected: true)
            ]),
            CancellationToken.None);

        db.ChangeTracker.Clear();
        var feeds = await db.Set<ExternalCalendarFeed>()
            .Where(f => f.ConnectionId == connection.Id)
            .ToListAsync();

        feeds.Should().HaveCount(2);
        feeds.First(f => f.ProviderCalendarId == "cal-1").IsSelected.Should().BeFalse();
        feeds.First(f => f.ProviderCalendarId == "cal-2").IsSelected.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_AddsNewFeed_WhenCalendarIdNotYetInDb()
    {
        var (db, connection, familyId, memberId) = await SeedConnectionAsync(
            feeds: ("cal-1", "Work", true, true));

        var handler = BuildHandler(db);

        var result = await handler.Handle(
            BuildCommand(familyId, memberId, connection.Id.Value,
            [
                ("cal-1", "Work",    isSelected: true),
                ("cal-2", "Personal", isSelected: true) // new calendar
            ]),
            CancellationToken.None);

        result.SelectedCalendarCount.Should().Be(2);

        db.ChangeTracker.Clear();
        var feeds = await db.Set<ExternalCalendarFeed>()
            .Where(f => f.ConnectionId == connection.Id)
            .ToListAsync();

        feeds.Should().HaveCount(2);
        feeds.Select(f => f.ProviderCalendarId).Should().BeEquivalentTo(["cal-1", "cal-2"]);
        feeds.All(f => f.IsSelected).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DeselectedFeedNotInSelections_IsSetToUnselected()
    {
        // A feed in the DB that is NOT sent in the incoming selections should be deselected.
        var (db, connection, familyId, memberId) = await SeedConnectionAsync(
            feeds:
            [
                ("cal-1", "Work",    true,  true),
                ("cal-2", "Old Cal", false, true) // selected, but will not appear in command
            ]);

        var handler = BuildHandler(db);

        // Only cal-1 is mentioned; cal-2 is absent from the selection list.
        await handler.Handle(
            BuildCommand(familyId, memberId, connection.Id.Value,
                [("cal-1", "Work", isSelected: true)]),
            CancellationToken.None);

        db.ChangeTracker.Clear();
        var cal2 = await db.Set<ExternalCalendarFeed>()
            .FirstAsync(f => f.ProviderCalendarId == "cal-2");

        cal2.IsSelected.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ChangeHorizonOnly_DoesNotThrow_AndInvalidatesDeltaTokens()
    {
        var (db, connection, familyId, memberId) = await SeedConnectionAsync(
            feeds: ("cal-1", "Work", true, true));

        // Manually set a delta token so we can verify invalidation.
        var feed = await db.Set<ExternalCalendarFeed>()
            .FirstAsync(f => f.ConnectionId == connection.Id);
        feed.RecordDeltaToken("some-delta-token", DateTime.UtcNow);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var handler = BuildHandler(db);

        // Reload the connection after the delta token was set.
        var freshConn = await db.Set<ExternalCalendarConnection>()
            .Include(c => c.Feeds)
            .FirstAsync(c => c.Id == connection.Id);

        var configure = () => handler.Handle(
            BuildCommand(familyId, memberId, freshConn.Id.Value,
                [("cal-1", "Work", isSelected: true)],
                horizonDays: 180), // changed from default 90
            CancellationToken.None);

        await configure.Should().NotThrowAsync();

        db.ChangeTracker.Clear();
        var updatedFeed = await db.Set<ExternalCalendarFeed>()
            .FirstAsync(f => f.ConnectionId == connection.Id);

        updatedFeed.LastDeltaToken.Should().BeNull("horizon change must invalidate stored delta tokens");

        var updatedConn = await db.Set<ExternalCalendarConnection>()
            .FirstAsync(c => c.Id == connection.Id);
        updatedConn.Horizon.ForwardHorizonDays.Should().Be(180);
    }

    [Fact]
    public async Task Handle_MultipleFeeds_NoExceptionAndAllMutationsCommitted()
    {
        // Verify that updating multiple existing feeds in one call does not throw
        // DbUpdateConcurrencyException and all changes are committed.
        var (db, connection, familyId, memberId) = await SeedConnectionAsync(
            feeds:
            [
                ("cal-1", "Work",     true,  true),
                ("cal-2", "Personal", false, false),
                ("cal-3", "Shared",   false, true)
            ]);

        var handler = BuildHandler(db);

        // Re-load fresh after seeding so the handler gets a clean tracked context.
        var freshConn = await db.Set<ExternalCalendarConnection>()
            .Include(c => c.Feeds)
            .FirstAsync(c => c.Id == connection.Id);

        var configure = () => handler.Handle(
            BuildCommand(familyId, memberId, freshConn.Id.Value,
            [
                ("cal-1", "Work",     isSelected: false),  // deselect
                ("cal-2", "Personal", isSelected: true),   // select
                ("cal-3", "Shared",   isSelected: true),   // keep selected
                ("cal-4", "New Cal",  isSelected: true)    // add new
            ]),
            CancellationToken.None);

        await configure.Should().NotThrowAsync();

        db.ChangeTracker.Clear();
        var feeds = await db.Set<ExternalCalendarFeed>()
            .Where(f => f.ConnectionId == connection.Id)
            .ToListAsync();

        feeds.Should().HaveCount(4);
        feeds.First(f => f.ProviderCalendarId == "cal-1").IsSelected.Should().BeFalse();
        feeds.First(f => f.ProviderCalendarId == "cal-2").IsSelected.Should().BeTrue();
        feeds.First(f => f.ProviderCalendarId == "cal-3").IsSelected.Should().BeTrue();
        feeds.First(f => f.ProviderCalendarId == "cal-4").IsSelected.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ConnectionNotFound_ThrowsCalendarException()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var configure = () => handler.Handle(
            BuildCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                [("cal-1", "Work", isSelected: true)]),
            CancellationToken.None);

        await configure.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.ConnectionNotFound);
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsCalendarException()
    {
        var (db, connection, familyId, memberId) = await SeedConnectionAsync();
        var auth = new StubCalendarAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var configure = () => handler.Handle(
            BuildCommand(familyId, memberId, connection.Id.Value, []),
            CancellationToken.None);

        await configure.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_InvalidHorizonDays_ThrowsCalendarException()
    {
        var (db, connection, familyId, memberId) = await SeedConnectionAsync();
        var handler = BuildHandler(db);

        var configure = () => handler.Handle(
            BuildCommand(familyId, memberId, connection.Id.Value, [],
                horizonDays: 999), // not an allowed value
            CancellationToken.None);

        await configure.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.InvalidInput);
    }
}
