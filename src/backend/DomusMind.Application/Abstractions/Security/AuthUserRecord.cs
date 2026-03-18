namespace DomusMind.Application.Abstractions.Security;

/// <summary>
/// Lightweight auth user record used by the application layer.
/// Separate from the household domain Member concept.
/// </summary>
public sealed record AuthUserRecord(Guid UserId, string Email, string PasswordHash, bool MustChangePassword = false);
