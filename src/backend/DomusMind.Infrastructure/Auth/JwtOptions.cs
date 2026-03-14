namespace DomusMind.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SigningKey { get; init; } = default!;

    public string Issuer { get; init; } = "domusmind";

    public string Audience { get; init; } = "domusmind";

    public int ExpiryMinutes { get; init; } = 60;
}
