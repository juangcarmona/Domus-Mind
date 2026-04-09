using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
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

    // Maximum pages per calendar per sync round before pagination is considered pathological.
    // A household calendar with ~1 000 events at page size 50 consumes ~20 pages; 200 pages
    // (10 000 events) is a realistic hard ceiling for any single-calendar sync.
    private const int MaxPagesPerCalendarRound = 200;

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
            "https://graph.microsoft.com/v1.0/me/calendars?$top=50&$select=id,name,isDefaultCalendar,hexColor",
            JsonOptions,
            cancellationToken);

        if (response?.Value is null)
            return [];

        return response.Value.Select(c => new ExternalCalendarProviderCalendar(
            c.Id ?? string.Empty,
            c.Name ?? string.Empty,
            c.IsDefaultCalendar == true,
            string.IsNullOrWhiteSpace(c.HexColor) ? null : c.HexColor)).ToList().AsReadOnly();
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

        await foreach (var page in FetchDeltaPagesAsync(accessToken, calendarId, url, "initial", cancellationToken))
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

        await foreach (var page in FetchDeltaPagesAsync(accessToken, calendarId, url, "delta", cancellationToken))
            yield return page;
    }

    // -------------------------------------------------------------------------
    // FetchDeltaPagesAsync — pagination model and observability contract
    //
    // Graph calendarView/delta does NOT expose the total page count before
    // iteration begins. The @odata.nextLink / $skipToken values are fully opaque
    // server-side cursors; they carry no item-count or page-count metadata.
    // The only reliable completion signal is the presence of @odata.deltaLink
    // in a response, which Graph returns instead of @odata.nextLink on the
    // final page. Page count is therefore revealed only by following the chain.
    //
    // What "normal" looks like:
    //   A calendar with N events at page size 50 produces at least ⌈N/50⌉ pages.
    //   Multiple requests for the same calendar in one sync round are expected.
    //
    // What indicates a broken loop:
    //   The same @odata.nextLink URL appearing twice in the same round is never
    //   valid — each cursor must advance. A hard cap of MaxPagesPerCalendarRound
    //   provides a second safety net against runaway pagination.
    // -------------------------------------------------------------------------
    private async IAsyncEnumerable<ExternalCalendarProviderDeltaPage> FetchDeltaPagesAsync(
        string accessToken,
        string calendarId,
        string initialUrl,
        string syncType,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var client = CreateClient(accessToken);
        var url = initialUrl;
        var pageIndex = 0;
        var runningTotal = 0;

        // Tracks every @odata.nextLink seen this round. A repeated link means Graph
        // is returning the same cursor twice — the client would loop forever.
        var seenNextLinks = new HashSet<string>(StringComparer.Ordinal);

        // Short identifier for correlating all per-page log entries within one round.
        var roundId = Guid.NewGuid().ToString("N")[..8];
        var roundSw = Stopwatch.StartNew();

        while (url is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (pageIndex >= MaxPagesPerCalendarRound)
            {
                throw new InvalidOperationException(
                    $"Graph delta sync exceeded {MaxPagesPerCalendarRound} pages for calendar '{calendarId}'. " +
                    $"RoundId={roundId} PagesProcessed={pageIndex} ItemsProcessed={runningTotal} " +
                    $"LastNextLinkHash={HashLink(url)}. Aborting to prevent a runaway pagination loop.");
            }

            var pageSw = Stopwatch.StartNew();

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.TryAddWithoutValidation("Prefer", "odata.maxpagesize=50");

            using var httpResponse = await client.SendAsync(request, cancellationToken);

            // Throws HttpRequestException on 4xx/5xx — intentionally propagated so the
            // caller (SyncFeedAsync) does not mark the feed as successfully synced.
            httpResponse.EnsureSuccessStatusCode();

            var response = await httpResponse.Content.ReadFromJsonAsync<GraphDeltaResponse>(
                JsonOptions, cancellationToken);

            pageSw.Stop();

            if (response is null)
                break;

            var providerEvents = (response.Value ?? []).Select(MapEvent).ToList();
            var nextLink = response.OdataNextLink;
            var deltaLink = response.OdataDeltaLink;
            var isLastPage = nextLink is null;

            runningTotal += providerEvents.Count;

            _logger.LogInformation(
                "Graph delta page fetched. CalendarId={CalendarId} SyncType={SyncType} RoundId={RoundId} " +
                "Page={Page} Items={Items} HasNextLink={HasNextLink} HasDeltaLink={HasDeltaLink} " +
                "NextLinkHash={NextLinkHash} DeltaLinkHash={DeltaLinkHash} " +
                "RunningTotal={RunningTotal} PageElapsedMs={PageElapsedMs}",
                calendarId, syncType, roundId,
                pageIndex, providerEvents.Count,
                nextLink is not null, deltaLink is not null,
                HashLink(nextLink), HashLink(deltaLink),
                runningTotal, pageSw.ElapsedMilliseconds);

            // A repeated @odata.nextLink means Graph returned the same cursor twice —
            // following it would spin forever.
            if (nextLink is not null && !seenNextLinks.Add(nextLink))
            {
                throw new InvalidOperationException(
                    $"Graph delta sync detected a repeated @odata.nextLink for calendar '{calendarId}'. " +
                    $"RoundId={roundId} Page={pageIndex} NextLinkHash={HashLink(nextLink)} " +
                    $"ItemsProcessed={runningTotal}. This indicates a broken pagination cursor.");
            }

            yield return new ExternalCalendarProviderDeltaPage(
                providerEvents,
                isLastPage ? deltaLink : null,
                isLastPage);

            url = nextLink;
            pageIndex++;
        }

        _logger.LogInformation(
            "Graph delta round completed. CalendarId={CalendarId} SyncType={SyncType} RoundId={RoundId} " +
            "TotalPages={TotalPages} TotalItems={TotalItems} RoundElapsedMs={RoundElapsedMs}",
            calendarId, syncType, roundId, pageIndex, runningTotal, roundSw.ElapsedMilliseconds);
    }

    private HttpClient CreateClient(string accessToken)
    {
        var client = _httpClientFactory.CreateClient("MicrosoftGraph");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    // -------------------------------------------------------------------------
    // MapEvent — timezone semantics for imported events
    //
    // StartsAtUtc / EndsAtUtc: true UTC moment (Kind=Utc) from
    //   ParseGraphDateTimeAsUtc(start.dateTime, start.timeZone).
    //
    // OriginalTimezone: the zone the event was *authored* in, NOT the
    //   response-wire zone (start.timeZone = "UTC" when no Prefer:outlook.timezone
    //   header is sent). Graph returns originalStartTimeZone unconditionally.
    //   FormatLocalTime/LocalDateOf in GetWeeklyGridQueryHandler use this to
    //   convert StartsAtUtc back to household local time for display.
    // -------------------------------------------------------------------------
    private ExternalCalendarProviderEvent MapEvent(GraphEvent evt)
    {
        DateTime startsAt = evt.Start?.DateTime is not null
            ? ParseGraphDateTimeAsUtc(evt.Start.DateTime, evt.Start.TimeZone)
            : DateTime.UtcNow;

        DateTime? endsAt = evt.End?.DateTime is not null
            ? ParseGraphDateTimeAsUtc(evt.End.DateTime, evt.End.TimeZone)
            : null;

        // Graph returns lastModifiedDateTime as ISO 8601 with Z. Ensure Kind=Utc after
        // JSON deserialization regardless of how System.Text.Json resolved the value.
        DateTime? lastModified = evt.LastModifiedDateTime.HasValue
            ? EnsureUtc(evt.LastModifiedDateTime.Value)
            : null;

        var isDeleted = evt.Removed is not null;
        var status = evt.IsCancelled == true ? "cancelled" : "confirmed";

        // Graph may return an empty/null subject for some recurring-series occurrences
        // (e.g. cancelled exceptions where the invite body is withheld). Treat both
        // null and empty string as truly missing rather than only null.
        var title = string.IsNullOrWhiteSpace(evt.Subject) ? "(No title)" : evt.Subject!;

        // Prefer the event's authored timezone (originalStartTimeZone) over the
        // response-format timezone (start.timeZone, which is always "UTC" when
        // no Prefer: outlook.timezone header is sent). This is the fix for
        // imported event times appearing in UTC rather than local household time.
        var originalTimezone = !string.IsNullOrWhiteSpace(evt.OriginalStartTimeZone)
            ? evt.OriginalStartTimeZone
            : evt.Start?.TimeZone;

        _logger.LogDebug(
            "Imported event timezone mapping. EventId={EventId} Subject={Subject} " +
            "GraphResponseTz={ResponseTz} GraphOriginalStartTz={OriginalStartTz} StoredTz={StoredTz} " +
            "StartsAtUtc={StartsAtUtc:o}",
            evt.Id, evt.Subject,
            evt.Start?.TimeZone, evt.OriginalStartTimeZone, originalTimezone,
            startsAt);

        return new ExternalCalendarProviderEvent(
            evt.Id ?? string.Empty,
            evt.ICalUId,
            evt.SeriesMasterId,
            title,
            startsAt,
            endsAt,
            evt.IsAllDay == true,
            evt.Location?.DisplayName,
            null,
            status,
            evt.WebLink,
            lastModified,
            isDeleted,
            originalTimezone);
    }

    /// <summary>
    /// Parses a Graph <c>dateTimeTimeZone.dateTime</c> string into a UTC <see cref="DateTime"/>.
    /// </summary>
    /// <remarks>
    /// Graph returns start/end as a split model: a bare local-time string (no offset, no Z) plus a
    /// separate <c>timeZone</c> field. Parsing the bare string with <see cref="DateTimeStyles.RoundtripKind"/>
    /// yields <see cref="DateTimeKind.Unspecified"/>, which Npgsql 6+ rejects for
    /// <c>timestamp with time zone</c> columns.
    ///
    /// When the caller does not send <c>Prefer: outlook.timezone</c>, Graph returns UTC by default.
    /// The <c>timeZone</c> field will be "UTC" in that case.
    /// </remarks>
    public static DateTime ParseGraphDateTimeAsUtc(string dateTime, string? timeZone)
    {
        var parsed = DateTime.Parse(
            dateTime,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces);

        if (string.IsNullOrWhiteSpace(timeZone) ||
            string.Equals(timeZone, "UTC", StringComparison.OrdinalIgnoreCase))
        {
            return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
        }

        // Non-UTC zone: interpret the local time in the supplied zone and convert to UTC.
        // TimeZoneInfo.FindSystemTimeZoneById accepts both IANA and Windows zone IDs on
        // .NET 6+ (cross-platform time zone support).
        TimeZoneInfo tz;
        try
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
        }
        catch (TimeZoneNotFoundException)
        {
            // Fall back to treating the value as UTC rather than propagating a mapping error.
            return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
        }

        var unspecified = DateTime.SpecifyKind(parsed, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecified, tz);
    }

    private static DateTime EnsureUtc(DateTime value)
        => value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value.ToUniversalTime(), DateTimeKind.Utc);

    // Produces a short stable hex digest of a link URL for structured logging.
    // Full delta/skip tokens are never emitted — they can be many kilobytes.
    private static string HashLink(string? url)
    {
        if (url is null) return "-";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(url));
        return Convert.ToHexString(hash[..4]).ToLowerInvariant();
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

        [JsonPropertyName("hexColor")]
        public string? HexColor { get; set; }
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

        // The timezone the event was AUTHORED in. Returned unconditionally by Graph
        // regardless of whether a Prefer: outlook.timezone header was sent.
        // This differs from Start.TimeZone which reflects the response wire format.
        [JsonPropertyName("originalStartTimeZone")]
        public string? OriginalStartTimeZone { get; set; }

        [JsonPropertyName("originalEndTimeZone")]
        public string? OriginalEndTimeZone { get; set; }
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
