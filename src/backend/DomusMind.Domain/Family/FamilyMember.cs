using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Family.ValueObjects;

namespace DomusMind.Domain.Family;

public sealed class FamilyMember : Entity<MemberId>
{
    public MemberName Name { get; private set; }

    /// <summary>
    /// Optional preferred display name shown in the UI instead of the legal name.
    /// When null, <see cref="Name"/> is the display name.
    /// </summary>
    public string? PreferredName { get; private set; }

    public MemberRole Role { get; private set; }
    public bool IsManager { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public DateTime JoinedAtUtc { get; private set; }

    /// <summary>Primary contact phone number. Optional.</summary>
    public string? PrimaryPhone { get; private set; }

    /// <summary>Primary contact email (separate from login email). Optional.</summary>
    public string? PrimaryEmail { get; private set; }

    /// <summary>Short household note for this member (non-sensitive). Optional.</summary>
    public string? HouseholdNote { get; private set; }

    /// <summary>
    /// Links this member to an authenticated user. Only set for members who have a
    /// DomusMind account (typically adults who registered). Children and pets have no
    /// auth account and therefore no AuthUserId.
    /// </summary>
    public Guid? AuthUserId { get; private set; }

    private FamilyMember(MemberId id, MemberName name, MemberRole role, bool isManager, DateOnly? birthDate, DateTime joinedAtUtc, Guid? authUserId = null)
        : base(id)
    {
        Name = name;
        Role = role;
        IsManager = isManager;
        BirthDate = birthDate;
        JoinedAtUtc = joinedAtUtc;
        AuthUserId = authUserId;
    }

    // Backward-compatible overload
    internal static FamilyMember Create(MemberId id, MemberName name, MemberRole role, DateTime joinedAtUtc)
        => new(id, name, role, false, null, joinedAtUtc);

    // Extended overload
    internal static FamilyMember Create(MemberId id, MemberName name, MemberRole role, bool isManager, DateOnly? birthDate, DateTime joinedAtUtc, Guid? authUserId = null)
        => new(id, name, role, isManager, birthDate, joinedAtUtc, authUserId);

    internal void Update(MemberName name, MemberRole role, bool isManager, DateOnly? birthDate)
    {
        Name = name;
        Role = role;
        IsManager = isManager;
        BirthDate = birthDate;
    }

    /// <summary>Updates the lightweight profile fields: preferred name, contact phone/email, household note.</summary>
    internal void UpdateProfile(string? preferredName, string? primaryPhone, string? primaryEmail, string? householdNote)
    {
        PreferredName = string.IsNullOrWhiteSpace(preferredName) ? null : preferredName.Trim();
        PrimaryPhone = string.IsNullOrWhiteSpace(primaryPhone) ? null : primaryPhone.Trim();
        PrimaryEmail = string.IsNullOrWhiteSpace(primaryEmail) ? null : primaryEmail.Trim();
        HouseholdNote = string.IsNullOrWhiteSpace(householdNote) ? null : householdNote.Trim();
    }

    /// <summary>
    /// Returns the effective display name: <see cref="PreferredName"/> if set, otherwise <see cref="Name"/>.
    /// </summary>
    public string DisplayName => !string.IsNullOrWhiteSpace(PreferredName) ? PreferredName : Name.Value;

    /// <summary>Links this member to an existing auth user account.</summary>
    internal void LinkAccount(Guid authUserId)
    {
        AuthUserId = authUserId;
    }

#pragma warning disable CS8618
    // EF Core parameterless constructor
    private FamilyMember() : base(default) { }
#pragma warning restore CS8618
}
