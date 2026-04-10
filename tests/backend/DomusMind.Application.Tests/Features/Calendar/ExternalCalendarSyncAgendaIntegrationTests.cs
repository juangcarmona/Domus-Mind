using DomusMind.Application.Abstractions.Integrations.Calendar;
using DomusMind.Application.Features.Calendar.GetMemberAgenda;
using DomusMind.Application.Features.Calendar.SyncExternalCalendarConnection;
using DomusMind.Domain.Calendar.ExternalConnections;
using DomusMind.Domain.Family;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DomusMind.Application.Tests.Features.Calendar;

public sealed class ExternalCalendarSyncAgendaIntegrationTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task SyncThenGetMemberAgenda_WhenExternalEventImported_ReturnsReadOnlyAgendaItem()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var memberId = MemberId.New();
        var connectionId = ExternalCalendarConnectionId.New();
        var now = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc);

        var connection = ExternalCalendarConnection.Connect(
            connectionId,
            familyId,
            memberId,
            ExternalCalendarProvider.Microsoft,
            "provider-account",
            "member@outlook.com",
            "Member Outlook",
            "common",
            now);

        var feed = ExternalCalendarFeed.Create(
            connectionId,
            "cal-1",
            "Calendar",
            true,
            true,
            now);

        db.Set<ExternalCalendarConnection>().Add(connection);
        db.Set<ExternalCalendarFeed>().Add(feed);
        db.SetExternalCalendarConnectionAuthMaterial(connection, "enc-token", "Calendars.Read");
        await db.SaveChangesAsync();
        connection.ClearDomainEvents();

        var syncHandler = new SyncExternalCalendarConnectionCommandHandler(
            db,
            new EventLogWriter(db),
            new StubCalendarAuthorizationService(),
            new TestAuthService("access-token"),
            new TestProviderClient(now.AddHours(2)),
            new TestLeaseService(),
            NullLogger<SyncExternalCalendarConnectionCommandHandler>.Instance);

        await syncHandler.Handle(
            new SyncExternalCalendarConnectionCommand(
                familyId.Value,
                memberId.Value,
                connectionId.Value,
                "manual",
                Guid.NewGuid()),
            CancellationToken.None);

        var agendaHandler = new GetMemberAgendaQueryHandler(
            db,
            new StubCalendarAuthorizationService());

        var from = now.ToString("yyyy-MM-dd");
        var to = now.ToString("yyyy-MM-dd");

        var agenda = await agendaHandler.Handle(
            new GetMemberAgendaQuery(familyId.Value, memberId.Value, from, to, Guid.NewGuid()),
            CancellationToken.None);

        var imported = agenda.Items
            .Should().ContainSingle(i => i.Type == "external-calendar-entry")
            .Which;

        imported.IsReadOnly.Should().BeTrue();
        imported.ProviderLabel.Should().Be("Outlook");
        imported.OpenInProviderUrl.Should().NotBeNullOrWhiteSpace();
    }

    private sealed class TestAuthService : IExternalCalendarAuthService
    {
        private readonly string? _accessToken;

        public TestAuthService(string? accessToken)
        {
            _accessToken = accessToken;
        }

        public Task<ExternalCalendarProviderAccount> ExchangeAuthorizationCodeAsync(
            string authorizationCode,
            string redirectUri,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<string?> GetAccessTokenAsync(Guid connectionId, CancellationToken cancellationToken = default)
            => Task.FromResult(_accessToken);

        public Task RevokeAsync(Guid connectionId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class TestProviderClient : IExternalCalendarProviderClient
    {
        private readonly DateTime _start;

        public TestProviderClient(DateTime start)
        {
            _start = start;
        }

        public Task<IReadOnlyCollection<ExternalCalendarProviderCalendar>> GetCalendarsAsync(
            string accessToken,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<ExternalCalendarProviderCalendar>>([]);

        public async IAsyncEnumerable<ExternalCalendarProviderDeltaPage> GetInitialEventsAsync(
            string accessToken,
            string calendarId,
            DateTime windowStartUtc,
            DateTime windowEndUtc,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return new ExternalCalendarProviderDeltaPage(
                [
                    new ExternalCalendarProviderEvent(
                        "evt-1",
                        null,
                        null,
                        "Imported Outlook Event",
                        _start,
                        _start.AddHours(1),
                        false,
                        "Room 1",
                        null,
                        "confirmed",
                        "https://outlook.office.com/calendar/item/1",
                        DateTime.UtcNow,
                        false)
                ],
                "delta-token",
                true);

            await Task.CompletedTask;
        }

        public async IAsyncEnumerable<ExternalCalendarProviderDeltaPage> GetDeltaEventsAsync(
            string accessToken,
            string calendarId,
            string deltaToken,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return new ExternalCalendarProviderDeltaPage([], deltaToken, true);
            await Task.CompletedTask;
        }
    }

    private sealed class TestLeaseService : IExternalCalendarSyncLeaseService
    {
        public Task<Guid?> TryAcquireAsync(Guid connectionId, CancellationToken cancellationToken = default)
            => Task.FromResult<Guid?>(Guid.NewGuid());

        public Task ReleaseAsync(Guid connectionId, Guid leaseId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
