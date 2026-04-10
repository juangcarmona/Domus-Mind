using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DomusMind.Application.Abstractions.Integrations.Calendar;
using Microsoft.Extensions.Logging;

namespace DomusMind.Infrastructure.Integrations.Calendar.Microsoft;

/// <summary>
/// Implements IExternalCalendarProviderClient via direct Microsoft Graph REST API calls.
/// Supports calendarView (initial bounded load) and delta sync.
/// </summary>
public sealed class MicrosoftGraphCalendarClient : IExternalCalendarProviderClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MicrosoftGraphCalendarClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public MicrosoftGraphCalendarClient(
        IHttpClientFactory httpClientFactory,
        ILogger<MicrosoftGraphCalendarClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<ExternalCalendarProviderCalendar>> GetCalendarsAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(accessToken);
        var response = await client.GetFromJsonAsync<GraphCalendarListResponse>(
            "https://graph.microsoft.com/v1.0/me/calendars?$top=50&$select=id,name,isDefaultCalendar",
            JsonOptions,
            cancellationToken);

        if (response?.Value is null)
            return [];

        return response.Value.Select(c => new ExternalCalendarProviderCalendar(
            c.Id ?? string.Empty,
            c.Name ?? string.Empty,
            c.IsDefaultCalendar == true)).ToList().AsReadOnly();
    }

    public async IAsyncEnumerable<ExternalCalendarProviderDeltaPage> GetInitialEventsAsync(
        string accessToken,
        string calendarId,
        DateTime windowStartUtc,
        DateTime windowEndUtc,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // The "o" (round-trip) specifier produces ISO 8601 UTC, e.g. 2026-01-01T00:00:00.0000000Z.
        // calendarView/delta requires startDateTime and endDateTime.
        // $select and $top are NOT supported by calendarView/delta — page size is controlled via
        // the Prefer: odata.maxpagesize header instead.
        var startTime = Uri.EscapeDataString(windowStartUtc.ToString("o"));
        var endTime = Uri.EscapeDataString(windowEndUtc.ToString("o"));

        var url = $"https://graph.microsoft.com/v1.0/me/calendars/{Uri.EscapeDataString(calendarId)}/calendarView/delta" +
                  $"?startDateTime={startTime}&endDateTime={endTime}";

        _logger.LogInformation(
            "Initial calendarView delta sync for calendar {CalendarId}. Url={Url}",
            calendarId, url);

        await foreach (var page in FetchDeltaPagesAsync(accessToken, url, "initial", cancellationToken))
            yield return page;
    }

    public async IAsyncEnumerable<ExternalCalendarProviderDeltaPage> GetDeltaEventsAsync(
        string accessToken,
        string calendarId,
        string deltaToken,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // The delta token/link returned by Graph is always treated as opaque.
        // A full URL (the typical case for calendarView/delta) is used verbatim.
        // A bare token string (not a URL) is not expected from calendarView/delta but
        // is handled as a fallback to keep the method safe.
        var isFullUrl = deltaToken.StartsWith("http", StringComparison.OrdinalIgnoreCase);
        var url = isFullUrl
            ? deltaToken
            : $"https://graph.microsoft.com/v1.0/me/calendars/{Uri.EscapeDataString(calendarId)}/calendarView/delta?$deltatoken={Uri.EscapeDataString(deltaToken)}";

        _logger.LogInformation(
            "Incremental delta sync for calendar {CalendarId}. DeltaLinkIsFullUrl={IsFullUrl}",
            calendarId, isFullUrl);

        await foreach (var page in FetchDeltaPagesAsync(accessToken, url, "delta", cancellationToken))
            yield return page;
    }

    private async IAsyncEnumerable<ExternalCalendarProviderDeltaPage> FetchDeltaPagesAsync(
        string accessToken,
        string initialUrl,
        string syncType,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var client = CreateClient(accessToken);
        var url = initialUrl;
        var pageIndex = 0;

        while (url is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogDebug(
                "Fetching Graph delta page {PageIndex} ({SyncType}).",
                pageIndex,
                pageIndex == 0 ? syncType : "continuation");

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.TryAddWithoutValidation("Prefer", "odata.maxpagesize=50");

            using var httpResponse = await client.SendAsync(request, cancellationToken);

            // Throws HttpRequestException on 4xx/5xx — intentionally propagated so the
            // caller (SyncFeedAsync) does not mark the feed as successfully synced.
            httpResponse.EnsureSuccessStatusCode();

            var response = await httpResponse.Content.ReadFromJsonAsync<GraphDeltaResponse>(
                JsonOptions, cancellationToken);

            if (response is null)
                break;

            var providerEvents = (response.Value ?? []).Select(MapEvent).ToList();
            var nextLink = response.OdataNextLink;
            var deltaLink = response.OdataDeltaLink;
            var isLastPage = nextLink is null;

            yield return new ExternalCalendarProviderDeltaPage(
                providerEvents,
                isLastPage ? deltaLink : null,
                isLastPage);

            url = nextLink;
            pageIndex++;
        }
    }

    private HttpClient CreateClient(string accessToken)
    {
        var client = _httpClientFactory.CreateClient("MicrosoftGraph");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    private static ExternalCalendarProviderEvent MapEvent(GraphEvent evt)
    {
        DateTime startsAt = evt.Start?.DateTime is not null
            ? DateTime.Parse(evt.Start.DateTime, null, System.Globalization.DateTimeStyles.RoundtripKind)
            : DateTime.UtcNow;

        DateTime? endsAt = evt.End?.DateTime is not null
            ? DateTime.Parse(evt.End.DateTime, null, System.Globalization.DateTimeStyles.RoundtripKind)
            : null;

        var isDeleted = evt.Removed is not null;
        var status = evt.IsCancelled == true ? "cancelled" : "confirmed";

        return new ExternalCalendarProviderEvent(
            evt.Id ?? string.Empty,
            evt.ICalUId,
            evt.SeriesMasterId,
            evt.Subject ?? "(No title)",
            startsAt,
            endsAt,
            evt.IsAllDay == true,
            evt.Location?.DisplayName,
            null,
            status,
            evt.WebLink,
            evt.LastModifiedDateTime,
            isDeleted);
    }

    // --- Graph response models ---

    private sealed class GraphCalendarListResponse
    {
        [JsonPropertyName("value")]
        public List<GraphCalendar>? Value { get; set; }
    }

    private sealed class GraphCalendar
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("isDefaultCalendar")]
        public bool? IsDefaultCalendar { get; set; }
    }

    private sealed class GraphDeltaResponse
    {
        [JsonPropertyName("value")]
        public List<GraphEvent>? Value { get; set; }

        [JsonPropertyName("@odata.nextLink")]
        public string? OdataNextLink { get; set; }

        [JsonPropertyName("@odata.deltaLink")]
        public string? OdataDeltaLink { get; set; }
    }

    private sealed class GraphEvent
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("iCalUId")]
        public string? ICalUId { get; set; }

        [JsonPropertyName("seriesMasterId")]
        public string? SeriesMasterId { get; set; }

        [JsonPropertyName("subject")]
        public string? Subject { get; set; }

        [JsonPropertyName("start")]
        public GraphDateTimeZone? Start { get; set; }

        [JsonPropertyName("end")]
        public GraphDateTimeZone? End { get; set; }

        [JsonPropertyName("isAllDay")]
        public bool? IsAllDay { get; set; }

        [JsonPropertyName("isCancelled")]
        public bool? IsCancelled { get; set; }

        [JsonPropertyName("webLink")]
        public string? WebLink { get; set; }

        [JsonPropertyName("lastModifiedDateTime")]
        public DateTime? LastModifiedDateTime { get; set; }

        [JsonPropertyName("location")]
        public GraphLocation? Location { get; set; }

        [JsonPropertyName("@removed")]
        public object? Removed { get; set; }
    }

    private sealed class GraphDateTimeZone
    {
        [JsonPropertyName("dateTime")]
        public string? DateTime { get; set; }

        [JsonPropertyName("timeZone")]
        public string? TimeZone { get; set; }
    }

    private sealed class GraphLocation
    {
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }
    }
}
