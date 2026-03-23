namespace DomusMind.Contracts.Family;

/// <summary>
/// Detail view of a single family member — Phase 2 shape.
/// Extends the directory item with the lightweight profile seam:
/// preferred name, contacts, and household note.
/// </summary>
public sealed record MemberDetailResponse(
    Guid MemberId,
    Guid FamilyId,
    string Name,
    string? PreferredName,
    string Role,
    bool IsManager,
    DateOnly? BirthDate,
    DateTime JoinedAtUtc,
    Guid? AuthUserId,
    MemberAccessStatus AccessStatus,
    string? LinkedEmail,
    DateTime? LastLoginAtUtc,
    bool IsCurrentUser,
    bool HasAccount,
    bool CanGrantAccess,
    bool CanEdit,
    string AvatarInitial,
    // ── Phase 2 profile seam ─────────────────────────────────────────────────
    string? PrimaryPhone,
    string? PrimaryEmail,
    string? HouseholdNote);
