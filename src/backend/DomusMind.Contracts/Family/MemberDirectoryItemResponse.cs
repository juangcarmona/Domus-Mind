namespace DomusMind.Contracts.Family;

/// <summary>
/// A single member entry for the household member directory.
/// Includes server-computed UI-projection fields so the client does not need
/// to re-derive access rules from raw data.
/// </summary>
public sealed record MemberDirectoryItemResponse(
    Guid MemberId,
    Guid FamilyId,
    string Name,
    /// <summary>Optional preferred display name. When set, use this instead of Name.</summary>
    string? PreferredName,
    string Role,
    bool IsManager,
    DateOnly? BirthDate,
    DateTime JoinedAtUtc,
    Guid? AuthUserId,
    MemberAccessStatus AccessStatus,
    string? LinkedEmail,

    /// <summary>True when this member is the currently authenticated user.</summary>
    bool IsCurrentUser,

    /// <summary>True when the member has a linked login account.</summary>
    bool HasAccount,

    /// <summary>True when the requesting user (a manager) can provision access for this member.</summary>
    bool CanGrantAccess,

    /// <summary>True when the requesting user may edit this member (is a manager or is this member).</summary>
    bool CanEdit,

    /// <summary>First letter of the effective display name, upper-cased, for use as an avatar placeholder.</summary>
    string AvatarInitial);
