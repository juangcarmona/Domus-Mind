namespace DomusMind.Contracts.Family;

/// <summary>Access status of a member's linked auth account.</summary>
public enum MemberAccessStatus
{
    /// <summary>No login account has been provisioned.</summary>
    NoAccess,
    /// <summary>Account exists but the member has never logged in (provisioned / invited, awaiting first login).</summary>
    InvitedOrProvisioned,
    /// <summary>Account exists and the member has logged in before, but must change the password.</summary>
    PasswordResetRequired,
    /// <summary>Account exists, active and in normal use.</summary>
    Active,
    /// <summary>Account has been disabled by an admin.</summary>
    Disabled,
}

public sealed record FamilyMemberResponse(
    Guid MemberId,
    Guid FamilyId,
    string Name,
    string Role,
    bool IsManager,
    DateOnly? BirthDate,
    DateTime JoinedAtUtc,
    Guid? AuthUserId,
    MemberAccessStatus AccessStatus,
    string? LinkedEmail);
