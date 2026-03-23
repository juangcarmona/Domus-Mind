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

    /// <summary>
    /// When true, the user must change their password on next login.
    /// Set to true for invited members who have a temporary password.
    /// </summary>
    public bool MustChangePassword { get; set; }

    /// <summary>Optional display name chosen by admin at provisioning time.</summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// When true the account is disabled and login is refused.
    /// Refresh tokens are revoked when this is set.
    /// </summary>
    public bool IsDisabled { get; set; }

    /// <summary>UTC timestamp of the last successful password change.</summary>
    public DateTime? PasswordChangedAtUtc { get; set; }

    /// <summary>UTC timestamp of the last successful login. Null until the first login occurs.</summary>
    public DateTime? LastLoginAtUtc { get; set; }

    /// <summary>
    /// The household family member this auth user is linked to.
    /// Null for the bootstrap admin and for users created before the
    /// member-provisioning flow was introduced.
    /// </summary>
    public Guid? MemberId { get; set; }

    /// <summary>UserId of the admin who provisioned this account, if applicable.</summary>
    public Guid? CreatedByUserId { get; set; }
}
