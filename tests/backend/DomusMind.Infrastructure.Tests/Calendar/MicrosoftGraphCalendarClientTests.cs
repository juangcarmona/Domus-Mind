using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DomusMind.Infrastructure.Integrations.Calendar.Microsoft;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace DomusMind.Infrastructure.Tests.Calendar;

public sealed class MicrosoftGraphCalendarClientTests
{
    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static MicrosoftGraphCalendarClient BuildClient(FakeHttpHandler handler)
    {
        var factory = new FakeHttpClientFactory(handler);
        return new MicrosoftGraphCalendarClient(
            factory,
            NullLogger<MicrosoftGraphCalendarClient>.Instance);
    }

    /// <summary>
    /// Returns a JSON body representing a final delta page (no nextLink, has deltaLink).
    /// </summary>
    private static HttpResponseMessage FinalDeltaPage(string deltaLink = "https://graph.microsoft.com/v1.0/me/calendars/cal-123/calendarView/delta?$deltatoken=abc123")
    {
        var payload = new
        {
            value = Array.Empty<object>(),
            @odata_deltaLink = deltaLink
        };

        // Build the JSON manually to ensure the odata key is correct.
        var json = $$"""{"value":[],"@odata.deltaLink":"{{deltaLink}}"}""";
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };
    }

    // -----------------------------------------------------------------------
    // Tests: DateTime UTC boundary — the Npgsql timestamptz requirement
    // -----------------------------------------------------------------------

    [Fact]
    public void ParseGraphDateTimeAsUtc_WhenTimeZoneIsUTC_ReturnsUtcKind()
    {
        var result = MicrosoftGraphCalendarClient.ParseGraphDateTimeAsUtc(
            "2026-04-10T09:30:00.0000000", "UTC");

        result.Kind.Should().Be(DateTimeKind.Utc);
        result.Should().Be(new DateTime(2026, 4, 10, 9, 30, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void ParseGraphDateTimeAsUtc_WhenTimeZoneIsNull_ReturnsUtcKind()
    {
        var result = MicrosoftGraphCalendarClient.ParseGraphDateTimeAsUtc(
            "2026-04-10T09:30:00.0000000", null);

        result.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void ParseGraphDateTimeAsUtc_WhenTimeZoneIsEmpty_ReturnsUtcKind()
    {
        var result = MicrosoftGraphCalendarClient.ParseGraphDateTimeAsUtc(
            "2026-04-10T09:30:00.0000000", "");

        result.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void ParseGraphDateTimeAsUtc_WhenTimeZoneIsKnownIana_ConvertsToUtc()
    {
        // America/New_York is UTC-4 during EDT (April).
        var result = MicrosoftGraphCalendarClient.ParseGraphDateTimeAsUtc(
            "2026-04-10T09:30:00.0000000", "America/New_York");

        result.Kind.Should().Be(DateTimeKind.Utc);
        // 09:30 EDT (UTC-4) = 13:30 UTC
        result.Should().Be(new DateTime(2026, 4, 10, 13, 30, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void ParseGraphDateTimeAsUtc_WhenTimeZoneIsUnknown_FallsBackToUtcKind()
    {
        // Unknown zone IDs must not throw; fall back to UTC treatment.
        var act = () => MicrosoftGraphCalendarClient.ParseGraphDateTimeAsUtc(
            "2026-04-10T09:30:00.0000000", "Unknown/Zone");

        var result = act.Should().NotThrow().Which;
        result.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task GetInitialEventsAsync_GraphEventTimes_AreReturnedAsUtcKind()
    {
        // Build a response containing a Graph event with UTC start/end (no Z suffix,
        // timeZone = "UTC"), matching what Graph returns by default.
        var json = """
            {
                "value": [
                    {
                        "id": "evt-1",
                        "subject": "Stand-up",
                        "start": { "dateTime": "2026-04-10T09:30:00.0000000", "timeZone": "UTC" },
                        "end":   { "dateTime": "2026-04-10T09:45:00.0000000", "timeZone": "UTC" },
                        "lastModifiedDateTime": "2026-04-09T08:00:00Z"
                    }
                ],
                "@odata.deltaLink": "https://graph.microsoft.com/v1.0/me/calendars/cal-1/calendarView/delta?$deltatoken=tok"
            }
            """;

        var handler = new FakeHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
        });
        var client = BuildClient(handler);

        var pages = new List<DomusMind.Application.Abstractions.Integrations.Calendar.ExternalCalendarProviderDeltaPage>();
        await foreach (var page in client.GetInitialEventsAsync(
            "tok", "cal-1",
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc)))
        {
            pages.Add(page);
        }

        var evt = pages.SelectMany(p => p.Events).Should().ContainSingle().Which;
        evt.StartsAtUtc.Kind.Should().Be(DateTimeKind.Utc,
            "Npgsql timestamptz requires UTC Kind; Kind=Unspecified causes Npgsql to throw");
        evt.EndsAtUtc.Should().NotBeNull();
        evt.EndsAtUtc!.Value.Kind.Should().Be(DateTimeKind.Utc);
    }

    // -----------------------------------------------------------------------
    // Tests: Initial sync URL structure
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetInitialEventsAsync_IncludesStartDateTimeAndEndDateTime_InUrl()
    {
        var handler = new FakeHttpHandler(_ => FinalDeltaPage());
        var client = BuildClient(handler);

        var windowStart = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var windowEnd = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);

        await foreach (var _ in client.GetInitialEventsAsync("tok", "cal-123", windowStart, windowEnd))
        {
            // consume first page only
            break;
        }

        handler.LastRequestUri.Should().NotBeNull();
        var query = handler.LastRequestUri!.Query;

        // Query string should include startDateTime and endDateTime.
        query.Should().Contain("startDateTime=", "Graph calendarView/delta requires startDateTime");
        query.Should().Contain("endDateTime=", "Graph calendarView/delta requires endDateTime");

        // Values should reflect the ISO 8601 UTC format.
        var unescaped = Uri.UnescapeDataString(query);
        unescaped.Should().Contain("2026-01-01T00:00:00", "windowStart date should appear in URL");
        unescaped.Should().Contain("2026-04-01T00:00:00", "windowEnd date should appear in URL");

        // Path should reference calendarView/delta.
        handler.LastRequestUri.AbsolutePath
            .Should().EndWith("calendarView/delta");
    }

    [Fact]
    public async Task GetInitialEventsAsync_UrlContainsCalendarId_Escaped()
    {
        var handler = new FakeHttpHandler(_ => FinalDeltaPage());
        var client = BuildClient(handler);

        var windowStart = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var windowEnd = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);

        await foreach (var _ in client.GetInitialEventsAsync("tok", "my calendar id", windowStart, windowEnd))
            break;

        // Calendar ID with spaces should be percent-encoded in the path.
        handler.LastRequestUri!.AbsolutePath.Should().Contain("my%20calendar%20id");
    }

    // -----------------------------------------------------------------------
    // Tests: Delta link treated as opaque
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetDeltaEventsAsync_WhenDeltaLinkIsFullUrl_UsesItVerbatim()
    {
        const string storedDeltaLink =
            "https://graph.microsoft.com/v1.0/me/calendars/cal-123/calendarView/delta?$deltatoken=very-opaque-token-abc";

        var handler = new FakeHttpHandler(_ => FinalDeltaPage(storedDeltaLink));
        var client = BuildClient(handler);

        await foreach (var _ in client.GetDeltaEventsAsync("tok", "cal-123", storedDeltaLink))
            break;

        handler.LastRequestUri.Should().NotBeNull();
        // The exact stored URL — including its token — must be used verbatim.
        handler.LastRequestUri!.AbsoluteUri.Should().Be(storedDeltaLink);
    }

    [Fact]
    public async Task GetDeltaEventsAsync_WhenDeltaLinkIsFullUrl_DoesNotAppendParams()
    {
        // Verify that no extra query params (like startDateTime) are appended to delta link requests.
        const string storedDeltaLink =
            "https://graph.microsoft.com/v1.0/me/calendars/cal-123/calendarView/delta?$deltatoken=abc";

        var handler = new FakeHttpHandler(_ => FinalDeltaPage(storedDeltaLink));
        var client = BuildClient(handler);

        await foreach (var _ in client.GetDeltaEventsAsync("tok", "cal-123", storedDeltaLink))
            break;

        var query = handler.LastRequestUri!.Query;
        query.Should().NotContain("startDateTime");
        query.Should().NotContain("endDateTime");
    }

    [Fact]
    public async Task GetInitialEventsAsync_DoesNotInclude_SelectOrTop_InUrl()
    {
        var handler = new FakeHttpHandler(_ => FinalDeltaPage());
        var client = BuildClient(handler);

        var windowStart = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var windowEnd = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);

        await foreach (var _ in client.GetInitialEventsAsync("tok", "cal-123", windowStart, windowEnd))
            break;

        var query = handler.LastRequestUri!.Query;
        query.Should().NotContain("$select", "calendarView/delta does not support $select");
        query.Should().NotContain("$top", "calendarView/delta does not support $top");
    }

    [Fact]
    public async Task GetInitialEventsAsync_SendsPreferHeader_WithMaxPageSize()
    {
        var handler = new FakeHttpHandler(_ => FinalDeltaPage());
        var client = BuildClient(handler);

        var windowStart = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var windowEnd = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);

        await foreach (var _ in client.GetInitialEventsAsync("tok", "cal-123", windowStart, windowEnd))
            break;

        handler.LastRequest!.Headers.TryGetValues("Prefer", out var prefer).Should().BeTrue();
        prefer!.Should().ContainSingle().Which.Should().Be("odata.maxpagesize=50");
    }

    // -----------------------------------------------------------------------
    // Tests: 4xx error handling
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetInitialEventsAsync_WhenGraphReturns400_ThrowsHttpRequestException()
    {
        var handler = new FakeHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("""{"error":{"code":"BadRequest","message":"startDateTime is required."}}""",
                System.Text.Encoding.UTF8, "application/json")
        });
        var client = BuildClient(handler);

        var windowStart = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var windowEnd = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);

        Func<Task> act = async () =>
        {
            await foreach (var _ in client.GetInitialEventsAsync("tok", "cal-123", windowStart, windowEnd))
            {
                // should throw before yielding any page
            }
        };

        await act.Should().ThrowAsync<HttpRequestException>(
            "a 400 response from Graph must surface as an exception, not silently abort");
    }

    [Fact]
    public async Task GetDeltaEventsAsync_WhenGraphReturns401_ThrowsHttpRequestException()
    {
        var handler = new FakeHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("""{"error":{"code":"InvalidAuthenticationToken"}}""",
                System.Text.Encoding.UTF8, "application/json")
        });
        var client = BuildClient(handler);

        Func<Task> act = async () =>
        {
            await foreach (var _ in client.GetDeltaEventsAsync("tok", "cal-123",
                "https://graph.microsoft.com/v1.0/me/calendars/cal-123/calendarView/delta?$deltatoken=abc"))
            {
            }
        };

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    // -----------------------------------------------------------------------
    // Tests: recurring event title mapping
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetInitialEventsAsync_WhenSubjectIsEmpty_UsesFallbackTitle()
    {
        var json = """
            {
                "value": [
                    {
                        "id": "evt-recurring",
                        "subject": "",
                        "seriesMasterId": "master-1",
                        "start": { "dateTime": "2026-04-10T09:00:00.0000000", "timeZone": "UTC" },
                        "end":   { "dateTime": "2026-04-10T09:30:00.0000000", "timeZone": "UTC" }
                    }
                ],
                "@odata.deltaLink": "https://graph.microsoft.com/v1.0/delta?$deltatoken=tok"
            }
            """;

        var handler = new FakeHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
        });
        var client = BuildClient(handler);

        var pages = new List<DomusMind.Application.Abstractions.Integrations.Calendar.ExternalCalendarProviderDeltaPage>();
        await foreach (var page in client.GetInitialEventsAsync(
            "tok", "cal-1",
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc)))
        {
            pages.Add(page);
        }

        var evt = pages.SelectMany(p => p.Events).Should().ContainSingle().Which;
        evt.Title.Should().Be("(No title)",
            "an empty string subject must produce the fallback title, not an empty display value");
    }

    [Fact]
    public async Task GetInitialEventsAsync_WhenSubjectHasValue_PreservesTitle()
    {
        var json = """
            {
                "value": [
                    {
                        "id": "evt-recurring-2",
                        "subject": "Weekly Sync",
                        "seriesMasterId": "master-2",
                        "start": { "dateTime": "2026-04-10T10:00:00.0000000", "timeZone": "UTC" },
                        "end":   { "dateTime": "2026-04-10T10:30:00.0000000", "timeZone": "UTC" }
                    }
                ],
                "@odata.deltaLink": "https://graph.microsoft.com/v1.0/delta?$deltatoken=tok2"
            }
            """;

        var handler = new FakeHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
        });
        var client = BuildClient(handler);

        var pages = new List<DomusMind.Application.Abstractions.Integrations.Calendar.ExternalCalendarProviderDeltaPage>();
        await foreach (var page in client.GetInitialEventsAsync(
            "tok", "cal-1",
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc)))
        {
            pages.Add(page);
        }

        pages.SelectMany(p => p.Events).Should().ContainSingle()
            .Which.Title.Should().Be("Weekly Sync");
    }

    // -----------------------------------------------------------------------
    // Tests: OriginalTimezone population
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetInitialEventsAsync_PopulatesOriginalTimezone_FromGraphStartField()
    {
        var json = """
            {
                "value": [
                    {
                        "id": "evt-tz",
                        "subject": "Standup",
                        "start": { "dateTime": "2026-04-10T09:00:00.0000000", "timeZone": "Eastern Standard Time" },
                        "end":   { "dateTime": "2026-04-10T09:30:00.0000000", "timeZone": "Eastern Standard Time" }
                    }
                ],
                "@odata.deltaLink": "https://graph.microsoft.com/v1.0/delta?$deltatoken=tok"
            }
            """;

        var handler = new FakeHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
        });
        var client = BuildClient(handler);

        var pages = new List<DomusMind.Application.Abstractions.Integrations.Calendar.ExternalCalendarProviderDeltaPage>();
        await foreach (var page in client.GetInitialEventsAsync(
            "tok", "cal-1",
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc)))
        {
            pages.Add(page);
        }

        var evt = pages.SelectMany(p => p.Events).Should().ContainSingle().Which;
        evt.OriginalTimezone.Should().Be("Eastern Standard Time",
            "OriginalTimezone must carry the source timezone for correct local-time display");
        // 09:00 EST (UTC-4 in April) = 13:00 UTC
        evt.StartsAtUtc.Should().Be(new DateTime(2026, 4, 10, 13, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task GetInitialEventsAsync_WhenTimeZoneIsUTC_OriginalTimezoneIsUTC()
    {
        var json = """
            {
                "value": [
                    {
                        "id": "evt-utc",
                        "subject": "Backlog Review",
                        "start": { "dateTime": "2026-04-10T14:00:00.0000000", "timeZone": "UTC" },
                        "end":   { "dateTime": "2026-04-10T14:30:00.0000000", "timeZone": "UTC" }
                    }
                ],
                "@odata.deltaLink": "https://graph.microsoft.com/v1.0/delta?$deltatoken=tok"
            }
            """;

        var handler = new FakeHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
        });
        var client = BuildClient(handler);

        var pages = new List<DomusMind.Application.Abstractions.Integrations.Calendar.ExternalCalendarProviderDeltaPage>();
        await foreach (var page in client.GetInitialEventsAsync(
            "tok", "cal-1",
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc)))
        {
            pages.Add(page);
        }

        var evt = pages.SelectMany(p => p.Events).Should().ContainSingle().Which;
        evt.OriginalTimezone.Should().Be("UTC");
        evt.StartsAtUtc.Should().Be(new DateTime(2026, 4, 10, 14, 0, 0, DateTimeKind.Utc));
    }

    // -----------------------------------------------------------------------
    // Fake infrastructure
    // -----------------------------------------------------------------------

    internal sealed class FakeHttpHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;

        public Uri? LastRequestUri { get; private set; }
        public HttpRequestMessage? LastRequest { get; private set; }

        public FakeHttpHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
            => _respond = respond;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri;
            LastRequest = request;
            return Task.FromResult(_respond(request));
        }
    }

    private sealed class FakeHttpClientFactory : System.Net.Http.IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;

        public FakeHttpClientFactory(HttpMessageHandler handler) => _handler = handler;

        public HttpClient CreateClient(string name) => new(_handler);
    }
}
