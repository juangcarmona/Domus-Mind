using DomusMind.Application.Abstractions.Integrations.Calendar;
using DomusMind.Application.Features.Calendar.GetExternalCalendarConnectionDetail;
using DomusMind.Domain.Calendar.ExternalConnections;
using DomusMind.Domain.Family;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DomusMind.Application.Tests.Features.Calendar;

public sealed class GetExternalCalendarConnectionDetailQueryHandlerTests
{
    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static GetExternalCalendarConnectionDetailQueryHandler BuildHandler(
        DomusMindDbContext db,
        IExternalCalendarAuthService? authService = null,
        IExternalCalendarProviderClient? providerClient = null) =>
        new(db,
            new StubCalendarAuthorizationService(),
            authService ?? new StubExternalCalendarAuthService(accessToken: null),
            providerClient ?? new StubExternalCalendarProviderClient(),
            NullLogger<GetExternalCalendarConnectionDetailQueryHandler>.Instance);

    private static async Task<(DomusMindDbContext Db, ExternalCalendarConnection Connection, Guid FamilyId, Guid MemberId)>
        SeedConnectionWithFeedsAsync(
            params (string CalendarId, string Name, bool IsDefault, bool IsSelected)[] feeds)
    {
        var db = CreateDb();
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

    // -----------------------------------------------------------------------
    // Tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ReturnsFeedsWithCorrectIsSelected_AfterSave()
    {
        var (db, connection, familyId, memberId) = await SeedConnectionWithFeedsAsync(
            ("cal-1", "Work", true, true),
            ("cal-2", "Personal", false, false));

        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetExternalCalendarConnectionDetailQuery(familyId, memberId, connection.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Feeds.Should().HaveCount(2);

        var work = result.Feeds.First(f => f.CalendarId == "cal-1");
        work.IsSelected.Should().BeTrue();

        var personal = result.Feeds.First(f => f.CalendarId == "cal-2");
        personal.IsSelected.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_AfterConfigure_ReturnsUpdatedSelection()
    {
        // Arrange: two feeds, both selected from the start.
        var (db, connection, familyId, memberId) = await SeedConnectionWithFeedsAsync(
            ("cal-1", "Work", true, true),
            ("cal-2", "Personal", false, true));

        // Deselect cal-2 directly in the DB (simulates what ConfigureExternalCalendarConnection does).
        var feed2 = await db.Set<ExternalCalendarFeed>()
            .FirstAsync(f => f.ProviderCalendarId == "cal-2");
        feed2.UpdateSelection("Personal", false, DateTime.UtcNow);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new GetExternalCalendarConnectionDetailQuery(familyId, memberId, connection.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Feeds.Should().HaveCount(2);
        result.Feeds.First(f => f.CalendarId == "cal-1").IsSelected.Should().BeTrue();
        result.Feeds.First(f => f.CalendarId == "cal-2").IsSelected.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenAvailableCalendarsEnriched_ReturnsIsSelectedPersistedState()
    {
        // Available calendars from the provider include both calendars; only one is selected in DB.
        var (db, connection, familyId, memberId) = await SeedConnectionWithFeedsAsync(
            ("cal-1", "Work", true, true),
            ("cal-2", "Personal", false, false));

        var providerCalendars = new ExternalCalendarProviderCalendar[]
        {
            new("cal-1", "Work", true),
            new("cal-2", "Personal", false)
        };
        var authService = new StubExternalCalendarAuthService(accessToken: "tok");
        var providerClient = new StubExternalCalendarProviderClient(providerCalendars);
        var handler = BuildHandler(db, authService, providerClient);

        var result = await handler.Handle(
            new GetExternalCalendarConnectionDetailQuery(familyId, memberId, connection.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.AvailableCalendars.Should().HaveCount(2);

        var availWork = result.AvailableCalendars.First(c => c.CalendarId == "cal-1");
        availWork.IsSelected.Should().BeTrue("Work calendar was selected in DB");

        var availPersonal = result.AvailableCalendars.First(c => c.CalendarId == "cal-2");
        availPersonal.IsSelected.Should().BeFalse("Personal calendar was not selected in DB");
    }

    // -----------------------------------------------------------------------
    // Stubs
    // -----------------------------------------------------------------------

    private sealed class StubExternalCalendarAuthService : IExternalCalendarAuthService
    {
        private readonly string? _accessToken;

        public StubExternalCalendarAuthService(string? accessToken) => _accessToken = accessToken;

        public Task<ExternalCalendarProviderAccount> ExchangeAuthorizationCodeAsync(
            string authorizationCode, string redirectUri, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Not used in detail query tests.");

        public Task<string?> GetAccessTokenAsync(Guid connectionId, CancellationToken cancellationToken = default)
            => Task.FromResult(_accessToken);

        public Task RevokeAsync(Guid connectionId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class StubExternalCalendarProviderClient : IExternalCalendarProviderClient
    {
        private readonly IReadOnlyCollection<ExternalCalendarProviderCalendar> _calendars;

        public StubExternalCalendarProviderClient(
            IReadOnlyCollection<ExternalCalendarProviderCalendar>? calendars = null)
            => _calendars = calendars ?? [];

        public Task<IReadOnlyCollection<ExternalCalendarProviderCalendar>> GetCalendarsAsync(
            string accessToken, CancellationToken cancellationToken = default)
            => Task.FromResult(_calendars);

        public async IAsyncEnumerable<ExternalCalendarProviderDeltaPage> GetInitialEventsAsync(
            string accessToken, string calendarId, DateTime windowStartUtc, DateTime windowEndUtc,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }

        public async IAsyncEnumerable<ExternalCalendarProviderDeltaPage> GetDeltaEventsAsync(
            string accessToken, string calendarId, string deltaToken,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }
    }
}
