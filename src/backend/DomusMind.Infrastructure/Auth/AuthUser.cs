namespace DomusMind.Infrastructure.Auth;

/// <summary>
/// Authentication identity. Separate from the household domain Member concept.
/// </summary>
public sealed class AuthUser
{
    public Guid UserId { get; init; }

    public string Email { get; set; } = default!;

    public string PasswordHash { get; set; } = default!;

    public DateTime CreatedAtUtc { get; init; }
}
