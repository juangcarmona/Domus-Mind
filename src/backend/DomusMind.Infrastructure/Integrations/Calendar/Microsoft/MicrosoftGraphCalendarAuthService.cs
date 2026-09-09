using DomusMind.Application.Abstractions.Integrations.Calendar;
using DomusMind.Domain.Calendar.ExternalConnections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System.Security.Cryptography;
using System.Text;

namespace DomusMind.Infrastructure.Integrations.Calendar.Microsoft;

/// <summary>
/// Handles Microsoft delegated OAuth for external calendar connections.
/// 
/// Important:
/// - This implementation uses MSAL user token cache persistence.
/// - The shadow property currently named "EncryptedRefreshToken" stores the encrypted
///   serialized MSAL user token cache, not a raw refresh token.
/// - Access tokens are cached separately for short-circuit reads, but silent acquisition
///   always relies on the MSAL user token cache.
/// </summary>
public sealed class MicrosoftGraphCalendarAuthService : IExternalCalendarAuthService
{
    private const string CachedAccessTokenProperty = "CachedAccessToken";
    private const string AccessTokenExpiresAtUtcProperty = "AccessTokenExpiresAtUtc";
    private const string EncryptedTokenCacheProperty = "EncryptedRefreshToken";

    private static readonly string[] DelegatedScopes =
    [
        "https://graph.microsoft.com/Calendars.Read"
    ];

    private readonly DomusMind.Infrastructure.Persistence.DomusMindDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MicrosoftGraphCalendarAuthService> _logger;

    public MicrosoftGraphCalendarAuthService(
        DomusMind.Infrastructure.Persistence.DomusMindDbContext dbContext,
        IConfiguration configuration,
        ILogger<MicrosoftGraphCalendarAuthService> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ExternalCalendarProviderAccount> ExchangeAuthorizationCodeAsync(
        string authorizationCode,
        string redirectUri,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(authorizationCode))
            throw new InvalidOperationException("Authorization code is required.");

        if (string.IsNullOrWhiteSpace(redirectUri))
            throw new InvalidOperationException("OAuth redirect URI is required.");

        var clientId = GetRequiredSetting("MicrosoftGraph:ClientId");
        var clientSecret = GetRequiredSetting("MicrosoftGraph:ClientSecret");
        var tenantId = _configuration["MicrosoftGraph:TenantId"] ?? "common";

        var tokenCacheEnvelope = new TokenCacheEnvelope();
        var app = BuildConfidentialClient(clientId, clientSecret, tenantId, redirectUri, tokenCacheEnvelope);

        AuthenticationResult result;
        try
        {
            result = await app
                .AcquireTokenByAuthorizationCode(DelegatedScopes, authorizationCode)
                .ExecuteAsync(cancellationToken);
        }
        catch (MsalException ex)
        {
            _logger.LogWarning(
                ex,
                "Microsoft auth code exchange failed. RedirectUri={RedirectUri}",
                redirectUri);

            throw new InvalidOperationException(
                "Authorization code exchange failed: " + ex.Message,
                ex);
        }

        if (result.Account is null)
            throw new InvalidOperationException("Microsoft authorization succeeded but no account was returned.");

        if (string.IsNullOrWhiteSpace(tokenCacheEnvelope.SerializedCache))
            throw new InvalidOperationException("Microsoft authorization succeeded but token cache serialization produced no data.");

        var providerAccountId = result.Account.HomeAccountId?.Identifier ?? result.UniqueId;
        var accountEmail = result.Account.Username ?? string.Empty;
        var effectiveTenantId = result.TenantId;
        var grantedScopes = string.Join(" ", result.Scopes);

        var encryptedSerializedTokenCache = Encrypt(tokenCacheEnvelope.SerializedCache);

        return new ExternalCalendarProviderAccount(
            providerAccountId,
            accountEmail,
            effectiveTenantId,
            encryptedSerializedTokenCache,
            grantedScopes);
    }

    public async Task<string?> GetAccessTokenAsync(
        Guid connectionId,
        CancellationToken cancellationToken = default)
    {
        var connection = await _dbContext
            .Set<ExternalCalendarConnection>()
            .FirstOrDefaultAsync(
                c => c.Id == ExternalCalendarConnectionId.From(connectionId),
                cancellationToken);

        if (connection is null)
            return null;

        var cachedToken = _dbContext.Entry(connection)
            .Property<string?>(CachedAccessTokenProperty)
            .CurrentValue;

        var expiresAtUtc = _dbContext.Entry(connection)
            .Property<DateTime?>(AccessTokenExpiresAtUtcProperty)
            .CurrentValue;

        if (!string.IsNullOrWhiteSpace(cachedToken) &&
            expiresAtUtc.HasValue &&
            expiresAtUtc.Value > DateTime.UtcNow.AddMinutes(5))
        {
            return cachedToken;
        }

        var encryptedSerializedTokenCache = _dbContext.Entry(connection)
            .Property<string?>(EncryptedTokenCacheProperty)
            .CurrentValue;

        if (string.IsNullOrWhiteSpace(encryptedSerializedTokenCache))
        {
            _logger.LogInformation(
                "No Microsoft token cache is stored for connection {ConnectionId}. Reconnect is required.",
                connectionId);

            return null;
        }

        try
        {
            var serializedTokenCache = Decrypt(encryptedSerializedTokenCache);
            var tokenCacheEnvelope = new TokenCacheEnvelope(serializedTokenCache);

            var clientId = GetRequiredSetting("MicrosoftGraph:ClientId");
            var clientSecret = GetRequiredSetting("MicrosoftGraph:ClientSecret");
            var tenantId = _configuration["MicrosoftGraph:TenantId"] ?? "common";

            // Redirect URI is not required for silent acquisition, but MSAL builder
            // is kept otherwise identical.
            var app = BuildConfidentialClient(
                clientId,
                clientSecret,
                tenantId,
                redirectUri: null,
                tokenCacheEnvelope);

            var account = await ResolveAccountAsync(app, connection);
            if (account is null)
            {
                _logger.LogInformation(
                    "No cached MSAL account could be resolved for external calendar connection {ConnectionId}. Reconnect is required.",
                    connectionId);

                return null;
            }

            AuthenticationResult result;
            try
            {
                result = await app
                    .AcquireTokenSilent(DelegatedScopes, account)
                    .ExecuteAsync(cancellationToken);
            }
            catch (MsalUiRequiredException ex)
            {
                _logger.LogInformation(
                    ex,
                    "Silent Microsoft token acquisition requires reauthentication for connection {ConnectionId}.",
                    connectionId);

                return null;
            }

            _dbContext.Entry(connection).Property<string?>(CachedAccessTokenProperty).CurrentValue = result.AccessToken;
            _dbContext.Entry(connection).Property<DateTime?>(AccessTokenExpiresAtUtcProperty).CurrentValue = result.ExpiresOn.UtcDateTime;
            _dbContext.Entry(connection).Property<string?>(EncryptedTokenCacheProperty).CurrentValue =
                Encrypt(tokenCacheEnvelope.SerializedCache ?? serializedTokenCache);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return result.AccessToken;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(
                ex,
                "Failed to acquire delegated Microsoft access token for connection {ConnectionId}.",
                connectionId);

            return null;
        }
    }

    public async Task RevokeAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        var connection = await _dbContext
            .Set<ExternalCalendarConnection>()
            .FirstOrDefaultAsync(
                c => c.Id == ExternalCalendarConnectionId.From(connectionId),
                cancellationToken);

        if (connection is null)
            return;

        _dbContext.Entry(connection).Property<string?>(EncryptedTokenCacheProperty).CurrentValue = null;
        _dbContext.Entry(connection).Property<string?>(CachedAccessTokenProperty).CurrentValue = null;
        _dbContext.Entry(connection).Property<DateTime?>(AccessTokenExpiresAtUtcProperty).CurrentValue = null;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IConfidentialClientApplication BuildConfidentialClient(
        string clientId,
        string clientSecret,
        string tenantId,
        string? redirectUri,
        TokenCacheEnvelope tokenCacheEnvelope)
    {
        var builder = ConfidentialClientApplicationBuilder
            .Create(clientId)
            .WithClientSecret(clientSecret)
            .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
            .WithLegacyCacheCompatibility(false);

        if (!string.IsNullOrWhiteSpace(redirectUri))
        {
            builder = builder.WithRedirectUri(redirectUri);
        }

        var app = builder.Build();
        RegisterTokenCacheSerialization(app.UserTokenCache, tokenCacheEnvelope);
        return app;
    }

    private void RegisterTokenCacheSerialization(
        ITokenCache tokenCache,
        TokenCacheEnvelope envelope)
    {
        tokenCache.SetBeforeAccess(args =>
        {
            if (!string.IsNullOrWhiteSpace(envelope.SerializedCache))
            {
                args.TokenCache.DeserializeMsalV3(Encoding.UTF8.GetBytes(envelope.SerializedCache), shouldClearExistingCache: true);
            }
        });

        tokenCache.SetAfterAccess(args =>
        {
            if (!args.HasStateChanged)
                return;

            envelope.SerializedCache = Encoding.UTF8.GetString(args.TokenCache.SerializeMsalV3());
        });
    }

    private async Task<IAccount?> ResolveAccountAsync(
        IConfidentialClientApplication app,
        ExternalCalendarConnection connection)
    {
        var persistedProviderAccountId = connection.ProviderAccountId;

        if (string.IsNullOrWhiteSpace(persistedProviderAccountId))
            return null;

        return await app.GetAccountAsync(persistedProviderAccountId).ConfigureAwait(false);
    }

    private string GetRequiredSetting(string key) =>
        _configuration[key] ?? throw new InvalidOperationException($"{key} is not configured.");

    private string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        var keyBase64 = _configuration["MicrosoftGraph:TokenEncryptionKey"];
        if (string.IsNullOrWhiteSpace(keyBase64))
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));

        var key = Convert.FromBase64String(keyBase64);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var result = new byte[aes.IV.Length + cipherBytes.Length];
        aes.IV.CopyTo(result, 0);
        cipherBytes.CopyTo(result, aes.IV.Length);

        return Convert.ToBase64String(result);
    }

    private string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return encryptedText;

        var keyBase64 = _configuration["MicrosoftGraph:TokenEncryptionKey"];
        if (string.IsNullOrWhiteSpace(keyBase64))
            return Encoding.UTF8.GetString(Convert.FromBase64String(encryptedText));

        var encryptedBytes = Convert.FromBase64String(encryptedText);
        var key = Convert.FromBase64String(keyBase64);

        using var aes = Aes.Create();
        aes.Key = key;

        var ivLength = aes.BlockSize / 8;
        var iv = encryptedBytes[..ivLength];
        var cipherBytes = encryptedBytes[ivLength..];
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }

    private sealed class TokenCacheEnvelope
    {
        public TokenCacheEnvelope()
        {
        }

        public TokenCacheEnvelope(string serializedCache)
        {
            SerializedCache = serializedCache;
        }

        public string? SerializedCache { get; set; }
    }
}