namespace DomusMind.Application.Abstractions.Integrations.Calendar;

/// <summary>
/// Handles the OAuth code exchange and provider token lifecycle for external calendar connections.
/// </summary>
public interface IExternalCalendarAuthService
{
    /// <summary>
    /// Exchanges an authorization code for provider account details and encrypted refresh material.
    /// </summary>
    Task<ExternalCalendarProviderAccount> ExchangeAuthorizationCodeAsync(
        string authorizationCode,
        string redirectUri,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a valid access token for the connection, refreshing it if necessary.
    /// Returns null if the token cannot be refreshed (auth expired).
    /// </summary>
    Task<string?> GetAccessTokenAsync(
        Guid connectionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes stored auth metadata for a connection on disconnect.
    /// </summary>
    Task RevokeAsync(Guid connectionId, CancellationToken cancellationToken = default);
}
