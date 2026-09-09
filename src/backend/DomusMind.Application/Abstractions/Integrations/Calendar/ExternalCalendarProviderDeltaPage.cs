namespace DomusMind.Application.Abstractions.Integrations.Calendar;

/// <summary>
/// A page of delta results from a provider calendar delta query.
/// </summary>
public sealed record ExternalCalendarProviderDeltaPage(
    IReadOnlyCollection<ExternalCalendarProviderEvent> Events,
    string? NextDeltaToken,
    bool IsLastPage);
