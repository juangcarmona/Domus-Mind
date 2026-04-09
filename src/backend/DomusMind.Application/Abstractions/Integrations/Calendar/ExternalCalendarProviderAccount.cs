namespace DomusMind.Application.Abstractions.Integrations.Calendar;

/// <summary>
/// The result of an OAuth code exchange for a provider account.
/// </summary>
public sealed record ExternalCalendarProviderAccount(
    string ProviderAccountId,
    string AccountEmail,
    string? TenantId,
    string EncryptedRefreshToken,
    string GrantedScopes);
